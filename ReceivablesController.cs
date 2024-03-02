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
    public class ReceivablesController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();

        // Level 2 Acc
        readonly List<int?> ReceivablesAccCodes = new List<int?> { 10500 };

        // GET api/<controller>
        public IEnumerable<dynamic> Get(DateTime date)
        {
            
            var receivables = (from voucher in DB.Ledgers
                            join acc in DB.Account on voucher.AccountCode equals acc.AccountCode
                            where ReceivablesAccCodes.Contains(acc.ParentCode)
                            && voucher.VoucherDate <= date
                            orderby voucher.VoucherNumber descending
                            select new
                            {
                                voucher.id,
                                voucher.PeriodCode,
                                voucher.VoucherType,
                                voucher.VoucherNumber,
                                voucher.VoucherDate,
                                voucher.BrCode,
                                voucher.Particulars,
                                voucher.Status,
                                voucher.CreatedBy,
                                voucher.PostedBy,
                                voucher.AccountCode,
                                voucher.DebitAmount,
                                voucher.CreditAmount,
                                voucher.Narration
                            }).ToList();

            return receivables;
        }

        // GET api/<controller>/5
        public decimal Get()
        {
            List<int?> ReceivablesAccCodes = new List<int?> { 10500 };

            var receivables = (from voucher in DB.Ledgers
                               join acc in DB.Account on voucher.AccountCode equals acc.AccountCode
                               where ReceivablesAccCodes.Contains(acc.ParentCode)
                               orderby voucher.VoucherNumber descending
                               select new
                               {
                                   voucher.DebitAmount,
                                   voucher.CreditAmount,
                               }).ToList();

            return receivables.Sum(x => (decimal)x.DebitAmount) - receivables.Sum(x => (decimal)x.CreditAmount);
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