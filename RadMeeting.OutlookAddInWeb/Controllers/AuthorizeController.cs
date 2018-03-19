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
        // GET: Authorize
        public ActionResult Index(string code, string state)
        {
            string client_id = ConfigurationManager.AppSettings["clientId"];
            string client_secret = ConfigurationManager.AppSettings["clientSecret"];
            string redirect_url = ConfigurationManager.AppSettings["redirect_url"];
            string scope = ConfigurationManager.AppSettings["scopes"];
            string source = ConfigurationManager.AppSettings["source"];
            string body = string.Format("client_id={0}&scope={1}&code={2}&grant_type=authorization_code&client_secret={3}&redirect_uri={4}",
               client_id, scope, code, client_secret, redirect_url);

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
    }
}