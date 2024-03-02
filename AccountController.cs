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
    public class AccountController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public IEnumerable<dynamic> Get()
        {
            return (from account in DB.Account
                    orderby account.AccountCode
                    select new
                    {
                        account.AccountCode,
                        account.AccountName,
                        account.AccountType,
                        account.AccountGroup,
                        account.AccountStatus,
                        tcode = account.AccountCode.ToString().Length == 1 ? account.AccountCode.ToString() + "0000" : account.AccountCode.ToString() 
                    }).ToList().OrderBy(a => a.tcode);
        }

        // GET api/<controller>/5
        public dynamic Get(int id)
        {
            return (from acc in DB.Account where acc.AccountCode == id select new { acc.AccountCode, acc.AccountName, acc.AccountStatus, acc.AccountType, acc.AccountGroup }).FirstOrDefault();
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] AccountCreationModel acc)
        {
            try
            {
                if (acc == null || acc.AccountName == "" || acc.AccountName == null || acc.ParentCode <= 0 || acc.ParentLevel <= 0) throw new Exception("Null Values");
                if (DB.Account.Where(pr => acc.AccountName == pr.AccountName).FirstOrDefault() != null) throw new Exception("Exists");

                DB.InsertNewAccount(acc.AccountName, acc.ParentCode, acc.ParentLevel);
                
                return Request.CreateResponse(HttpStatusCode.OK, "Account Added Successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Null Values")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
                }
                else if (ex.InnerException is SqlException sqlEX && sqlEX.Number == 2627)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "Account already exists with that name");
                }
                else if (ex.Message == "Exists")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "Account already exists with that name");
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                Account acc = DB.Account.Find(id);
                var childs =  (from a in DB.Account where a.ParentCode == acc.AccountCode select a).ToList();
                if (childs.Count <= 0)
                {
                    DB.Account.Remove(acc);
                    DB.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Account Deleted Successfully");
                }
                return Request.CreateResponse(HttpStatusCode.Ambiguous, "Account consist of sub-accounts");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error deleting account: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An unknown error occured: " + ex.Message);
            }
        }
    }
}