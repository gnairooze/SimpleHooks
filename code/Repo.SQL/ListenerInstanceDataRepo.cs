using Interfaces;
using Models.Instance;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repo.SQL
{
    public class ListenerInstanceDataRepo : IDataRepository<EventInstance>
    {
        public string ConnectionString { get; set; }

        public object BeginTransaction(object connection)
        {
            throw new NotImplementedException();
        }

        public void CloseConnection(object connection)
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction(object transaction)
        {
            throw new NotImplementedException();
        }

        public EventInstance Create(EventInstance entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public object CreateConnection()
        {
            throw new NotImplementedException();
        }

        public void DisposeConnection(object connection)
        {
            throw new NotImplementedException();
        }

        public EventInstance Edit(EventInstance entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public void OpenConnection(object connection)
        {
            throw new NotImplementedException();
        }

        public List<EventInstance> Read(Dictionary<string, string> options, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public EventInstance Remove(EventInstance entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction(object transaction)
        {
            throw new NotImplementedException();
        }
    }
}
