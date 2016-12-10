using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb.Client
{
    public class OrientDbConnection : DbConnection, IDbConnection
    {
        #region vars
        private string _server = string.Empty;
        private string _database = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _connected = false;
        private string _sessionId = string.Empty;
        private string _connectionString = string.Empty;
        private ConnectionState _state;
        #endregion

        public OrientDbConnection()
        {
            ParseConnectionString();
            _state = ConnectionState.Closed;
        }

        public OrientDbConnection(string connectionString)
        {
            ConnectionString = connectionString;
            ParseConnectionString();
            _state = ConnectionState.Closed;
        }

        private void ParseConnectionString()
        {
            if (string.IsNullOrEmpty(ConnectionString)
                || _connectionString.Split(';').Count() < 4)
                throw new Exception("Invalid connection string.");

            string[] conn = _connectionString.Split(';');
            _server = conn[0].Replace("server=", "");
            _database = conn[1].Replace("database=", "");
            _username = conn[2].Replace("user=", "");
            _password = conn[3].Replace("password=", "");
        }

        public bool Connected
        {
            get { return _connected; }
        }

        private void Connect()
        {
            if (_connected)
                return;

            string url = string.Format("{0}/connect/{1}", _server, _database);
            var response = Task.Run(() => HttpClient.SendRequest(url, "GET", "", _username, _password, _sessionId)).Result;

            if (response.StatusCode == 0)
            {
                throw new Exception("Server not running.");
            }
            else if (response.StatusCode == 401)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            else if (response.StatusCode != 200 && response.StatusCode != 204)
            {
                throw new Exception(string.Format("{0} {1}", response.StatusCode, response.StatusString));
            }

            _sessionId = response.OSessionId;
            _connected = true;
            _state = ConnectionState.Open;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            if (this.State == ConnectionState.Closed)
                this.Connect();

            return new OrientDbTransaction(this);
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            if (!_connected)
                return;

            string url = string.Format("{0}/disconnect", _server);
            var response = Task.Run(() => HttpClient.SendRequest(url, "GET", "", _username, _password, _sessionId)).Result;

            if (response.StatusCode == 200)
            {
                _sessionId = string.Empty;
                _connected = false;
            }

            this._state = ConnectionState.Closed;
        }

        public override string ConnectionString
        {
            get
            {
                return this._connectionString;
            }
            set
            {
                this._connectionString = value;
                ParseConnectionString();
            }
        }

        public override string DataSource
        {
            get { return "OrientDb"; }
        }

        public override string ServerVersion
        {
            get { return "2.1"; }
        }

        public string SessionId
        {
            get { return _sessionId; }
        }

        public override int ConnectionTimeout
        {
            get { throw new NotImplementedException(); }
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }

        public override string Database
        {
            get { return _database; }
        }

        public string Server
        {
            get { return _server; }
        }

        public string Username
        {
            get { return _username; }
        }

        public string Password
        {
            get { return _password; }
        }

        public override void Open()
        {
            if (State == ConnectionState.Closed)
                Connect();

            _state = ConnectionState.Open;
        }

        public override ConnectionState State
        {
            get { return this._state; }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
