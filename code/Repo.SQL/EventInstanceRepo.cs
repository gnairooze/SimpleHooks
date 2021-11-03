using Interfaces;
using Models.Instance;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Repo.SQL
{
    public class EventInstanceRepo : Interfaces.IRepository<Models.Instance.EventInstance>, IDisposable
    {
        protected readonly SqlConnection _Connection;
        public string ConnectionString { get; set; }
        public EventInstanceRepo()
        {
            this._Connection = new SqlConnection(this.ConnectionString);
        }

        public EventInstance Create(EventInstance entity, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENTINSTANCE_ADD, this._Connection, (SqlTransaction)transaction);
            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ACTIVE, entity.Active);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_BY, entity.CreateBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_DATE, entity.CreateDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_DATA, entity.EventDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_DEFINITION_ID, entity.EventDefinitionId);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_INSTANCE_STATUS_ID, entity.Status);
            cmd.Parameters.AddWithValue(Constants.PARAM_ID, entity.Id).Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_BY, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_DATE, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_NOTES, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.PARAM_REFERENCE_NAME, entity.ReferenceName);
            cmd.Parameters.AddWithValue(Constants.PARAM_REFERENCE_VALUE, entity.ReferenceValue);
            cmd.Parameters.AddWithValue(Constants.PARAM_TIMESTAMP, entity.TimeStamp).Direction = System.Data.ParameterDirection.Output;
            #endregion

            if (transaction == null) this.OpenConnection();

            cmd.ExecuteNonQuery();

            entity.Id = (long)cmd.Parameters[Constants.PARAM_ID].Value;
            entity.TimeStamp = (byte[])cmd.Parameters[Constants.PARAM_TIMESTAMP].Value;

            if (transaction == null) this.CloseConnection();

            return entity;
        }

        public EventInstance Remove(EventInstance entity, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENTINSTANCE_REMOVE, this._Connection, (SqlTransaction)transaction);
            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ID, entity.Id);
            cmd.Parameters.AddWithValue(Constants.PARAM_TIMESTAMP, entity.TimeStamp);
            #endregion

            if (transaction == null) this.OpenConnection();

            cmd.ExecuteNonQuery();

            if (transaction == null) this.CloseConnection();

            return entity;
        }

        public EventInstance Edit(EventInstance entity, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENTINSTANCE_EDIT, this._Connection, (SqlTransaction)transaction);
            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ACTIVE, entity.Active);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_BY, entity.CreateBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_DATE, entity.CreateDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_DATA, entity.EventDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_DEFINITION_ID, entity.EventDefinitionId);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_INSTANCE_STATUS_ID, entity.Status);
            cmd.Parameters.AddWithValue(Constants.PARAM_ID, entity.Id);
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_BY, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_DATE, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_NOTES, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.PARAM_REFERENCE_NAME, entity.ReferenceName);
            cmd.Parameters.AddWithValue(Constants.PARAM_REFERENCE_VALUE, entity.ReferenceValue);
            cmd.Parameters.AddWithValue(Constants.PARAM_TIMESTAMP, entity.TimeStamp).Direction = System.Data.ParameterDirection.InputOutput;
            #endregion

            if (transaction == null) this.OpenConnection();

            cmd.ExecuteNonQuery();

            entity.Id = (long)cmd.Parameters[Constants.PARAM_ID].Value;
            entity.TimeStamp = (byte[])cmd.Parameters[Constants.PARAM_TIMESTAMP].Value;

            if (transaction == null) this.CloseConnection();

            return entity;
        }

        public List<EventInstance> Read(Dictionary<string, string> options, object transaction)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            bool isReadNotProcessedOperation = options.ContainsKey(Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString());
            if(isReadNotProcessedOperation)
            {
                DateTime runDate = DateTime.Parse(options[Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString()]);
                return ReadNotProcessed(runDate);
            }

            return new List<EventInstance>();
        }

        private List<EventInstance> ReadNotProcessed(DateTime runDate)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENTINSTANCE_ADD, this._Connection);
            cmd.Parameters.AddWithValue(Constants.PARAM_DATE, runDate);

            this.OpenConnection();

            var reader = cmd.ExecuteReader();

            //todo: dev the fill objects

            this.CloseConnection();

            throw new NotImplementedException();
        }

        public void OpenConnection() 
        {
            this._Connection.Open();
        }
        public void CloseConnection() 
        {
            this._Connection.Close();
        }

        public Object BeginTransaction() 
        {
            return this._Connection.BeginTransaction(); 
        }
        public void CommitTransaction(Object transaction) 
        {
            ((SqlTransaction)transaction).Commit();
        }
        public void RollbackTransaction(Object transaction) 
        {
            ((SqlTransaction)transaction).Rollback();
        }

        public void Dispose()
        {
            this._Connection.Dispose();
        }
    }
}
