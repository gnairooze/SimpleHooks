using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSimpleHooks.Repos
{
    internal class GenericRepo<T> : Interfaces.IRepository<T> where T : Models.ModelBase
    {
        private readonly List<T> _Entities = new();
        private static long _Counter = 2;
        public List<T> Entities
        {
            get
            {
                return this._Entities;
            }
        }

        public string ConnectionString { get; set; }

        public T Create(T entity, Object transaction)
        {
            entity.Id = GenericRepo<T>._Counter++;

            this.Entities.Add(entity);

            return entity;
        }

        public T Edit(T entity, Object transaction)
        {
            var savedEntity = this.Entities.Where(e => e.Id == entity.Id).Single();
            savedEntity = entity;

            return savedEntity;
        }

        public List<T> Read(Dictionary<string, string> options, Object transaction)
        {
            return this.Entities;
        }

        public T Remove(T entity, Object transaction)
        {
            var savedEntity = this.Entities.Where(e => e.Id == entity.Id).Single();
            this.Entities.Remove(entity);

            return savedEntity;
        }

        public void OpenConnection() { }
        public void CloseConnection() { }
        public Object BeginTransaction() { return null; }
        public void CommitTransaction(Object transaction) { }
        public void RollbackTransaction(Object transaction) { }
    }
}
