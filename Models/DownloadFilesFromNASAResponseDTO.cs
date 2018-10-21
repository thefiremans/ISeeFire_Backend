using System;

namespace NASATest2018.Models
{
    [Serializable]
    public class DownloadFilesFromNASAResponseDTO: ResponseDTO
    {
        public int RowsImported { get; set;}

        public string PathProcessed {get; set;}
        
    }
}