using RedMeeting.OutlookAddInWeb.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RedMeeting.OutlookAddInWeb.Filters
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            //base.OnAuthorization(actionContext);
            if (actionContext.Request.Headers.GetValues("Authorization") != null)
            {
                // get value from header
                string identityToken = Convert.ToString(actionContext.Request.Headers.GetValues("Authorization").FirstOrDefault());
                ExchangeIdToken idToken = new ExchangeIdToken(identityToken.Replace("identityToken", "").Trim());
                var addInUrl = ConfigurationManager.AppSettings["addInUrl"];
                var result = idToken.Validate(addInUrl);
                if (!result.IsValid)
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                    return;
                }

                HttpContext.Current.Response.AddHeader("Authorization", identityToken);
                HttpContext.Current.Response.AddHeader("AuthenticationStatus", "Authorized");
                return;
            }

            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            actionContext.Response.ReasonPhrase = "Identity token is missing into header.";
        }
    }
}