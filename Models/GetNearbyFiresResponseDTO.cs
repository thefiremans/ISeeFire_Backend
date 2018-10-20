using System;

namespace NASATest2018.Models
{
    [Serializable]
    public class GetNearbyFiresResponseDTO: ParameterDTO
    {
        public decimal Latitude {get; set;}
        public decimal Longitude {get; set;}
        public string PhotoUrl {get; set;}
        public bool IsOwner {get; set;}
        public bool IsNasa {get; set;}
        public decimal Confidence {get; set;}
    }
}