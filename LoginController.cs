using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        //POST api/<controller>
        public HttpResponseMessage Post([FromBody] LoginModel UserData)
        {
            AqsaTradersEntities DB = new AqsaTradersEntities();
            if (UserData == null || UserData.Username == "" || UserData.Password == "")
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
            }

            var user = (from obj in DB.UserData
                        where obj.UserName == UserData.Username && obj.Password == UserData.Password
                        select new
                        {
                            obj.Name,
                            obj.Role.RoleName,
                        }).FirstOrDefault();

            if (user == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Username or password was incorrect");
            }

            var elems = (from roleObj in DB.RoleObject where roleObj.Role.RoleName == user.RoleName select new { roleObj.AppObject.ObjName }).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, new { user.Name, user.RoleName, Rights = elems});
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}