using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RoleController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public IEnumerable<dynamic> Get()
        {
            return (from role in DB.Role select new { role.RoleName }).ToList();
        }

        // GET api/<controller>/5
        public HttpResponseMessage Get(string RoleName)
        {
            if (RoleName == null) { return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RoleName was invalid"); }
            var rol =  (from role in DB.Role where role.RoleName == RoleName select new { role.RoleCode }).FirstOrDefault();
            return Request.CreateResponse(HttpStatusCode.OK, rol);
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] RoleCreationModel Role)
        {
            try
            {
                if (Role is null | Role.RoleName == "" | Role.RoleName == null) throw new Exception("Null Values");
                if (DB.Role.Where(rol => rol.RoleName == Role.RoleName).FirstOrDefault() != null) throw new Exception("Exists");
                var role = new Role { RoleName = Role.RoleName };
                DB.Role.Add(role);
                DB.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Role Created Successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Null Values")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
                }
                else if (ex.Message == "Exists")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "The role name already exists");
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(int RoleCode)
        {
            try
            {
                DB.DeleteRole(RoleCode);
                return Request.CreateResponse(HttpStatusCode.OK, "Role Deleted Successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error deleting user: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An unknown error occured: " + ex.Message);
            }
        }
    }
}