using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RoleObjectController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public HttpResponseMessage Get(string RoleName)
        {
            if (RoleName == null) { return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RoleName was invalid"); }

            // Getting the role code
            Role role = DB.Role.Where(rol => rol.RoleName == RoleName).FirstOrDefault();
            if (role == null) { return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RoleName was invalid"); }

            // Getting the list of role elements
            var elems = (from roleObj in DB.RoleObject where roleObj.Role.RoleName == role.RoleName select new { roleObj.AppObject.ObjName }).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, elems);
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] RoleObjectCreationModel RoleObject)
        {
            try
            {
                if (RoleObject is null) throw new Exception("Null Values");
                var RObject = new RoleObject { RoleCode = RoleObject.RoleCode, ObjCode = RoleObject.ObjCode };
                DB.RoleObject.Add(RObject);
                DB.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Role Object Created Successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Null Values")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
                }
                else if (ex.InnerException is SqlException sqlEXX && sqlEXX.Number == 547)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "The role code or obj code specified is not valid");
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
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