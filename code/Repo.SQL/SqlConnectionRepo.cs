using System;
using System.Data.SqlClient;

namespace SimpleTools.SimpleHooks.Repo.SQL
{
    public class SqlConnectionRepo : SimpleTools.SimpleHooks.Interfaces.IConnectionRepository
    {
        public string ConnectionString { get; set; }

        #region connection
        public object CreateConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }
        public void OpenConnection(object connection)
        {
            ((SqlConnection)connection).Open();
        }
        public void CloseConnection(object connection)
        {
            ((SqlConnection)connection).Close();
        }
        public void DisposeConnection(object connection)
        {
            ((SqlConnection)connection).Dispose();
        }
        #endregion

        #region transaction
        public Object BeginTransaction(object connection)
        {
            return ((SqlConnection)connection).BeginTransaction();
        }
        public void CommitTransaction(Object transaction)
        {
            ((SqlTransaction)transaction).Commit();
        }
        public void RollbackTransaction(Object transaction)
        {
            ((SqlTransaction)transaction).Rollback();
        }
        #endregion
    }
}
