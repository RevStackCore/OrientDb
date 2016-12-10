using System;
using System.Data;
using System.Data.Common;

namespace RevStackCore.OrientDb.Client
{
    public class OrientDbTransaction : DbTransaction
    {
        private OrientDbConnection _connection;

        public OrientDbTransaction(OrientDbConnection connection)
        {
            _connection = connection;
        }

        public override void Commit() { }

        override protected DbConnection DbConnection
        {
            get { return _connection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return IsolationLevel.Unspecified; }
        }

        public override void Rollback() { }

        public void Dispose()
        {
            _connection = null;
        }
    }
}
