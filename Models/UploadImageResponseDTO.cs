using System;

namespace NASATest2018.Models
{
    [Serializable]
    public class UploadImageResponseDTO: ResponseDTO
    {
        public long TotalUploadedSize {get; set;}

        public string GeneratedImageName {get; set;}
    }
}