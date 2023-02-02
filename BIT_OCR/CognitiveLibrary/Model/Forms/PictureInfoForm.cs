using System.ComponentModel.DataAnnotations;

namespace CognitiveLibrary.Model
{
    /// <summary>
    ///     This class represents all that is needed in order to create a new instance of the <see cref="AnalyseResultEntity"/> class.
    /// </summary>
    public class PictureInfoForm
    {
        [Required] public int Id { get; set; }
        [Required] public string? FileName { get; set; }
        [Required] public string FileUrl { get; set; }
    }
}
