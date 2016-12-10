using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb.Client
{
    public static class HttpClient
    {
        public static async Task<RestResponse> SendRequest(string url, string method, string body, string username, string password, string sessionId)
        {
            RestResponse rv = new RestResponse
            {
                Headers = new Dictionary<string, string>(),
                Body = string.Empty
            };

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                //request.KeepAlive = false;

                var cred = new NetworkCredential(username, password);
                request.Credentials = cred;
                request.ContentType = "application/json; charset=utf-8";

                if (!string.IsNullOrEmpty(body))
                {
                    var bytes = Encoding.UTF8.GetBytes(body);
                    using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                response = (HttpWebResponse) await request.GetResponseAsync();
                rv.StatusCode = (int)((HttpWebResponse)response).StatusCode;
                rv.StatusString = ((HttpWebResponse)response).StatusDescription;
                rv.ContentLength = response.ContentLength;
                rv.ContentType = response.ContentType;
                response.Headers.AllKeys.ToList().ForEach(o => rv.Headers.Add(o, response.Headers[o]));

            }
            catch (WebException ex)
            {
                rv.Exception = ex;
                if (ex.Response != null)
                {
                    rv.StatusCode = (int)((HttpWebResponse)ex.Response).StatusCode;
                    rv.StatusString = ((HttpWebResponse)ex.Response).StatusDescription;
                    response = (HttpWebResponse)ex.Response;
                }
            }
            catch (Exception ex)
            {
                rv.Exception = ex;
            }

            if (response != null && response.ContentLength > 0)
            {
                string tempString = null;
                int count = 0;
                byte[] buf = new byte[8192];
                StringBuilder sb = new StringBuilder();

                do
                {
                    count = response.GetResponseStream().Read(buf, 0, buf.Length);
                    if (count != 0)
                    {
                        tempString = Encoding.UTF8.GetString(buf, 0, count);
                        sb.Append(tempString);
                    }
                }
                while (count > 0);
                
                rv.Body = sb.ToString();
            }

            if (rv.Headers.ContainsKey("Set-Cookie"))
            {
                if (rv.Headers["Set-Cookie"].Contains("OSESSIONID"))
                {
                    rv.OSessionId = Regex.Match(rv.Headers["Set-Cookie"], "(?<=OSESSIONID=).*?(?=;)", RegexOptions.Compiled).Value;
                }
            }

            return rv;
        }
        
    }
}
