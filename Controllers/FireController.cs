using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        
        private List<NasaFireReport> parseNasaDataCSV(MemoryStream stream)
        {
            var result = new List<NasaFireReport>();
            string[] headers = null;
            int latitude = 0
                ,longitude = 0
                ,acq_date = 0
                ,acq_time = 0
                ,confidence = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();
                headers = line.Split(',', options: StringSplitOptions.RemoveEmptyEntries);

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

                string[] confidenceLevels = new [] {"nominal", "low", "high"};

                bool isViirsData = false;
                bool isSecondRow = true;

                while( !reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if(string.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    var content = line.Split(',', options: StringSplitOptions.RemoveEmptyEntries);

                    CultureInfo provider = CultureInfo.InvariantCulture;

                    string date =$"{content[acq_date]}-{content[acq_time].Substring(0,2)}:{content[acq_time].Substring(2)}";

                    string localConfidence = content[confidence];

                    decimal parsedConfidence = 0;

                    if(isSecondRow)
                    {
                        isSecondRow = false;
                        isViirsData = confidenceLevels.Contains(localConfidence);
                    }

                    if(isViirsData)
                    {
                        if(localConfidence.StartsWith("l"))
                        {
                            parsedConfidence = 0.10m;
                        }
                        if(localConfidence.StartsWith("n"))
                        {
                            parsedConfidence = 0.40m;
                        }
                        if(localConfidence.StartsWith("h"))
                        {
                            parsedConfidence = 0.80m;
                        }
                    }
                    else
                    {
                        parsedConfidence = Decimal.Parse(localConfidence);
                        parsedConfidence = parsedConfidence / 100.0m;
                    }

                    var x = new NasaFireReport
                    {
                        Latitude = Decimal.Parse( content[latitude]),
                        Longitude = Decimal.Parse(content[longitude]),
                        Timestamp = DateTime.ParseExact(date, "yyyy-MM-dd-HH:mm", provider),
                        Confidence = parsedConfidence
                    };
                    result.Add(x);
                 } 
             }
             return result;
            
        }

        private bool isLastImportNASAFilesDateIsActual()
        {
            return false;
        }

        private JsonResult downloadNasaFileAndImportInDb(string path)
        {
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
                 
                 {
                     
                     try
            {
            MemoryStream stream = new MemoryStream(client.DownloadData(path));
            var result = parseNasaDataCSV(stream);
            using(var context = new IsfContext())
            {
                try
                {
                context.NasaFireReports.AddRange(result);
                context.SaveChanges(); 
                }
                catch(Exception ex)
                {
                Console.WriteLine(ex.Message);
                }

                
            }
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
        public JsonResult DownloadModisFileFromNASA()
         {
             string[] filesFromNASA = new [] 
             {
                 "https://firms.modaps.eosdis.nasa.gov/data/active_fire/c6/csv/MODIS_C6_Global_24h.csv"
             };

             return downloadNasaFileAndImportInDb(filesFromNASA[0]);
             
         }

         [HttpPost]
        public JsonResult DownloadViirsFileFromNASA()
         {
             string[] filesFromNASA = new [] 
             {
                 "https://firms.modaps.eosdis.nasa.gov/data/active_fire/viirs/csv/VNP14IMGTDL_NRT_Global_24h.csv"
             };

             return downloadNasaFileAndImportInDb(filesFromNASA[0]);
             
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
                bool longNormal = true;

                if(longitudeDelta + parameters.Longitude > 180 || 
                    parameters.Longitude - longitudeDelta < -180)
                    longNormal = false;

                decimal minLongitude = NormalizeLongitude(parameters.Longitude - longitudeDelta);
                decimal maxLongitude = NormalizeLongitude(parameters.Longitude + longitudeDelta);

                var reports = context
                    .NasaFireReports
                    .Where(q => q.Latitude < Math.Min(parameters.Latitude + lattitudeDelta, 90) 
                            && q.Latitude > Math.Max(parameters.Latitude - lattitudeDelta, -90));

                if(longNormal)
                    reports = reports
                        .Where(q => q.Longitude < parameters.Longitude + longitudeDelta &&
                                    q.Longitude > parameters.Longitude - longitudeDelta);
                else
                {
                    reports = reports
                        .Where(q => 
                            (q.Longitude < maxLongitude && q.Longitude > -180) ||
                            (q.Longitude > minLongitude && q.Longitude < 180) 
                        );
                }

                // search nasa db
                foreach(var report in reports)
                {
                    response.Add(new GetNearbyFiresResponseDTO()
                    {
                        Latitude = report.Latitude,
                        Longitude = report.Longitude,
                        PhotoUrl = "stillFake",
                        IsOwner = false,
                        IsNasa = true,
                        Confidence = report.Confidence //confidence is already normalized
                    });
                }

                // search reports
                // filter by distance
                var userRerorts = context.Reports
                    .Where(q => q.Latitude < Math.Min(parameters.Latitude + lattitudeDelta, 90) 
                            && q.Latitude > Math.Max(parameters.Latitude - lattitudeDelta, -90));

                if(longNormal)
                    userRerorts = userRerorts 
                        .Where(q => q.Longitude < parameters.Longitude + longitudeDelta &&
                                    q.Longitude > parameters.Longitude - longitudeDelta);
                else
                {
                    userRerorts = userRerorts 
                        .Where(q => 
                            (q.Longitude < maxLongitude && q.Longitude > -180) ||
                            (q.Longitude > minLongitude && q.Longitude < 180) 
                        );
                }
                
                foreach(var report in userRerorts)
                {
                    response.Add(new GetNearbyFiresResponseDTO()
                    {
                        Latitude = report.Latitude,
                        Longitude = report.Longitude,
                        PhotoUrl = "stillFake",
                        IsOwner = false,
                        IsNasa = false,
                        Confidence = 0.2m
                    });
                }
            }

            response = response.OrderBy(q => q.Distance).ToList();

            return new JsonResult(response);
        }

        private decimal NormalizeLongitude(decimal longitude)
        {
            while(longitude > 180)
            {
                longitude -= 360;
            }

            while(longitude < -180)
            {
                longitude += 360;
            }
            return longitude;
        }

        private decimal DistanceToLongitude(decimal distance, decimal latitude)
        {
            int radius = 6371000;   // in meters
            double localRadius = radius*Math.Cos((double)latitude);
            double localLength = localRadius*2.0*Math.PI;
            return (decimal)Math.Abs(((double)distance*360.0)/localLength);
        }

        private decimal DistanceToLatitude(decimal distance)
        {
            return (decimal)Math.Abs((double) ((180.0m*distance)/20003930.0m));   // in meters
        }
    }
}
