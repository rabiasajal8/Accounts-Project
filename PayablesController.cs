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
    public class PayablesController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();

        // Level 2 Acc
        readonly List<int?> PayableAccCodes = new List<int?> { 20300 };
        // GET api/<controller>
        public IEnumerable<dynamic> Get(DateTime date)
        {

            var payables = (from voucher in DB.Ledgers
                            join acc in DB.Account on voucher.AccountCode equals acc.AccountCode
                            where PayableAccCodes.Contains(acc.ParentCode)
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

            return payables;
        }

        // GET api/<controller>/5
        public decimal Get()
        {
            var payables = (from voucher in DB.Ledgers
                            join acc in DB.Account on voucher.AccountCode equals acc.AccountCode
                            where PayableAccCodes.Contains(acc.ParentCode)
                            orderby voucher.VoucherNumber descending
                            select new
                            {
                                voucher.DebitAmount,
                                voucher.CreditAmount,
                            }).ToList();

            return payables.Sum(x => (decimal)x.DebitAmount) - payables.Sum(x => (decimal)x.CreditAmount);
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