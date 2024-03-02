using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LedgerController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public IEnumerable<dynamic> Get(DateTime date)
        {
            string sql = "EXEC sp_refreshview @view";
            DB.Database.ExecuteSqlCommand(sql,
                new SqlParameter("@view", "ledger"));

            var ledger = (from record in DB.Ledgers orderby record.BrCode where record.VoucherDate >= date select new
            {
                record.id,
                record.PeriodCode,
                record.VoucherType,
                record.VoucherNumber,
                record.VoucherDate,
                record.BrCode,
                record.CreatedBy,
                record.PostedBy,
                record.AccountCode,
                record.DebitAmount,
                record.CreditAmount,
            }).ToList();
            return ledger;
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
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