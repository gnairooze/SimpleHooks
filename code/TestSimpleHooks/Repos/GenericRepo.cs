using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSimpleHooks.Repos
{
    internal class GenericRepo<T> : Interfaces.IDataRepository<T> where T : Models.ModelBase
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

        public T Create(T entity, object connection, Object transaction)
        {
            entity.Id = GenericRepo<T>._Counter++;

            this.Entities.Add(entity);

            return entity;
        }

        public T Edit(T entity, object connection, Object transaction)
        {
            var savedEntity = this.Entities.Where(e => e.Id == entity.Id).Single();
            savedEntity = entity;

            return savedEntity;
        }

        public List<T> Read(Dictionary<string, string> options, object connection, Object transaction)
        {
            return this.Entities;
        }

        public T Remove(T entity, object connection, Object transaction)
        {
            var savedEntity = this.Entities.Where(e => e.Id == entity.Id).Single();
            this.Entities.Remove(entity);

            return savedEntity;
        }
    }
}
