using System;

namespace Interfaces
{
    public interface IConnectionRepository
    {
        string ConnectionString { get; set; }
        object CreateConnection();
        void OpenConnection(object connection);
        void CloseConnection(object connection);
        void DisposeConnection(object connection);
        Object BeginTransaction(object connection);
        void CommitTransaction(Object transaction);
        void RollbackTransaction(Object transaction);
    }
}
