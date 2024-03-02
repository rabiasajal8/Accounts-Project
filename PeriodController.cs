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
    public class PeriodController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public IEnumerable<dynamic> Get(string YearName, bool getAllActive)
        {
            if (getAllActive)
            { 
                return (from pr in DB.Period where pr.PeriodStatus == true && pr.Year.YearStatus == true
                        select new { pr.PeriodCode, pr.PeriodName, pr.PeriodStatus }).ToList(); 
            }

            if (YearName == null)
            {
                return (from pr in DB.Period select new { pr.PeriodCode, pr.PeriodName, pr.PeriodStatus }).ToList();
            }

            var year = DB.Year.Where(yr => yr.YearName == YearName).FirstOrDefault();

            if (year != null)
            {
                // Getting Periods
                var YearCode = year.YearCode;
                var Periods = (from pr in DB.Period where pr.YearCode == YearCode select new { pr.PeriodCode, pr.PeriodName, pr.PeriodStatus }).ToList();
                return Periods;
            }
            return null;
        }

        // GET api/<controller>/5
        public dynamic Get(int id)
        {
            var period = DB.Period.Where(pr => pr.PeriodCode == id).FirstOrDefault();
            if (period == null) { return null; }
            return new { period.PeriodCode, period.PeriodName, period.PeriodStatus };
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] PeriodCreationModel Period)
        {
            try
            {
                if (Period == null || Period.PeriodName == "" || Period.PeriodName == null) throw new Exception("Null Values");
                if (DB.Period.Where(pr => Period.PeriodName == pr.PeriodName && pr.YearCode == Period.YearCode).FirstOrDefault() != null) throw new Exception("Exists");

                var period = new Period { YearCode = Period.YearCode, PeriodName = Period.PeriodName };
                DB.Period.Add(period);
                DB.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Period Added Successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Null Values")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
                }
                else if (ex.InnerException is SqlException sqlEXX && sqlEXX.Number == 547)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "The year specified is not valid");
                }
                else if (ex.Message == "Exists")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "The period name already exists");
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // PUT api/<controller>/5
        public HttpResponseMessage Put(int id)
        {
            Period period = DB.Period.Find(id);
            if (period == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Period Code");

            // Toggling Status
            // Execute a SQL query to update the PeriodStatus column
            string sql = "UPDATE Period SET PeriodStatus = @newStatus WHERE PeriodCode = @periodId";
            DB.Database.ExecuteSqlCommand(sql,
                new SqlParameter("@newStatus", !period.PeriodStatus),
                new SqlParameter("@periodId", period.PeriodCode));

            return Request.CreateResponse(HttpStatusCode.OK, "Updated Successfully");
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                Period period = DB.Period.Find(id);
                DB.Period.Remove(period);
                DB.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Period Deleted Successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error deleting period: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An unknown error occured: " + ex.Message);
            }
        }
    }
}