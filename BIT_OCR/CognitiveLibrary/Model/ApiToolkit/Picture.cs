using System;

namespace CognitiveLibrary.Model.ApiToolkit
{
    public class Picture
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }
        public string FileName { get; set; }
        public string FileExtention { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int HorizontalDPI { get; set; }
        public int VerticalDPI { get; set; }
        public int BitDepth { get; set; }
        public DateTime PictureTaken { get; set; }
        public int PictureSetId { get; set; }
        public Angle Angle { get; set; }
    }
}
