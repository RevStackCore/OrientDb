using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
//using System.Web;
using RevStackCore.OrientDb.Utils;
using System.Reflection;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb.Client
{
    public class OrientDbCommand : IDisposable
    {
        public OrientDbCommand() { }

        public OrientDbCommand(string commandText)
        {
            CommandText = commandText;
        }

        public OrientDbCommand(OrientDbConnection connection, OrientDbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public OrientDbCommand(string commandText, OrientDbConnection connection, OrientDbTransaction transaction)
        {
            CommandText = commandText;
            Connection = connection;
            Transaction = transaction;
        }

        public string CommandText { get; set; }
        public OrientDbConnection Connection { get; set; }
        public OrientDbTransaction Transaction { get; set; }

        public TEntity Insert<TEntity>(TEntity entity)
        {
            Type type = entity.GetType();
            var info = type.GetProperty("Id");
            if (info == null)
                throw new Exception("Id is required.");

            entity = OrientDbUtils.SetEntityIdProperty<TEntity>(entity);
            string query = OrientDbUtils.GetEntityIdQueryFormat<TEntity>(entity);
            string jEntity = CamelCaseJsonSerializer.SerializeObject(entity);
            
            string name = entity.GetType().Name;

            // orientdb meta for insert 
            if (jEntity.Contains("\"rId\""))
            {
                jEntity = jEntity.Replace("\"rId\"", "\"@rid\"");
            }

            if (jEntity.Contains("_class"))
            {
                jEntity = jEntity.Replace("_class", "@class");
            }

            JObject json = JObject.Parse(jEntity);

            if (json["@class"] == null || string.IsNullOrEmpty(json["@class"].ToString()))
            {
                json["@class"] = name;
            }

            name = json["@class"].ToString();
            
            InsertInternal(json);
            
            return this.Find<TEntity>("select from " + name + " where " + query, -1, "*:-1").SingleOrDefault();
        }

        public TEntity Update<TEntity>(TEntity entity)
        {
            Type type = entity.GetType();
            var info = type.GetProperty("Id");
            if (info == null)
                throw new Exception("Id is required.");

            string query = OrientDbUtils.GetEntityIdQueryFormat<TEntity>(entity);
            string jEntity = CamelCaseJsonSerializer.SerializeObject(entity);

            string name = entity.GetType().Name;

            // orientdb meta for update 
            if (jEntity.Contains("\"rId\""))
            {
                jEntity = jEntity.Replace("\"rId\"", "\"@rid\"");
            }

            if (jEntity.Contains("_class"))
            {
                jEntity = jEntity.Replace("_class", "@class");
            }

            JObject json = JObject.Parse(jEntity);

            if (json["@class"] == null || string.IsNullOrEmpty(json["@class"].ToString()))
            {
                json["@class"] = name;
            }

            name = json["@class"].ToString();

            UpdateInternal(json);
            
            return this.Find<TEntity>("select from " + name + " where " + query, -1, "*:-1").SingleOrDefault();
        }

        public void Delete<TEntity>(TEntity entity)
        {
            Type type = typeof(TEntity);
            string query = OrientDbUtils.GetEntityIdQueryFormat<TEntity>(entity);
            query = "DELETE FROM " + type.Name + " WHERE " + query;
            this.Execute(query);
        }

        public string Execute(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql");

            sql = System.Net.WebUtility.UrlDecode(sql).Replace("http:/", "http://");
            sql = sql.Replace("https:/", "https://");
            sql = sql.Replace("?", "\\u003F");
            sql = sql.Replace("#", "");

            long rv = 0;
            string url = string.Format("{0}/command/{1}/sql/{2}", Connection.Server, Connection.Database, System.Net.WebUtility.UrlEncode(sql));
            var response = Task.Run(()=> HttpClient.SendRequest(url, "POST", string.Empty, Connection.Username, Connection.Password, Connection.SessionId)).Result;

            if (response.StatusCode != 200 && response.StatusCode != 204)
            {
                throw new RestException
                {
                    StatusCode = response.StatusCode,
                    Body = response.Body,
                    StatusMessage = response.StatusString,
                    Url = url
                };
            }

            return response.Body;
        }

        public string Batch<TEntity>(IList<TEntity> entities)
        {
            if (entities.Count == 0)
                return "";

            string jEntities = CamelCaseJsonSerializer.SerializeObject(entities);
            
            //orientdb meta
            jEntities = jEntities.Replace("_class", "@class");
            jEntities = jEntities.Replace("\"rId\"", "\"@rid\"");
            
            JArray json = JArray.Parse(jEntities);
            foreach (JObject entity in json)
            {
                InsertInternal(entity);
            }

            return "";
        }

        #region private
        private IQueryable<TEntity> Find<TEntity>(string sqlQuery, int top = 20, string fetchPlan = "")
        {
            var jResults = Find(sqlQuery, top, fetchPlan);
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.DeserializeObject<IEnumerable<TEntity>>(jResults.ToString(), settings).AsQueryable();
        }

        private JArray Find(string sqlQuery, int top = 20, string fetchPlan = "")
        {
            if (string.IsNullOrEmpty(sqlQuery))
                throw new ArgumentNullException("sql");

            sqlQuery = System.Net.WebUtility.UrlDecode(sqlQuery).Replace("http:/", "http://");
            sqlQuery = sqlQuery.Replace("https:/", "https://");
            sqlQuery = sqlQuery.Replace("?", "\\u003F");
            sqlQuery = sqlQuery.Replace("#", "");

            string url = string.Format("{0}/query/{1}/sql/{2}/{3}", Connection.Server, Connection.Database, System.Net.WebUtility.UrlEncode(sqlQuery), top);

            if (!string.IsNullOrEmpty(fetchPlan))
            {
                url += "/" + fetchPlan;
            }
            var response = Task.Run(() => HttpClient.SendRequest(url, "GET", string.Empty, Connection.Username, Connection.Password, Connection.SessionId)).Result;

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
            
            //orientdb meta
            body = body.Replace("@rid", "rId");
            body = body.Replace("@class", "_class");
            body = body.Replace("@version", "_version");
            //body = body.Replace("@type", "_type");
            //body = body.Replace("@created", "_created");
            //body = body.Replace("@modified", "_modified");

            var jRoot = JObject.Parse(body);
            var jResults = jRoot.Value<JArray>("result");

            return jResults;
        }

        private string CreateClass(string className, string typeName)
        {
            if (string.IsNullOrEmpty(className))
                throw new ArgumentNullException("className");

            string cid = "";
            string url = string.Format("{0}/class/{1}/{2}", Connection.Server, Connection.Database, className);
            var response = Task.Run(() => HttpClient.SendRequest(url, "POST", className, Connection.Username, Connection.Password, Connection.SessionId)).Result;
            if (response.StatusCode != 201)
            {
                return null;
            }

            cid = response.Body;

            //DEPRICATED
            //create property
            //url = string.Format("{0}/property/{1}/{2}/{3}", Connection.Server, Connection.Database, className, "name");
            //response = Task.Run(() => HttpClient.SendRequest(url, "POST", string.Empty, Connection.Username, Connection.Password, Connection.SessionId)).Result;
            //create property
            //url = string.Format("{0}/property/{1}/{2}/{3}", Connection.Server, Connection.Database, className, "policies");
            //response = Task.Run(() => HttpClient.SendRequest(url, "POST", string.Empty, Connection.Username, Connection.Password, Connection.SessionId)).Result;

            //create index
            this.Execute("CREATE PROPERTY " + className + ".id " + typeName);
            this.Execute("CREATE INDEX " + className + ".id UNIQUE");

            return cid;
        }

        private void InsertInternal(JObject entity)
        {
            string typeName = OrientDbUtils.GetEntityIdJToken(entity["id"]);
            string name = entity["@class"].ToString();
            entity["id"] = entity["id"];

            //orientdb meta
            entity["@class"] = name.Replace("\"", "");
            entity["@rid"] = "-1:-1";
            entity["@version"] = 0;

            //remove meta for insert
            entity.Remove("_version");

            //depricated
            //entity["@type"] = "d";
            //entity["@created"] = System.DateTime.UtcNow.ToString("O");
            //entity["@modified"] = System.DateTime.UtcNow.ToString("O");
            //remove meta for insert
            //entity.Remove("_created");
            //entity.Remove("_modified");

            foreach (JToken child in entity.Children().ToList())
                this.RecurseForDocumentInsert(child);

            try
            {
                CreateClass(entity["@class"].ToString(), typeName);
            }
            catch
            {
            }

            string url = string.Format("{0}/document/{1}/-1%3A-1", Connection.Server, Connection.Database);
            string body = entity.ToString();
            body = entity.ToString();

            var response = Task.Run(() => HttpClient.SendRequest(url, "POST", body, Connection.Username, Connection.Password, Connection.SessionId)).Result;
            if (response.StatusCode != 201)
            {
                string sExMessage = string.Format("{0} - {1}", response.StatusCode, response.StatusString);
                if (!string.IsNullOrEmpty(response.Body))
                {
                    sExMessage = response.Body;
                }
                throw new Exception(sExMessage);
            }

            entity["@rid"] = JObject.Parse(response.Body)["@rid"];
        }

        private void RecurseForDocumentInsert(JToken child)
        {
            if (!child.First.HasValues)
                return; 

            var query_where1 = from a in child.Values()
                               where a.SelectToken("@class") != null
                               select a;

            IList<JToken> list = query_where1.ToList();

            if (list.Count > 0)
            {
                foreach (JToken item in list)
                {
                    this.RecurseForDocumentInsert(item);
                }
            }

            JToken t = child.SelectToken("@class", false);
            if (t == null)
            {
                if (child.Children().ToList().Count > 0)
                {
                    IEnumerable<JToken> j = child.Children();

                    query_where1 = from a in j
                                   where a.SelectToken("@class") != null
                                   select a;

                    list = query_where1.ToList();

                    if (list.Count > 0)
                    {
                        foreach (JToken item in list)
                        {
                            this.RecurseForDocumentInsert(item);
                        }
                    }
                }
            }

            if (child.SelectToken("@class", false) != null) //f no children and we have a class then process
            {
                string classname = child["@class"].ToString();

                JToken c = child.SelectToken("@rid", false);

                if (c == null || (c.ToString() == "-1:-1" || c.ToString().Trim() == ""))
                {
                    this.InsertInternal((JObject)child);
                    JObject obj = new JObject();
                    obj[classname] = child["@rid"].ToString();
                    child.Replace(obj[classname].ToString().Replace("{", "").Replace("}", ""));
                }
                else
                {
                    if (child.SelectToken("@class", false) != null)
                    {
                        this.UpdateInternal((JObject)child);
                        JObject obj = new JObject();
                        obj[classname] = child["@rid"];
                        child.Replace(obj[classname].ToString().Replace("{", "").Replace("}", ""));
                    }
                }
            }
        }

        private void UpdateInternal(JObject entity)
        {
            string query = OrientDbUtils.GetEntityIdQueryFormat(entity["id"]);

            JArray entityData = this.Find("select from " + entity["@class"].ToString() + " where " + query, 1, "");
            if (entityData.Count() == 0)
                throw new Exception("Entity " + entity["@class"].ToString() + " does not exist.");

            JObject entityJson = (JObject)entityData[0];

            //orientdb meta
            entity["@rid"] = entityJson["rId"];
            entity["@class"] = entityJson["_class"];
            entity["@version"] = int.Parse(entityJson["_version"].ToString());

            //remove meta for update
            entity.Remove("_version");
            entityJson.Remove("_version");
            entityJson.Remove("_class");
            entityJson.Remove("rId");
            entityJson.Remove("@type");

            //handle graph scenarios
            entity.Remove("in");
            entity.Remove("out");

            //depricated
            //entity["@type"] = entityJson["_type"];
            //entity["@created"] = entityJson["_created"];
            //entity["@modified"] = System.DateTime.UtcNow.ToString("O");
            //remove meta for update
            //entity.Remove("_created");
            //entity.Remove("_modified");
            
            //allowPartial schema - merge properties by default
            List<JProperty> current_props = entityJson.Properties().ToList();
            List<JProperty> updated_props = entity.Properties().ToList();
            foreach (JProperty prop in current_props)
            {
                if (!updated_props.Any(a => a.Name == prop.Name))
                {
                    if (prop.Name != "@rid" &&
                        prop.Name != "@fieldTypes")
                    {
                        entity.Add(prop);
                    }
                } 
            }

            //traverse embedded documents, create and replace with rid
            foreach (JToken child in entity.Children().ToList())
                this.RecurseForDocumentUpdate(child);

            string url = string.Format("{0}/document/{1}/{2}", Connection.Server, Connection.Database, entity["@rid"].ToString().Replace("#", ""));
            string body = entity.ToString();
            var response = Task.Run(() => HttpClient.SendRequest(url, "PUT", body, Connection.Username, Connection.Password, Connection.SessionId)).Result;
            if (response.StatusCode > 201)
            {
                string sExMessage = string.Format("{0} - {1}", response.StatusCode, response.StatusString);
                if (!string.IsNullOrEmpty(response.Body))
                {
                    sExMessage = response.Body;
                }
            }
        }

        private void RecurseForDocumentUpdate(JToken child)
        {
            if (!child.First.HasValues)
                return;

            var query_where1 = from a in child.Values()
                               where a.SelectToken("@class") != null
                               select a;

            IList<JToken> list = query_where1.ToList();

            if (list.Count > 0)
            {
                foreach (JToken item in list)
                {
                    this.RecurseForDocumentUpdate(item);
                }
            }

            var childClass = child.SelectToken("@class", false);
            if (childClass == null)
            {
                return;
            }
                

            JToken c = child.SelectToken("@rid", false);

            if (c == null || (c.ToString() == "-1:-1" || c.ToString().Trim() == ""))
            {
                this.RecurseForDocumentInsert(c);
            }
            else
            {
                if (child.SelectToken("@class", false) != null)
                {
                    string classname = child["@class"].ToString();
                    this.UpdateInternal((JObject)child);
                    JObject obj = new JObject();
                    obj[classname] = child["@rid"];
                    child.Replace(obj[classname].ToString().Replace("{", "").Replace("}", ""));
                }
            }
        }

        private void Delete(string rid)
        {
            string url = string.Format("{0}/document/{1}/{2}", Connection.Server, Connection.Database, rid.Replace("#", ""));
            var response = Task.Run(() => HttpClient.SendRequest(url, "DELETE", "", Connection.Username, Connection.Password, Connection.SessionId)).Result;
            if (response.StatusCode != 204)
            {
                throw new RestException
                {
                    StatusCode = response.StatusCode,
                    Body = response.Body,
                    StatusMessage = response.StatusString,
                    Url = url
                };
            }
        }
        
        #endregion

        public void Dispose()
        {

        }
    }
}
