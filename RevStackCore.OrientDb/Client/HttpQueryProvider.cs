using System;
using System.Collections.Generic;
using RevStackCore.OrientDb.Query;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RevStackCore.OrientDb.Client
{
    public class HttpQueryProvider : QueryProvider
    {
        OrientDbConnection _connection;

        public HttpQueryProvider(OrientDbConnection connection)
        {
            _connection = connection;
        }

        public override object Execute(Expression expression)
        {
            int top = -1;
            string fetch = "*:-1"; 

            string query = this.Translate(expression);
            Type elementType = TypeSystem.GetElementType(expression.Type);

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

            if (response.StatusCode != 200)
            {
                throw new RestException
                {
                    StatusCode = response.StatusCode,
                    Body = response.Body,
                    StatusMessage = response.StatusString,
                    Url = url
                };
            }

            string body = response.Body;
            body = body.Replace("@rid", "_rid");
            var jRoot = JObject.Parse(body);
            var jResults = jRoot.Value<JArray>("result");

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            object results = JsonConvert.DeserializeObject(jResults.ToString(), typeof(IEnumerable<>).MakeGenericType(elementType), settings);
            return results;
        }
    }
}
