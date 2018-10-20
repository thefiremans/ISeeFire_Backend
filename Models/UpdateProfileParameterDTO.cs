using System;

namespace NASATest2018.Models
{
    [Serializable]
    public class UpdateProfileParameterDTO: ParameterDTO
    {
        public string Phone {get; set;}
        public string Name {get; set;}
    }
}