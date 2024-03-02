using AqsaAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.DynamicData;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AqsaAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AccountTreeController : ApiController
    {
        AqsaTradersEntities DB = new AqsaTradersEntities();

        private List<NodeModel> GetNodes(int? level, int? parentCode)
        {
            var nodes = new List<NodeModel>();
            IQueryable<Account> accounts;

            if (level == 2)
            {
                // Second Level init
                accounts = DB.Account.Where(a => a.AccountType != null && a.AccountGroup == null);
            }

            else if (level == 3)
            {
                // Third level init
                accounts = DB.Account.Where(a => a.AccountType != null && a.AccountGroup != null);
            }

            else 
            {
                accounts = DB.Account.Where(a => a.ParentCode == parentCode);
            }

            foreach (var account in accounts)
            {
                var node = new NodeModel
                {
                    name = account.AccountName,
                    attributes = new Dictionary<string, object>
                    {
                        { "AccountCode", account.AccountCode }
                    },
                    children = GetNodes(null, account.AccountCode)
                };
                if (node.children.Count == 0) node.children = null;
                nodes.Add(node);
            }
            return nodes;
        }

        // GET api/<controller>
        public IEnumerable<dynamic> Get(int? level)
        {
            if (level == 1)
            {
                return ( from acc in DB.Account where acc.AccountType == null select new { acc.AccountCode, acc.AccountName, acc.AccountType, acc.AccountGroup, acc.AccountStatus, acc.ParentCode }).ToList();
            }
            else if (level == 2)
            {
                return (from acc in DB.Account where acc.AccountType != null && acc.AccountGroup == null select new { acc.AccountCode, acc.AccountName, acc.AccountType, acc.AccountGroup, acc.AccountStatus, acc.ParentCode }).ToList();
            }
            else
            {
                return (from acc in DB.Account where acc.AccountType != null && acc.AccountGroup != null select new { acc.AccountCode, acc.AccountName, acc.AccountType, acc.AccountGroup, acc.AccountStatus, acc.ParentCode }).ToList();
            }
        }

        // GET api/<controller>/5
        public dynamic Get(bool fromLevel, int? level)
        {
            NodeModel node = new NodeModel
            {
                name = "Chart of Accounts",
                children = !fromLevel? GetNodes(null, null) : GetNodes(level, null),
            };
            return node;
        }

        // POST api/<controller>
        public object Post([FromBody] AccountCodeGetModel model)
        {
            var accountCode = new ObjectParameter("accountCode", typeof(int));
            DB.GetNextChildAccountCode(model.parentCode, model.parentLevel, accountCode);
            return accountCode.Value;
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