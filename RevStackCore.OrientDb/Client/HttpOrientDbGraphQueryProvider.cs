using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RevStackCore.OrientDb.Client
{
    public class HttpOrientDbGraphQueryProvider : Query.OrientDbGraphQueryProvider
    {
        OrientDbConnection _connection;

        public HttpOrientDbGraphQueryProvider(OrientDbConnection connection)
        {
            _connection = connection;
        }

        public override object Execute(string query, Type elementType)
        {
            int top = -1;
            //string fetch = "*:-1";
            string fetch = "*:1";

            query = System.Net.WebUtility.UrlDecode(query).Replace("http:/", "http://");
            query = query.Replace("https:/", "https://");
            query = query.Replace("?", "\\u003F");
            query = query.Replace("#", "");

            string url = string.Format("{0}/query/{1}/sql/{2}/{3}", _connection.Server, _connection.Database, System.Net.WebUtility.UrlEncode(query), top);

            if (!string.IsNullOrEmpty(fetch))
            {
                url += "/" + fetch;
            }
            var response = Task.Run(() => HttpClient.SendRequest(url, "GET", string.Empty, _connection.Username, _connection.Password, _connection.SessionId)).Result;

            string body = response.Body;

            if (response.StatusCode != 200)
            {
                //return empty array object
                body = "{ \"result\": [] }";
            }

            //orientdb meta
            body = body.Replace("@rid", "rId");
            body = body.Replace("@class", "_class");
            body = body.Replace("@version", "_version");
            
            var jRoot = JObject.Parse(body);
            var jResults = jRoot.Value<JArray>("result");

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            if (elementType == typeof(Int32))
            {
                return jResults.Count();
            }
            else if (elementType == typeof(bool))
            {
                return jResults.Any();
            }

            object results = JsonConvert.DeserializeObject(jResults.ToString(), typeof(IEnumerable<>).MakeGenericType(elementType), settings);
            return results;
        }
    }
}
