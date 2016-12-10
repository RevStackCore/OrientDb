using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevStackCore.OrientDb.Client;
using System.Data;
using System.Reflection;
using System.Linq.Expressions; 

namespace RevStackCore.OrientDb
{
    public class OrientDbDatabase : IDisposable
    {
        private OrientDbConnection _connection;

        private OrientDbDatabase() { }

        public OrientDbDatabase(IDbConnection connection)
        {
            _connection = (OrientDbConnection)connection;
        }

        public OrientDbDatabase(string connectionString)
        {
            _connection = new OrientDbConnection(connectionString);
        }

        public TEntity Insert<TEntity>(TEntity entity)
        {
            return this.InsertInternal(entity);
        }

        public TEntity Update<TEntity>(TEntity entity)
        {
            return this.UpdateInternal(entity);
        }

        public void Delete<TEntity>(TEntity entity)
        {
            this.DeleteInternal<TEntity>(entity);
        }

        public void Batch<TEntity>(IList<TEntity> entity)
        {
            _connection.Open();
            OrientDbTransaction transaction = (OrientDbTransaction)_connection.BeginTransaction();
            
            try
            {
                using (OrientDbCommand command = new OrientDbCommand())
                {
                    command.Connection = _connection;
                    command.Transaction = transaction;
                    command.Batch(entity);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
            }
            finally
            {
                _connection.Close();
            }
        }

        #region "private methods"
        private TEntity InsertInternal<TEntity>(TEntity entity)
        {
            _connection.Open();
            OrientDbTransaction transaction = (OrientDbTransaction)_connection.BeginTransaction();
            TEntity result = default(TEntity);

            try
            {
                using (OrientDbCommand command = new OrientDbCommand())
                {
                    command.Connection = _connection;
                    command.Transaction = transaction;
                    result = command.Insert<TEntity>(entity);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return result;
            }
            finally
            {
                _connection.Close();
            }

            return result;
        }

        private TEntity UpdateInternal<TEntity>(TEntity entity)
        {
            _connection.Open();
            OrientDbTransaction transaction = (OrientDbTransaction)_connection.BeginTransaction();
            TEntity result = default(TEntity);

            try
            {
                using (OrientDbCommand command = new OrientDbCommand())
                {
                    command.Connection = _connection;
                    command.Transaction = transaction;
                    result = command.Update<TEntity>(entity);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return result;
            }
            finally
            {
                _connection.Close();
            }

            return result;
        }

        private void DeleteInternal<TEntity>(TEntity entity)
        {
            _connection.Open();
            OrientDbTransaction transaction = (OrientDbTransaction)_connection.BeginTransaction();
            
            try
            {
                using (OrientDbCommand command = new OrientDbCommand())
                {
                    command.Connection = _connection;
                    command.Transaction = transaction;
                    command.Delete<TEntity>(entity);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
            }
            finally
            {
                _connection.Close();
            }
        }

        public string Execute(string sql)
        {
            _connection.Open();
            OrientDbTransaction transaction = (OrientDbTransaction)_connection.BeginTransaction();
            string result = "0";

            try
            {
                using (OrientDbCommand command = new OrientDbCommand())
                {
                    command.Connection = _connection;
                    command.Transaction = transaction;
                    result = command.Execute(sql);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return result;
            }
            finally
            {
                _connection.Close();
            }

            return result;
        }
        
        #endregion

        public void Dispose()
        {
            _connection = null;
        }
    }
}
