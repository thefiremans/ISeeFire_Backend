﻿using System;
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
        private bool checkSecretUserId(string SecretUserId)
        {
            return true;
        }

        [HttpPost]
        public JsonResult ReportTheFire(ReportTheFireParameterDTO param)
        {
            var response = new ReportTheFireResponseDTO
            {
            };
            //check secret user id
            if(checkSecretUserId(param.SecretUserId))
            {
                using(var context = new IsfContext())
                {
                    var added = context.Reports.Add( 
                        new Report
                        {
                            SecretUserId = param.SecretUserId,
                            Longitude = param.Longitude,
                            Latitude = param.Latitude,
                            TextOfComment = param.TextOfComment,
                            Timestamp = DateTime.UtcNow

                        }

                    );
                    context.SaveChanges();
                    response.ReportId = added.Entity.ReportId;
                }
                
            }
            else
            {
                response.Error = $"Unknown secret user id: \"{param.SecretUserId}\"";

            }
            
            
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
