using AutoMapper;
using BiT.Central.Core.Logging;
using BiT.Central.Core.Mvc;
using BiT.Central.Core.Mvc.Query;
using BiT.Central.OCR.Api.Models;
using BiT.Central.OCR.Api.Models.Views;
using CognitiveLibrary;
using CognitiveLibrary.Model;
using CognitiveLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BiT.Central.OCR.Api.Controllers
{
    /// <summary>
    ///     The OCRController holds all routes that operate on <see cref="AnalyseResultEntity"/> objects and it's children.
    ///     The Controller inherits from <see cref="BitCentralController"/>, this means that is has some default mechanisms that need explanation.
    /// </summary>
    [ApiExplorerSettings(GroupName = "AnalyseResults")]
    public class OCRController : BitCentralController
    {
        private ILogger<OCRController> _logger;
        private Serilog.ILogger logger;
        private IMapper _mapper;
        private BitCentralErrorFactory _errorFactory;
        private BitCentralValidator _validator;
        private IConfiguration _configuration;
        private TableService _tableService;
        private BlobService _blobService;
        private CognitiveAnalysisService _cognitiveAnalysisService;
        private RequestAnalyseService _requestAnalyseService;
        private StoreService _storeService;
        private SearchService _searchService;

        public OCRController(
            ILogger<OCRController> logger,
            IMapper mapper,
            BitCentralErrorFactory errorFactory,
            BitCentralValidator validator,
            TableService tableService,
            StoreService storeService,
            BlobService blobService,
            CognitiveAnalysisService cognitiveAnalysisService,
            RequestAnalyseService requestAnalyseService,
            SearchService searchService,
            IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _errorFactory = errorFactory;
            _validator = validator;
            _configuration = configuration;
            _tableService = tableService;
            _storeService = storeService;
            _blobService = blobService;
            _cognitiveAnalysisService = cognitiveAnalysisService;
            _requestAnalyseService = requestAnalyseService;
            _searchService = searchService;

        }

        //TODO: optimize API request handeling through Azure Function to scale dependence of Cognitive Services.
        /// <summary>
        ///     Retrieve a table entity based on id
        /// </summary>
        /// 
        /// <remarks>
        ///     This endpoint requires no permission. <br></br> 
        /// </remarks>
        /// 
        /// <param name="resultId">Id of the table entity</param>
        /// <response code="200">Returned when the request is valid..</response>
        ///// <response code="401">Returned when the given token is invalid or incomplete.</response>
        ///// <response code="403">Returned when the given token does not permit access to the current endpoint.</response>
        /// 
        [HttpGet("analyseResult/{resultId}")]
        [ProducesResponseType(typeof(GetResultView), 200)]
        [ProducesResponseType(typeof(BitCentralError<string>), 401)]
        [ProducesResponseType(typeof(BitCentralError<string>), 403)]
        [ProducesResponseType(typeof(BitCentralError<string>), 404)]
        //[BitCentralAuthentication( // A BiT-Central Token is required for acessing this endpoint
        //    //Permissions: new string[] {"read-orcrresult"}, // The token given in the Authorization header requires the following permissions
        //    AddRelationSupport: false, // Relation tokens can be used to access this endpoint, disabled by default
        //    ForceConcernParam: false,  // A Concern query parameter is required (and used) when a wildcard token is given
        //    ForceCompanyParam: false,
        //    ForceRelationParam: false
        //)]
        public async Task<ActionResult<GetResultView>> AnalyseResult_Get(
            [FromRoute] string resultId)
        {
            var logContext = new BitCentralLogContext()
                .AddHttp(HttpContext)
                .AddToken(HttpContext);

            using (logContext.Build())
            {
                var entity = await _tableService.GetTableEntity(resultId);
                if (entity == null)
                {
                    return NotFound("Not found");
                }

                
                var blobClient = _blobService.CreateBlobClient(entity.PictureName);
                if (!blobClient.Exists())
                {
                    var view = _mapper.Map<AnalyseResultView>(entity);
                    return Ok(view);
                }

                var blob = await blobClient.OpenReadAsync();
                StreamReader streamReader = new StreamReader(blob);
                var resultString = streamReader.ReadToEnd();

                GetResultView resultView = new GetResultView()
                {
                    TableEntityView = _mapper.Map<AnalyseResultView>(entity),
                    FilterResult = JsonConvert.DeserializeObject<FilterdResult>(resultString)
                };
                return Ok(resultView);
            }
        }

        /// <summary>
        ///     Retrieve all analyseResults based on a range of filters.
        /// </summary>
        /// 
        /// <remarks>
        ///     This endpoint requires no permission. <br></br>
        /// </remarks>
        /// <param name="pagination">The page number and size required to support pagination, all items are returned if not given.</param>
        /// <param name="sorting">Support for sorting of entities, any property on the entity can be used to sort.</param>
        /// <param name="filters">Support for filtering of entities, any property on the entity can be used to sort.</param>
        /// <response code="200">Returned when the request is valid.</response>
        /// <response code="204">Returned when the request is valid but no objects matching the filters or concern/company restrictions could be found.</response>
        /// <response code="401">Returned when the given token is invalid or incomplete.</response>
        /// <response code="403">Returned when the given token does not permit access to the current endpoint.</response>
        /// <response code="404">Returned when the given identifier does not lead to an order.</response>
        /// <response code="409">Returned when the given identifier leads to multiple orders.</response>
        /// 
        [HttpGet("analyseResults")]
        [ProducesResponseType(typeof(AnalyseResultView), 200)]
        [ProducesResponseType(204)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 401)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 403)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 409)]
        //[BitCentralAuthentication(
        //    Permissions: new[] { "read-orders@order" },
        //    AddRelationSupport: true,
        //    ForceConcernParam: true,
        //    ForceCompanyParam: true,
        //    ForceRelationParam: true
        //)]
        public async Task<ActionResult> AnalyseResults_Index(
            [FromQuery] PaginationQueryParameters pagination,
            [FromQuery] SortingQueryParameters sorting,
            [FromQuery] List<Filter> filters)
        {
            var logContext = new BitCentralLogContext()
                .AddHttp(HttpContext)
                .AddToken(HttpContext)
                .AddIndex(pagination, sorting, filters);

            using (logContext.Build())
            {
                var entities = await _tableService.GetAllTableEntities(pagination, sorting, filters);

                _logger.LogInformation($"current entitie displayed: {entities.Items.Count()}");

                var view = _mapper.Map<PagedListPage<AnalyseResultView>>(entities);
                return Ok(view);
            }
        }

        /// <summary>
        ///     Create a new analyseResult entity and analyse picture from url through Cognitive Services
        /// </summary>
        /// 
        /// <remarks>
        ///     This endpoint requires the `create-analyseResult@analyseResults` permission. <br></br>
        /// </remarks>
        /// 
        /// <param name="form">The template with values to base the analyseResult on.</param>
        /// 
        /// <response code="201">Returned when the request is valid and the resource has been created.</response>
        /// <response code="400">Returned when the model validation on the form has failed, or an illegal parameter/value has been given.</response>
        /// <response code="401">Returned when the given token is invalid or incomplete.</response>
        /// <response code="403">Returned when the given token does not permit access to the current endpoint.</response>
        /// <response code="409">Returned when the an order with the given combination of Concern and BitId already exists.</response>
        /// 
        [HttpPost("analyseResult")]
        [ProducesResponseType(typeof(AnalyseResultView), 201)]
        [ProducesResponseType(typeof(BitCentralError<List<string>>), 400)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 401)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 403)]
        //[BitCentralAuthentication(
        //    Permissions: new string[] {},
        //    AddRelationSupport: false, // Relations may not create orders
        //    ForceStrictWildcardMode: false,
        //    ForceConcernParam: false
        //)]
        public async Task<ActionResult> AnalysedResult_Create(
            [FromBody] PictureInfoForm form)
        {
            var logContext = new BitCentralLogContext()
                .AddHttp(HttpContext)
                .AddToken(HttpContext);

            using (logContext.Build())
            {
                
                var postResponse = await _cognitiveAnalysisService.PostReadApiRequest(form);

                ReadResultResponse readResultResponse = null;
                if (!postResponse.Status.Equals("Failed"))
                {
                    readResultResponse = await _cognitiveAnalysisService.GetResultAsJson(postResponse.Message);

                    
                    var resultEntity = _storeService.StoreResultResponse(form, readResultResponse, _tableService, _blobService);

                    var view = _mapper.Map<AnalyseResultView>(resultEntity.Result);
                    return Created($"/analyseResults/{resultEntity.Id}", view);
                }
                else
                {
                    return BadRequest(postResponse.Message);
                }
            }
        }

        /// <summary>
        ///     Update an existing analyseResult based on partitionKey and rowKey
        /// </summary>
        /// 
        /// <remarks>
        ///     This endpoint requires the `update-analyseResults@results` permission. <br></br>
        /// </remarks>
        /// 
        /// <param name="analyseResultEntity">The content of nieuw analyResultEntity </param>
        /// 
        /// <response code="200">Returned when the request is valid and the resource has been updated.</response>
        /// <response code="400">Returned when the model validation on the form has failed, or an illegal parameter/value has been given.</response>
        /// <response code="401">Returned when the given token is invalid or incomplete.</response>
        /// <response code="403">Returned when the given token does not permit access to the current endpoint.</response>
        /// <response code="404">Returned when the given identifier does not lead to an order.</response>
        /// <response code="409">Returned when the given identifier leads to multiple orders.</response>
        [HttpPut("analyseResult")]
        [ProducesResponseType(typeof(AnalyseResultView), 200)]
        [ProducesResponseType(typeof(BitCentralError<string>), 400)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 401)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 403)]
        [ProducesResponseType(typeof(BitCentralError<string>), 404)]
        [ProducesResponseType(typeof(BitCentralError<string>), 409)]
        //[BitCentralAuthentication(
        //    Permissions: new[] { "delete-orders@order" },
        //    AddRelationSupport: false,
        //    ForceConcernParam: true,
        //    ForceCompanyParam: true,
        //    ForceRelationParam: true
        //)]
        public async Task<ActionResult> AnalyseResult_Put([FromBody] AnalysisResultEntity analyseResultEntity)
        {
            var logContext = new BitCentralLogContext()
                .AddHttp(HttpContext)
                .AddToken(HttpContext);

            using (logContext.Build())
            {
                var entity = await _tableService.GetTableEntity(analyseResultEntity.Id.ToString());
                if (entity == null)
                {
                    return NotFound("Entity not found");
                }

                var updatedEntity = new AnalysisResultEntity()
                {
                    Id = (analyseResultEntity.Id != null) ? analyseResultEntity.Id : entity.Id,
                    PictureName = (analyseResultEntity.PictureName != null) ? analyseResultEntity.PictureName : entity.PictureName,
                    Status = (analyseResultEntity.Status != null) ? analyseResultEntity.Status : entity.Status,
                    AnalyseResultPath = (analyseResultEntity.AnalyseResultPath != null) ? analyseResultEntity.AnalyseResultPath : entity.AnalyseResultPath,
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey
                };
                //if (analyseResultEntity.PartitionKey == null || analyseResultEntity.RowKey == null)
                //{
                //    return BadRequest("Missing partitionkey or rowkey");
                //}
                await _tableService.UpdateTableEntity(updatedEntity);
                var view = _mapper.Map<AnalyseResultView>(updatedEntity);
                return Ok(view);
            }
        }

        /// <summary>
        ///     Delete a specific analyseResult based on id.
        /// </summary>
        /// 
        /// <remarks>
        ///     This endpoint requires the `delete-analyseResult@analyseResults` permission. <br></br>
        /// </remarks>
        /// <param name="resultId">The id of the table entity which need to be deleted</param>
        /// 
        /// <response code="200">Returned when the request is valid and the resources has been deleted.</response>
        /// <response code="401">Returned when the given token is invalid or incomplete.</response>
        /// <response code="403">Returned when the given token does not permit access to the current endpoint.</response>
        /// <response code="404">Returned when the given identifier does not lead to an order.</response>
        /// <response code="409">Returned when the given identifier leads to multiple orders.</response>
        /// 
        [HttpDelete("analyseResult/{resultId}")]
        [ProducesResponseType(typeof(AnalyseResultView), 200)]
        [ProducesResponseType(typeof(BitCentralError<string>), 401)]
        [ProducesResponseType(typeof(BitCentralError<string>), 403)]
        [ProducesResponseType(typeof(BitCentralError<string>), 404)]
        [ProducesResponseType(typeof(BitCentralError<string>), 409)]
        //[BitCentralAuthentication(
        //    Permissions: new[] { "delete-orders@order" },
        //    AddRelationSupport: false,
        //    ForceConcernParam: true,
        //    ForceCompanyParam: true,
        //    ForceRelationParam: true
        //)]
        public async Task<ActionResult> AnalyseResult_Delete([FromRoute] string resultId)
        {
            var logContext = new BitCentralLogContext()
                .AddHttp(HttpContext)
                .AddToken(HttpContext);

            using (logContext.Build())
            {
                var entity = _tableService.GetTableEntity(resultId).Result;
                if (entity == null)
                {
                    return NotFound("Entity not found");
                }
                await _tableService.DeleteTableEntity(resultId);
                await _blobService.DeleteBlob(entity.PictureName);
                var view = _mapper.Map<AnalyseResultView>(entity);

                return Ok(view);
            }
        }

        /// <summary>
        ///     Search relavant blobs based on keywords.
        /// </summary>
        /// 
        /// <param name="searchRequest">The keyword to search relavant blobs</param>
        ///  
        /// <response code="200">Returned when the request is valid and the resources has been deleted.</response>
        /// <response code="401">Returned when the given token is invalid or incomplete.</response>
        /// <response code="403">Returned when the given token does not permit access to the current endpoint.</response>
        /// 
        [HttpPost("analyseResult/search")]
        [ProducesResponseType(typeof(SearchItems), 200)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 401)]
        //[ProducesResponseType(typeof(BitCentralError<string>), 403)]
        //[BitCentralAuthentication(
        //    Permissions: new[] { "delete-orders@order" },
        //    AddRelationSupport: false,
        //    ForceConcernParam: true,
        //    ForceCompanyParam: true,
        //    ForceRelationParam: true
        //)]
        public async Task<ActionResult> AnalyseResult_Search([FromBody] SearchRequest searchRequest,
            [FromQuery] PaginationQueryParameters pagination,
            [FromQuery] SortingQueryParameters sorting,
            [FromQuery] List<Filter> filters)
        {
            var logContext = new BitCentralLogContext()
                .AddHttp(HttpContext)
                .AddToken(HttpContext);

            using (logContext.Build())
            {
                var view = await _searchService.SearchItem(searchRequest.Keyword, pagination, sorting, filters);
                if (view == null)
                {
                    return NotFound();
                }
                return Ok(view);
            }
        }
    }
}
