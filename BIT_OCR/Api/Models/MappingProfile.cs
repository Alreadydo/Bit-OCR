using AutoMapper;
using BiT.Central.OCR.Api.Models.Views;
using CognitiveLibrary.Model;

namespace BiT.Central.OCR.Api.Models
{

    /// <summary>
    ///     Mapping profile for all the classes in this project.
    /// </summary>
    public class MappingProfile : Profile
    {
        /*
         * See the following link for more information on how to use Automapper
         * https://docs.automapper.org/en/stable/Getting-started.html
         */
        public MappingProfile()
        {
            /*
             * Automapper maps properties with equal names automaticly. 
             * They do not have to be defined with a <c>.ForMember()</c> call.
             */
            CreateMap<PictureInfoForm, AnalysisResultEntity>();
            CreateMap<AnalysisResultEntity, AnalyseResultView>();
            //CreateMap<SearchItem, SearchItem>();

        }
    }
}
