using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTools.SimpleHooks.TestSimpleHooks.Repos
{
    internal class GenericRepo<T> : SimpleTools.SimpleHooks.Interfaces.IDataRepository<T> where T : SimpleTools.SimpleHooks.Models.ModelBase
    {
        private static long _counter = 2;

        protected List<T> Entities { get; } = [];

        public T Create(T entity, object connection, Object transaction)
        {
            entity.Id = GenericRepo<T>._counter++;

            this.Entities.Add(entity);

            return entity;
        }

        public T Edit(T entity, object connection, Object transaction)
        {
            var savedEntity = this.Entities.Single(e => e.Id == entity.Id);
            savedEntity = entity;

            return savedEntity;
        }

        public List<T> Read(Dictionary<string, string> options, object connection)
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
