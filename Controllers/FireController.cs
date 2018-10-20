using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NASATest2018.Models;

namespace NASATest2018.Controllers
{
    public class FireController : Controller
    {

        [HttpPost]
        public JsonResult ReportTheFire(ReportTheFireParameterDTO param)
        {
            var response = new ReportTheFireResponseDTO
            {
                ReportId = 42
            };
            return new JsonResult(response);
        }


        [HttpPost]
        public JsonResult GetSecretUserId()
        {
            var response = new GetSecretUserIdResponseDTO
            {
               SecretUserId = "bla-bla-bla-42-secret-id"
            };
            return new JsonResult(response);
        }
       
    }
}
