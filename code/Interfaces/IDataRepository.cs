using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface IDataRepository<T> where T:Models.ModelBase
    {
        T Create(T entity, object connection, Object transaction);
        T Remove(T entity, object connection, Object transaction);
        T Edit(T entity, object connection, Object transaction);
        List<T> Read(Dictionary<string, string> options, object connection);
    }
}
