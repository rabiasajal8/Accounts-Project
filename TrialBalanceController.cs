using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TrialBalanceController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public dynamic Get(DateTime start, DateTime end)
        {
            List<dynamic> Openning = new List<dynamic>();
            List<dynamic> Current = new List<dynamic>();
            List<dynamic> Closing = new List<dynamic>();

            // Second & Third level Accounts
            List<Account> secondLevel = DB.Account.Where(acc=>acc.AccountGroup == null && acc.AccountType != null).ToList();
            List<Account> thirdLevel = DB.Account.Where(acc => acc.AccountGroup != null && acc.AccountType != null).ToList();

            // Corresponding Sum of Level Child Accounts
            foreach (Account acc in secondLevel)
            {
                var balance1 = (from voucher in DB.Ledgers
                                 where voucher.AccountCode > acc.AccountCode
                                 && voucher.AccountCode < (acc.AccountCode + 100) && voucher.VoucherDate < start
                                 select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                var balance2 = (from voucher in DB.Ledgers
                                where voucher.AccountCode > acc.AccountCode
                                && voucher.AccountCode < (acc.AccountCode + 100) && voucher.VoucherDate >= start
                                && voucher.VoucherDate <= end
                                select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                var balance3 = (from voucher in DB.Ledgers
                                where voucher.AccountCode > acc.AccountCode
                                && voucher.AccountCode < (acc.AccountCode + 100) && voucher.VoucherDate < end
                                select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                decimal? TotalAccDebit1 = balance1.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit1 = balance1.Sum(x => x.CreditAmount);

                decimal? TotalAccDebit2 = balance2.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit2 = balance2.Sum(x => x.CreditAmount);

                decimal? TotalAccDebit3 = balance3.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit3 = balance3.Sum(x => x.CreditAmount);

                Openning.Add(new { acc.AccountCode, acc.AccountName, TotalAccCredit = TotalAccCredit1, TotalAccDebit = TotalAccDebit1 });
                Current.Add(new { acc.AccountCode, acc.AccountName, TotalAccCredit = TotalAccCredit2, TotalAccDebit = TotalAccDebit2 });
                Closing.Add(new { acc.AccountCode, acc.AccountName, TotalAccCredit = TotalAccCredit3, TotalAccDebit = TotalAccDebit3 });
            }

            foreach (Account acc in thirdLevel)
            {
                var balance1 = (from voucher in DB.Ledgers
                                where voucher.AccountCode == acc.AccountCode && voucher.VoucherDate < start
                                select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                var balance2 = (from voucher in DB.Ledgers
                                where voucher.AccountCode == acc.AccountCode && voucher.VoucherDate >= start
                                && voucher.VoucherDate <= end
                                select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                var balance3 = (from voucher in DB.Ledgers
                                where voucher.AccountCode == acc.AccountCode && voucher.VoucherDate < end
                                select new { voucher.DebitAmount, voucher.CreditAmount }).ToList();

                decimal? TotalAccDebit1 = balance1.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit1 = balance1.Sum(x => x.CreditAmount);

                decimal? TotalAccDebit2 = balance2.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit2 = balance2.Sum(x => x.CreditAmount);

                decimal? TotalAccDebit3 = balance3.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit3 = balance3.Sum(x => x.CreditAmount);

                Openning.Add(new { acc.AccountCode, acc.AccountName, TotalAccCredit = TotalAccCredit1, TotalAccDebit = TotalAccDebit1 });
                Current.Add(new { acc.AccountCode, acc.AccountName, TotalAccCredit = TotalAccCredit2, TotalAccDebit = TotalAccDebit2 });
                Closing.Add(new { acc.AccountCode, acc.AccountName, TotalAccCredit = TotalAccCredit3, TotalAccDebit = TotalAccDebit3 });
            }

            // Checking for idle accounts
            Openning = Openning.Where(x => x.TotalAccCredit != 0 || x.TotalAccDebit != 0).ToList();
            Current = Current.Where(x => x.TotalAccCredit != 0 || x.TotalAccDebit != 0).ToList();
            Closing = Closing.Where(x => x.TotalAccCredit != 0 || x.TotalAccDebit != 0).ToList();

            // Final Sum
            decimal TotalDebit1 = Openning.Sum(x => (decimal)x.TotalAccDebit);
            decimal TotalCredit1 = Openning.Sum(x => (decimal)x.TotalAccCredit);

            decimal TotalDebit2 = Current.Sum(x => (decimal)x.TotalAccDebit);
            decimal TotalCredit2 = Current.Sum(x => (decimal)x.TotalAccCredit);

            decimal TotalDebit3 = Closing.Sum(x => (decimal)x.TotalAccDebit);
            decimal TotalCredit3 = Closing.Sum(x => (decimal)x.TotalAccCredit);

            return new { Openning, Current, Closing, 
                Balances = new { Openning = new { TotalDebit =  TotalDebit1, TotalCredit = TotalCredit1 },
                    Current = new { TotalDebit = TotalDebit2, TotalCredit = TotalCredit2 },
                    Closing = new { TotalDebit = TotalDebit3, TotalCredit = TotalCredit3 }
                } };
        }

        // GET api/<controller>/5
        public dynamic Get()
        {
            var now = DateTime.Now;
            var data = new List<dynamic>();
            for (int i = 7; i >= 0; i--)
            {
                var month = now.AddMonths(-i).ToString("MMM");
                var startDate = new DateTime(now.Year, now.AddMonths(-i).Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var recs = (from rec in DB.Ledgers
                            where rec.VoucherDate >= startDate && rec.VoucherDate <= endDate
                            select new { rec.DebitAmount, rec.CreditAmount }).ToList();

                var totalDebit = recs.Sum(x => x.DebitAmount);
                var totalCredit = recs.Sum(x => x.CreditAmount);

                data.Add(new { name = month, Debit = totalDebit, Credit = totalCredit });
            }

            return data;
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