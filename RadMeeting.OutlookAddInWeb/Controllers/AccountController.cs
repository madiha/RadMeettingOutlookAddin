using RedMeeting.OutlookAddInWeb.Filters;
using RedMeeting.OutlookAddInWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RedMeeting.OutlookAddInWeb.Controllers
{
    [CustomAuthorize]
    public class AccountController : ApiController
    {
        private readonly radMeetingsDbContext dbContext;

        public AccountController()
        {
            dbContext = new radMeetingsDbContext();
        }

        [HttpPost]
        public IHttpActionResult GetAccount(string id)
        {
            var account = dbContext.Accounts.FirstOrDefault(x => x.EmailId.Equals(id));
            if (account == null)
            {
                return BadRequest("Account not found");
            }

            return Ok(account.AccessToken);
        }
    }
}
