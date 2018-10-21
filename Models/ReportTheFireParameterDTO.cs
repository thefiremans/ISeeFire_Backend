using System;

namespace NASATest2018.Models
{
    [Serializable]
    public class ReportTheFireParameterDTO: ParameterDTO
    {
        public string TextOfComment {get; set;}
        public decimal Longitude {get; set;}
        public decimal Latitude {get; set;}
        public decimal Distance {get; set;}
    }
}