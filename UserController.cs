using AqsaAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();

        // GET api/<controller>
        public IEnumerable<dynamic> Get()
        {
            return (from user in DB.UserData select new 
            {  user.UserName,
               user.Password,
               user.Name,
               RoleName = user.Role != null? user.Role.RoleName : null,
               RoleCode = user.Role != null? (int?)user.Role.RoleCode : null,
               user.Status 
            }).ToList();
        }

        // GET api/<controller>/5
        public HttpResponseMessage Get(string username)
        {
            var user = ( from obj in DB.UserData
                    where obj.UserName == username
                    select new
                    {
                        obj.Name,
                        obj.Password,
                        obj.Status,
                        obj.UserName,
                        obj.Role.RoleName,
                        obj.Role.RoleCode,
                    }).FirstOrDefault();
            
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");
            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] UserCreationModel NewUser)
        {
            try
            {
                if (NewUser is null || NewUser.Username is null || NewUser.Username == "") throw new Exception("Null Values");
                DB.InsertUserData(NewUser.Username, NewUser.Password, NewUser.Name, NewUser.RoleCode);
                return Request.CreateResponse(HttpStatusCode.OK, "User Created Successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Null Values")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
                }
                else if (ex.InnerException is SqlException sqlEX && sqlEX.Number == 2627) {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "User already exists with that username");
                }
                else if (ex.InnerException is SqlException sqlEXX && sqlEXX.Number == 547)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "The role specified is not valid");
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // PUT api/<controller>/5
        public HttpResponseMessage Put([FromBody] UserUpdateModel u)
        {
            UserData user = DB.UserData.Where(usr => usr.UserName == u.Username).FirstOrDefault();
            if (user == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Username");
            
            string sql = "UPDATE UserData SET RoleCode = @newCode WHERE UserName = @username";
            object newCodeValue = u.RoleCode != null ? (object)u.RoleCode : DBNull.Value;

            DB.Database.ExecuteSqlCommand(sql,
                new SqlParameter("@newCode", newCodeValue),
                new SqlParameter("@username", u.Username));

            return Request.CreateResponse(HttpStatusCode.OK, "Updated Successfully");
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(string username)
        {
            try
            {
                DB.DeleteUserData(username);
                return Request.CreateResponse(HttpStatusCode.OK, "User Deleted Successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,"Error deleting user: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An unknown error occured: " + ex.Message);
            }
        }
    }
}