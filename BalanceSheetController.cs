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
    public class BalanceSheetController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();


        // GET api/<controller>
        public IEnumerable<dynamic> Get(DateTime date)
        {

            // List of Current Assets Account Code
            List<int> CA = new List<int> { 10300, 10400, 10500, 10600 };
            List<int> NCA = new List<int> { 10100 };

            List<dynamic> Sheet = new List<dynamic>();
            List<dynamic> CurrentAssets = new List<dynamic>();
            List<dynamic> NonCurrentAssets = new List<dynamic>();
            List<dynamic> OtherAssets = new List<dynamic>();
            List<dynamic> Liabilities = new List<dynamic>();
            List<dynamic> Equity = new List<dynamic>();

            List<Account> assetAccs = DB.Account.Where(acc => acc.AccountGroup == null && acc.AccountType != null && acc.ParentCode == 1).ToList();
            List<Account> liabilityAccs = DB.Account.Where(acc => acc.AccountGroup == null && acc.AccountType != null && acc.ParentCode == 2).ToList();
            List<Account> equityAccs = DB.Account.Where(acc => acc.AccountGroup == null && acc.AccountType != null && acc.ParentCode == 3).ToList();

            // Assets
            foreach (Account acc in assetAccs)
            {
                var balance = (from voucher in DB.Ledgers
                               where voucher.AccountCode > acc.AccountCode
                               && voucher.AccountCode < (acc.AccountCode + 100)
                               && voucher.VoucherDate <= date
                               select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                decimal? TotalAccDebit = balance.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit = balance.Sum(x => x.CreditAmount);

                var totalBalance = TotalAccCredit - TotalAccDebit;

                if (CA.Contains(acc.AccountCode))
                {
                    CurrentAssets.Add(new { acc.AccountCode, acc.AccountName, Balance = totalBalance });
                }
                else if (NCA.Contains(acc.AccountCode))
                {
                    NonCurrentAssets.Add(new { acc.AccountCode, acc.AccountName, Balance = totalBalance });
                }
                else
                {
                    OtherAssets.Add(new { acc.AccountCode, acc.AccountName, Balance = totalBalance });
                }
            }

            // Liabilities
            foreach (Account acc in liabilityAccs)
            {
                var balance = (from voucher in DB.Ledgers
                               where voucher.AccountCode > acc.AccountCode
                               && voucher.AccountCode < (acc.AccountCode + 100)
                               && voucher.VoucherDate <= date
                               select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                decimal? TotalAccDebit = balance.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit = balance.Sum(x => x.CreditAmount);

                var totalBalance = TotalAccCredit - TotalAccDebit;

                Liabilities.Add(new { acc.AccountCode, acc.AccountName, Balance = totalBalance });
            }

            // Equity
            foreach (Account acc in equityAccs)
            {
                var balance = (from voucher in DB.Ledgers
                               where voucher.AccountCode > acc.AccountCode
                               && voucher.AccountCode < (acc.AccountCode + 100)
                               && voucher.VoucherDate <= date
                               select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                decimal? TotalAccDebit = balance.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit = balance.Sum(x => x.CreditAmount);

                var totalBalance = TotalAccCredit - TotalAccDebit;

                Equity.Add(new { acc.AccountCode, acc.AccountName, Balance = totalBalance });
            }

            var TotalCurrentAssets =  CurrentAssets.Sum(x => (decimal)x.Balance);
            var TotalNonCurrentAssets = NonCurrentAssets.Sum(x => (decimal)x.Balance);
            var TotalOtherAssets = OtherAssets.Sum(x => (decimal)x.Balance);
            var TotalAssets = TotalOtherAssets + TotalNonCurrentAssets + TotalCurrentAssets;

            var TotalLiabilities = Liabilities.Sum(x => (decimal)x.Balance);
            var TotalEquity = Equity.Sum(x => (decimal)x.Balance);

            var TotalEqLib = TotalEquity + TotalLiabilities;

            Sheet.Add(date);
            Sheet.Add(new { CurrentAssets, TotalCurrentAssets,  NonCurrentAssets, TotalNonCurrentAssets, OtherAssets, TotalOtherAssets, TotalAssets });
            Sheet.Add(new { Liabilities, TotalLiabilities });
            Sheet.Add(new { Equity, TotalEquity, TotalEqLib });

            return Sheet;

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