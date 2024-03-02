using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class IncomeStatementController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public dynamic Get(DateTime start, DateTime end)
        {
            List<dynamic> Sales = new List<dynamic>();
            List<dynamic> COGS = new List<dynamic>();
            List<dynamic> Expenses = new List<dynamic>();

            
            var SalesGroupAccs = (from saleGroup in DB.Account
                                                where saleGroup.AccountGroup == null
                                                && saleGroup.AccountType != null && saleGroup.ParentCode == 4
                                                select saleGroup).ToList();
            var ExpenseGroupAccs = (from expenseGroup in DB.Account
                                  where expenseGroup.AccountGroup == null
                                  && expenseGroup.AccountType != null && expenseGroup.ParentCode == 5
                                  select expenseGroup).ToList();

            List<int> CogsAccCodes = new List<int> { 50100, 50400, 50500 };


            foreach (Account groupAcc in SalesGroupAccs)
            {
                var sales = (from record in DB.Ledgers
                             join acc in DB.Account on
                              record.AccountCode equals acc.AccountCode
                             where acc.ParentCode == groupAcc.AccountCode
                             && record.VoucherDate >= start && record.VoucherDate <= end
                             select new { record.DebitAmount, record.CreditAmount }).ToList();

                decimal? TotalAccDebit = sales.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit = sales.Sum(x => x.CreditAmount);

                var totalBalance = TotalAccDebit - TotalAccCredit;

                Sales.Add(new { groupAcc.AccountCode, groupAcc.AccountName, Balance = totalBalance });
            }


            foreach (Account groupAcc in ExpenseGroupAccs)
            {
                if (CogsAccCodes.Contains(groupAcc.AccountCode))
                {
                    var cogs = (from record in DB.Ledgers
                                join acc in DB.Account on
                                 record.AccountCode equals acc.AccountCode
                                where acc.ParentCode == groupAcc.AccountCode
                                && record.VoucherDate >= start && record.VoucherDate <= end
                                select new { record.DebitAmount, record.CreditAmount }).ToList();

                    decimal? TotalAccDebit = cogs.Sum(x => x.DebitAmount);
                    decimal? TotalAccCredit = cogs.Sum(x => x.CreditAmount);

                    var totalBalance = TotalAccDebit - TotalAccCredit;

                    COGS.Add(new { groupAcc.AccountCode, groupAcc.AccountName, Balance = totalBalance });
                }

                else
                {
                    var expense = (from record in DB.Ledgers
                                 join acc in DB.Account on
                                  record.AccountCode equals acc.AccountCode
                                 where acc.ParentCode == groupAcc.AccountCode
                                 && record.VoucherDate >= start && record.VoucherDate <= end
                                 select new { record.DebitAmount, record.CreditAmount }).ToList();

                    decimal? TotalAccDebit = expense.Sum(x => x.DebitAmount);
                    decimal? TotalAccCredit = expense.Sum(x => x.CreditAmount);

                    var totalBalance = TotalAccDebit - TotalAccCredit;

                    Expenses.Add(new { groupAcc.AccountCode, groupAcc.AccountName, Balance = totalBalance });
                }
            }

            var TotalSales = Sales.Sum(x => (decimal) x.Balance);
            var TotalCOGS = COGS.Sum(x => (decimal) x.Balance);
            var NetCostofSale = TotalSales - TotalCOGS;
            var TotalExpenses = Expenses.Sum(x => (decimal) x.Balance);
            var NetProfit = NetCostofSale - TotalExpenses;


            return new { Sales, TotalSales, COGS, TotalCOGS, NetCostofSale, Expenses, TotalExpenses, NetProfit };

        }

        // GET api/<controller>/5
        public decimal Get()
        {
            List<dynamic> Sales = new List<dynamic>();

            var SalesGroupAccs = (from saleGroup in DB.Account
                                  where saleGroup.AccountGroup == null
                                  && saleGroup.AccountType != null && saleGroup.ParentCode == 4
                                  select saleGroup).ToList();

            var startDate = DateTime.Now;
            var endDate = startDate.AddMonths(-1).AddDays(-1);

            foreach (Account groupAcc in SalesGroupAccs)
            {
                var sales = (from record in DB.Ledgers
                             join acc in DB.Account on
                              record.AccountCode equals acc.AccountCode
                             where acc.ParentCode == groupAcc.AccountCode
                             && record.VoucherDate >= endDate && record.VoucherDate <= startDate
                             select new { record.DebitAmount, record.CreditAmount }).ToList();

                decimal? TotalAccDebit = sales.Sum(x => x.DebitAmount);
                decimal? TotalAccCredit = sales.Sum(x => x.CreditAmount);

                var totalBalance = TotalAccDebit - TotalAccCredit;

                Sales.Add(new { groupAcc.AccountCode, groupAcc.AccountName, Balance = totalBalance });
            }

            return Sales.Sum(x => (decimal)x.Balance);
        }

        public dynamic Get(DateTime currMonth)
        {
            List<dynamic> salesData = new List<dynamic>();

            List<int?> SalesGroupAccs = (from saleGroup in DB.Account
                                  where saleGroup.AccountGroup == null
                                  && saleGroup.AccountType != null && saleGroup.ParentCode == 4
                                  select (int?)saleGroup.AccountCode).ToList();

            for (int i = 7; i >= 0; i--)
            {
                var month = currMonth.AddMonths(-i);
                var startDate = new DateTime(month.Year, month.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var totalSalesForMonth = (from record in DB.Ledgers
                                          join acc in DB.Account on
                                          record.AccountCode equals acc.AccountCode
                                          where SalesGroupAccs.Contains(acc.ParentCode)
                                          && record.VoucherDate >= startDate && record.VoucherDate <= endDate
                                          select record.DebitAmount - record.CreditAmount).Sum();

                salesData.Add(new { Name = month.ToString("MMM"), TotalSales = totalSalesForMonth == null? 0: totalSalesForMonth });
            }

            return salesData;
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