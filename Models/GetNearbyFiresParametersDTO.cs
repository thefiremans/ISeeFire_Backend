using System;

namespace NASATest2018.Models
{
    [Serializable]
    public class GetNearbyFiresParametersDTO: ParameterDTO
    {
        public decimal Latitude {get; set;}
        public decimal Longitude {get; set;}
        public decimal Distance { get; set;}
    }
}