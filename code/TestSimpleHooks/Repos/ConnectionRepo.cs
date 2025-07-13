using System;

namespace SimpleTools.SimpleHooks.TestSimpleHooks.Repos
{
    internal class ConnectionRepo : SimpleTools.SimpleHooks.Interfaces.IConnectionRepository
    {
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object BeginTransaction(object connection)
        {
            return new object();
        }

        public void CloseConnection(object connection)
        {
            
        }

        public void CommitTransaction(object transaction)
        {
            
        }

        public object CreateConnection()
        {
            return new object();
        }

        public void DisposeConnection(object connection)
        {
            
        }

        public void OpenConnection(object connection)
        {
            
        }

        public void RollbackTransaction(object transaction)
        {
            
        }
    }
}
