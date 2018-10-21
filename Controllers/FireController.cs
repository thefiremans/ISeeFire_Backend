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

        public IActionResult Reports()
        {
            
            
            return View();
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
                            Timestamp = DateTime.UtcNow,
                            Distance = param.Distance
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
                            response.PathProcessed = path;
                            response.RowsImported = result.Count;
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

        [HttpGet]
        public JsonResult Check()
        {
            return new JsonResult(getImageUrl("NASA.png"));
        }

        private  string getImageUrl(string imagePath)
        {
        string host = Request.Host.Value;
        if(!host.StartsWith("http"))
        {
            host = "http://" + host;
        }
        string result = $"{host}/ISeeFireImages/{imagePath}";
        return result;
        }

        [HttpGet]
        public JsonResult DownloadNasaFiles()
        {
            
            string[] filesFromNASA = new [] 
            {
                "https://firms.modaps.eosdis.nasa.gov/data/active_fire/c6/csv/MODIS_C6_Global_24h.csv"
                , "https://firms.modaps.eosdis.nasa.gov/data/active_fire/viirs/csv/VNP14IMGTDL_NRT_Global_24h.csv"
            };  

            List<JsonResult> result = new List<JsonResult>();           

            foreach(var path in filesFromNASA )
            {

            result.Add(downloadNasaFileAndImportInDb(path));

            }

            return new JsonResult(result);

            
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
        public JsonResult GetNearbyFires([FromBody] GetNearbyFiresParametersDTO parameters)
        {
            var response = new List<GetNearbyFiresResponseDTO>();

            // owner
            response.Add(new GetNearbyFiresResponseDTO() {
                Latitude = 46.484566m, 
                Longitude = 30.737960m,
                PhotoUrl = "asdf",
                IsOwner = true,
                IsNasa = false,
                Confidence = 0.2m,   // from 0 to 1
                Distance = LatLongDistance(46.484566m, 30.737960m, parameters.Latitude, parameters.Longitude)
            });

            // not owner
            response.Add(new GetNearbyFiresResponseDTO() {
                Latitude = 46.500144m,
                Longitude = 30.663893m,
                PhotoUrl = "sdfg",
                IsOwner = false,
                IsNasa = false,
                Confidence = 0.2m,   // from 0 to 1
                Distance = LatLongDistance(46.500144m, 30.663893m, parameters.Latitude, parameters.Longitude)
            });

            // nasa
            response.Add(new GetNearbyFiresResponseDTO() {
                Latitude = 46.412424m,
                Longitude = 30.670665m,
                PhotoUrl = getImageUrl("NASA.png"),
                IsOwner = false,
                IsNasa = true,
                Confidence = 0.65m,   // from 0 to 1
                Distance = LatLongDistance(46.412424m, 30.670665m, parameters.Latitude, parameters.Longitude)
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
                        PhotoUrl = getImageUrl("NASA.png"),
                        IsOwner = false,
                        IsNasa = true,
                        Confidence = report.Confidence, //confidence is already normalized
                        Distance = LatLongDistance(report.Latitude, report.Longitude, parameters.Latitude, parameters.Longitude)
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
                        PhotoUrl = report.ImagePath ?? getImageUrl(report.ImagePath),
                        IsOwner = false,
                        IsNasa = false,
                        Confidence = 0.2m,
                        Distance = LatLongDistance(report.Latitude, report.Longitude, parameters.Latitude, parameters.Longitude)
                    });
                }
            }

            response = response.OrderBy(q => q.Distance).ToList();

            return new JsonResult(response);
        }

        private decimal LatLongDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var R = 6371000; // metres
            var phi1 = toRadians(lat1);
            var phi2 = toRadians(lat2);
            var dPhi = toRadians(lat2-lat1);
            var dLambda = toRadians(lon2-lon1);

            var a = Math.Sin((double)dPhi/2) * Math.Sin((double)dPhi/2) +
                    Math.Cos((double)phi1) * Math.Cos((double)phi2) *
                    Math.Sin((double)dLambda/2) * Math.Sin((double)dLambda/2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
            var d = R * c;

            return (decimal)d;
        }

        private decimal toRadians(decimal a)
        {
            return (a*2.0m*(decimal)Math.PI)/360.0m;
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
            double latitudeInRadians = ( ((double) latitude) * Math.PI ) / 180.0;
            double localRadius = radius*Math.Cos(latitudeInRadians );
            double localLength = localRadius*2.0*Math.PI;
            return (decimal)Math.Abs(((double)distance*360.0)/localLength);
        }
        private decimal DistanceToLatitude(decimal distance)
        {
            return (decimal)Math.Abs((double) ((180.0m*distance)/20003930.0m));   // in meters
        }
    }
}
