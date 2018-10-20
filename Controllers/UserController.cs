using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NASATest2018.Models;

namespace NASATest2018.Controllers
{
    public class UserController : Controller
    {
        [HttpPost]
        public JsonResult GetSecretUserId()
        {
            var userId = Guid.NewGuid().ToString();
            using(var context = new IsfContext())
            {
                context.Users.Add(new User(){
                    UserId = userId
                });
                context.SaveChanges();
            }
            
            var response = new GetSecretUserIdResponseDTO
            {
               SecretUserId = userId
            };

            return new JsonResult(response);
        }

        [HttpPost]       
        public JsonResult UpdateProfile([FromBody] UpdateProfileParameterDTO updateData)
        {
            using(var context = new IsfContext())
            {
                User user = context
                    .Users
                    .Where(q => q.UserId == updateData.SecretUserId)
                    .First();

                user.Phone = updateData.Phone;
                user.Name = updateData.Name;

                context.SaveChanges();
            }

            var response = new ResponseDTO()
            {
                Error = null
            };

            return new JsonResult(response);
        }
    }
}
