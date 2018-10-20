﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NASATest2018.Models;

namespace NASATest2018.Controllers
{
    public class FireController : Controller
    {
        private bool checkSecretUserId(string SecretUserId)
        {
            using(var context = new IsfContext())
            {
                var findResult = context.Users.FirstOrDefault(x => x.UserId == SecretUserId);
                return findResult != null;
            }
        }

        [HttpPost]
        public JsonResult ReportTheFire([FromBody] ReportTheFireParameterDTO param)
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
        
        private void parseCSV(MemoryStream stream)
        {
            bool isFirstLine = true;
            string[] headers = null;
            int latitude, longitude,acq_date,acq_time,confidence;
            using (StreamReader reader = new StreamReader(stream))
            {
                 string line = reader.ReadLine();
                if(isFirstLine)
                {
                    headers = line.Split(',', options: StringSplitOptions.RemoveEmptyEntries);
                    isFirstLine = false;
                    for(int i = 0; i < headers.Length; i++)
                    {
                        string actualHeader = headers[i];
                        if(actualHeader == nameof(latitude))
                        {
                            latitude = i;
                        }
                        if(actualHeader == nameof(longitude))
                        {
                            longitude = i;
                        }
                        if(actualHeader == nameof(acq_date))
                        {
                            acq_date = i;
                        }
                        if(actualHeader == nameof(acq_time))
                        {
                            acq_time = i;
                        }
                        if(actualHeader == nameof(confidence))
                        {
                            confidence = i;
                        }
                    }
                }
             }
            
        }

        private bool isLastImportNASAFilesDateIsActual()
        {
            return false;
        }

         [HttpPost]
        public JsonResult DownloadFilesFromNASA()
         {
             string[] filesFromNASA = new [] 
             {
                 "https://firms.modaps.eosdis.nasa.gov/data/active_fire/c6/csv/MODIS_C6_Global_24h.csv",
                 "https://firms.modaps.eosdis.nasa.gov/data/active_fire/viirs/csv/VNP14IMGTDL_NRT_Global_24h.csv"
             };
             DownloadFilesFromNASAResponseDTO response = new DownloadFilesFromNASAResponseDTO
             {
 
             };
 
             if(isLastImportNASAFilesDateIsActual())
             {
                 response.Error = "No need to import NASA files.";
                 return new JsonResult(response);
             }
 
             using (var client = new WebClient())
             {
                 foreach(var path in filesFromNASA)
                 {
                     try
                     {
                         MemoryStream stream = new MemoryStream(client.DownloadData(path));
                     }
                     catch(Exception ex)
                     {
                         Console.WriteLine(ex.Message);
                     }
                     
 
                 }
                 
             }
             
 
 
             return new JsonResult(response);
         }
        
        [HttpPost]
        public JsonResult GetNearbyFires(GetNearbyFiresParametersDTO parameters)
        {
            var response = new List<GetNearbyFiresResponseDTO>();

            // owner
            response.Add(new GetNearbyFiresResponseDTO() {
                Latitude = 46.484566m, 
                Longitude = 30.737960m,
                PhotoUrl = "asdf",
                IsOwner = true,
                IsNasa = false,
                Confidence = 0.2m   // from 0 to 1
            });

            // not owner
            response.Add(new GetNearbyFiresResponseDTO() {
                Latitude = 46.500144m,
                Longitude = 30.663893m,
                PhotoUrl = "sdfg",
                IsOwner = false,
                IsNasa = false,
                Confidence = 0.2m   // from 0 to 1
            });

            // nasa
            response.Add(new GetNearbyFiresResponseDTO() {
                Latitude = 46.412424m,
                Longitude = 30.670665m,
                PhotoUrl = "dfgh",
                IsOwner = false,
                IsNasa = true,
                Confidence = 0.65m   // from 0 to 1
            });

            decimal longitudeDelta = DistanceToLongitude(parameters.Distance, parameters.Latitude);
            decimal lattitudeDelta = DistanceToLatitude(parameters.Distance);
            using(var context = new IsfContext())
            {
                // search nasa db
                // search reports
            }

            return new JsonResult(response);
        }

        private decimal DistanceToLongitude(decimal distance, decimal latitude)
        {
            int radius = 6371000;   // in meters
            decimal localRadius = (decimal)(radius*Math.Cos((double)latitude));
            decimal localLength = localRadius*2.0m*(decimal)Math.PI;
            return (distance*360.0m)/localLength;
        }

        private decimal DistanceToLatitude(decimal distance)
        {
            return (180.0m*distance)/20003930.0m;   // in meters
        }
    }
}
