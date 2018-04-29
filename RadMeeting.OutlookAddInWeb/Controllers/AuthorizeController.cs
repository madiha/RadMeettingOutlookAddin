using Newtonsoft.Json;
using RedMeeting.OutlookAddInWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RedMeeting.OutlookAddInWeb.Controllers
{
    public class AuthorizeController : Controller
    {
        private readonly radMeetingsDbContext dbContext;

        public AuthorizeController()
        {
            dbContext = new radMeetingsDbContext();
        }

        // GET: Authorize
        public ActionResult Index(string code, string state)
        {
            var emailAddress = string.Empty;
            if (!string.IsNullOrEmpty(state))
            {
                var stateParams = state.Split('~');
                emailAddress = stateParams[1];
                if (!IsValidEmail(emailAddress))
                {
                    ViewBag.error = "Email address included into state information is not valid.";
                    return View();
                }
            }

            string client_id = ConfigurationManager.AppSettings["clientId"];
            string client_secret = ConfigurationManager.AppSettings["clientSecret"];
            string redirect_url = ConfigurationManager.AppSettings["redirect_url"];
            string scope = ConfigurationManager.AppSettings["scopes"];
            string source = ConfigurationManager.AppSettings["source"];
            string body = string.Format("client_id={0}&scope={1}&code={2}&grant_type=authorization_code&client_secret={3}&redirect_uri={4}&state={5}",
               client_id, scope, code, client_secret, redirect_url, state);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["authEndpoint"]);
                request.Method = "POST";

                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(body);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                var dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();

                string responsestring = null;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        // Read the content.
                        responsestring = reader.ReadToEnd();
                        var tokenResult = JsonConvert.DeserializeObject<TokenResult>(responsestring);

                        // token is success fully generated. 
                        // now store user info into db
                        var userAccount = dbContext.Accounts.FirstOrDefault(x => x.EmailId.Equals(emailAddress));
                        if (userAccount != null)
                        {
                            userAccount.AccessToken = tokenResult.access_token;
                            userAccount.RefreshToken = tokenResult.refresh_token;
                            userAccount.ExpiresIn = tokenResult.expires_in;
                            userAccount.LastModified = DateTime.UtcNow;
                        }
                        else
                        {
                            dbContext.Accounts.Add(new Account
                            {
                                AccessToken = tokenResult.access_token,
                                RefreshToken = tokenResult.refresh_token,
                                EmailId = emailAddress,
                                ExpiresIn = tokenResult.expires_in,
                                LastModified = DateTime.UtcNow
                            });
                        }

                        dbContext.SaveChanges();

                        if (string.IsNullOrEmpty(tokenResult.state)) tokenResult.state = state;
                        ViewBag.result = responsestring;
                        return View(tokenResult);
                    }
                }
            }
            catch (WebException ex)
            {
                using (Stream responseStream = ex.Response.GetResponseStream())
                {
                    using (StreamReader responseReader = new StreamReader(responseStream))
                    {
                        string res = responseReader.ReadToEnd();
                        ViewBag.error = res;
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
            
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}