using AqsaAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class VoucherController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();
        // GET api/<controller>
        public IEnumerable<dynamic> Get(int branchCode, int periodCode, string voucherType)
        {
            var vouchers = (from v in DB.Voucher
                            orderby v.VoucherNumber descending
                            where v.Branch.BrCode == branchCode && v.Period.PeriodCode == periodCode && v.VoucherType == voucherType
                            select new
                            {
                                v.VoucherNumber,
                                v.VoucherDate,
                                v.Particulars,
                                v.CreatedBy,
                                v.PostedBy,
                                Details = ( from vd in DB.VoucherDetail where vd.Voucher.VoucherNumber == v.VoucherNumber
                                            select new { vd.AccountCode, vd.DebitAmount, vd.CreditAmount, vd.Narration } )
                            }
                             ).ToList();
            return vouchers;
        }

        // GET api/<controller>/5
        public IEnumerable Get(DateTime date)
        {
            var vouchers = (from v in DB.Voucher
                            orderby v.BrCode
                            where v.VoucherDate == date && v.Status == true
                            select new
                            {
                                v.Branch.BrName,
                                v.VoucherNumber,
                                v.Period.PeriodName,
                                v.VoucherDate,
                                v.Particulars,
                                Amount = (from vd in DB.VoucherDetail
                                          where vd.Voucher.VoucherNumber == v.VoucherNumber
                                          select vd.DebitAmount).Sum(),
                                v.CreatedBy,
                                v.PostedBy,
                                Details = (from vd in DB.VoucherDetail
                                           where vd.Voucher.VoucherNumber == v.VoucherNumber
                                           select new { vd.AccountCode, vd.DebitAmount, vd.CreditAmount, vd.Narration })
                            }
                             ).ToList();
            return vouchers;
        }

        public IEnumerable Get()
        {
            var vouchers = (from v in DB.Voucher
                            orderby v.BrCode
                            where (v.Period.PeriodStatus == false || v.Period.Year.YearStatus == false) && v.Status == false
                            select new
                            {
                                v.Branch.BrName,
                                v.VoucherNumber,
                                v.Period.PeriodName,
                                v.VoucherDate,
                                v.Particulars,
                                Amount = (from vd in DB.VoucherDetail
                                          where vd.Voucher.VoucherNumber == v.VoucherNumber
                                          select vd.DebitAmount).Sum(),
                                v.CreatedBy,
                                v.PostedBy,
                                Details = (from vd in DB.VoucherDetail
                                           where vd.Voucher.VoucherNumber == v.VoucherNumber
                                           select new { vd.AccountCode, vd.DebitAmount, vd.CreditAmount, vd.Narration })
                            }
                             ).ToList();
            return vouchers;
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] VoucherInsertModel voucher)
        {
            try
            {
                if (voucher == null || voucher.Details == null || voucher.Details.Count == 0 ||
                    voucher.VoucherDate == null || voucher.VoucherType == null ||
                    voucher.BranchCode < 0 || voucher.PeriodCode < 0) throw new Exception("Null Values");

                var period = DB.Period.Find(voucher.PeriodCode );
                if (period.PeriodStatus == false || period.Year.YearStatus == false) throw new Exception("Period Inactive");

                decimal totalCredit = 0, totalDebit = 0;

                foreach (var vd in voucher.Details)
                {
                    if (vd.AccountCode <= 0) { throw new Exception("Null Values"); }
                    totalDebit += vd.DebitAmount;
                    totalCredit += vd.CreditAmount;   
                }

                if (totalCredit == 0 || totalDebit == 0) { throw new Exception("Invalid Debit/Credit");  }
                if (totalCredit - totalDebit != 0) { throw new Exception("Imbalanced Voucher"); }

                var v = new Voucher
                {
                    BrCode = voucher.BranchCode,
                    CreatedBy = voucher.CreatedBy,
                    PeriodCode = voucher.PeriodCode,
                    Particulars = voucher.Particulars,
                    VoucherType = voucher.VoucherType,
                    VoucherDate = voucher.VoucherDate,
                };

                DB.Voucher.Add(v);
                DB.SaveChanges();

                foreach (var vd in voucher.Details)
                {
                    VoucherDetail vdetail = new VoucherDetail
                    {
                        VoucherNumber = v.VoucherNumber,
                        AccountCode = vd.AccountCode,
                        DebitAmount = vd.DebitAmount,
                        CreditAmount = vd.CreditAmount,
                        Narration = vd.Narration,
                    };
                    DB.VoucherDetail.Add(vdetail);
                }

                DB.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Voucher Added Successfully");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Null Values")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "One or more values were empty");
                }
                else if (ex.Message == "Period Inactive")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Specified period is inactive");
                }
                else if (ex.Message == "Invalid Debit/Credit")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "Please provide valid Debit / Credit values");
                }
                else if (ex.Message == "Imbalanced Voucher")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The voucher does not appear to be balanced");
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // PUT api/<controller>/5
        public HttpResponseMessage Put(int id, string user)
        {
            Voucher voucher = DB.Voucher.Find(id);
            if (voucher == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Voucher");

            // Toggling Status
            // Execute a SQL query to update the Voucher Status Column
            string sql = "UPDATE Voucher SET Status = @newStatus WHERE VoucherNumber = @voucherId";
            DB.Database.ExecuteSqlCommand(sql,
                new SqlParameter("@newStatus", true),
                new SqlParameter("@voucherId", voucher.VoucherNumber));

            voucher.PostedBy = user;
            DB.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK, "Updated Successfully");
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(int id)
        {
            DB.Voucher.Remove(DB.Voucher.Find(id));
            DB.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK, "Voucher Deleted");
        }
    }
}