using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface IRepository<T> where T:Models.ModelBase
    {
        string ConnectionString { get; set; }

        T Create(T entity, Object transaction);
        T Remove(T entity, Object transaction);
        T Edit(T entity, Object transaction);
        List<T> Read(Dictionary<string, string> options, Object transaction);

        void OpenConnection();
        void CloseConnection();
        Object BeginTransaction();
        void CommitTransaction(Object transaction);
        void RollbackTransaction(Object transaction);
    }
}
