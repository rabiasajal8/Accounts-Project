using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class YearController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public IEnumerable<dynamic> Get(bool activeOnly)
        {
            if (activeOnly) { return (from yr in DB.Year where yr.YearStatus == true select new { yr.YearCode, yr.YearName, yr.YearStatus }).ToList(); }
            return DB.SelectYears();
        }

        // GET api/<controller>/5
        public HttpResponseMessage Get(string YearName)
        {
            if (YearName == null) { return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "YearName was invalid"); }
            var year = DB.Year.Where(yr => yr.YearName == YearName).FirstOrDefault();

            if (year != null)
            {                
                return Request.CreateResponse(HttpStatusCode.OK, year.YearCode);
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Year not found");
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] YearCreationModel Year)
        {
            try
            {
                if (Year == null || Year.YearName == "" || Year.YearName == null) throw new Exception("Null Values");
                if (DB.Year.Where(yr => Year.YearName == yr.YearName).FirstOrDefault() != null) throw new Exception("Exists");

                var year = new Year { YearName = Year.YearName };
                DB.Year.Add(year);
                DB.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Year Added Successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Null Values")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
                }
                else if (ex.Message == "Exists")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "The year name already exists");
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // PUT api/<controller>/5
        public HttpResponseMessage Put(int id)
        {
            Year year = DB.Year.Find(id);
            if (year == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Year Code");

            // Toggling Status
            // Execute a SQL query to update the YearStatus column
            string sql = "UPDATE Year SET YearStatus = @newStatus WHERE YearCode = @yearId";
            DB.Database.ExecuteSqlCommand(sql,
                new SqlParameter("@newStatus", !year.YearStatus),
                new SqlParameter("@yearId", year.YearCode));

            return Request.CreateResponse(HttpStatusCode.OK, "Updated Successfully");
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(string yearName)
        {
            try
            {
                var year = DB.Year.FirstOrDefault(yr => yr.YearName == yearName);
                DB.Year.Remove(year);
                DB.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Year Deleted Successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error deleting year: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An unknown error occured: " + ex.Message);
            }
        }
    }
}