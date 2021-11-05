using Interfaces;
using Models.Instance;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Repo.SQL
{
    public class EventInstanceDataRepo : IDataRepository<EventInstance>
    {
        public EventInstance Create(EventInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENT_INSTANCE_ADD, (SqlConnection)connection, (SqlTransaction)transaction);
            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ACTIVE, entity.Active);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_BY, entity.CreateBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_DATE, entity.CreateDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_DATA, entity.EventData);
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

              cmd.ExecuteNonQuery();

            entity.Id = (long)cmd.Parameters[Constants.PARAM_ID].Value;
            entity.TimeStamp = (byte[])cmd.Parameters[Constants.PARAM_TIMESTAMP].Value;

            return entity;
        }

        public EventInstance Remove(EventInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENT_INSTANCE_REMOVE, (SqlConnection)connection, (SqlTransaction)transaction);
            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ID, entity.Id);
            cmd.Parameters.AddWithValue(Constants.PARAM_TIMESTAMP, entity.TimeStamp);
            #endregion

            cmd.ExecuteNonQuery();

            return entity;
        }

        public EventInstance Edit(EventInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENT_INSTANCE_EDIT, (SqlConnection) connection, (SqlTransaction)transaction);
            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ACTIVE, entity.Active);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_DATA, entity.EventData);
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

            cmd.ExecuteNonQuery();

            entity.TimeStamp = (byte[])cmd.Parameters[Constants.PARAM_TIMESTAMP].Value;

            return entity;
        }

        #region read data
        public List<EventInstance> Read(Dictionary<string, string> options, object connection, object transaction)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            bool isReadNotProcessedOperation = options.ContainsKey(Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString());
            if(isReadNotProcessedOperation)
            {
                DateTime runDate = DateTime.Parse(options[Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString()]);
                return ReadNotProcessed(runDate, (SqlConnection)connection);
            }

            return new List<EventInstance>();
        }

        private List<EventInstance> ReadNotProcessed(DateTime runDate, SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENT_INSTANCE_GET_NOT_PROCESSED, (SqlConnection) connection);
            cmd.Parameters.AddWithValue(Constants.PARAM_DATE, runDate);

            var reader = cmd.ExecuteReader();

            List<EventInstance> events = FillEventInstances(reader);

            return events;
        }

        private List<EventInstance> FillEventInstances(SqlDataReader reader)
        {
            List<EventInstance> results = new List<EventInstance>();

            FillEventInstancesOnly(reader, results);

            reader.NextResult();

            var listenerInstances = ListenerInstanceDataRepo.FillListenerInstances(reader);

            OrganizeEventInstances(results, listenerInstances);

            return results;
        }

        private void OrganizeEventInstances(List<EventInstance> results, List<ListenerInstance> listenerInstances)
        {
            foreach (var eventInstance in results)
            {
                eventInstance.ListenerInstances.AddRange(listenerInstances.Where(l => l.EventInstanceId == eventInstance.Id));
            }
        }

        private void FillEventInstancesOnly(SqlDataReader reader, List<EventInstance> results)
        {
            while (reader.Read())
            {
                EventInstance instance = new EventInstance();

                int counter = 0;

                while (counter < reader.FieldCount)
                {
                    if (reader[counter] == DBNull.Value)
                    {
                        counter++;
                        continue;
                    }

                    #region read event instance fields
                    switch (reader.GetName(counter))
                    {
                        case Constants.FIELD_ACTIVE:
                            instance.Active = (bool)reader[counter];
                            break;
                        case Constants.FIELD_CREATE_BY:
                            instance.CreateBy = (string)reader[counter];
                            break;
                        case Constants.FIELD_CREATE_DATE:
                            instance.CreateDate = (DateTime)reader[counter];
                            break;
                        case Constants.FIELD_ID:
                            instance.Id = (long)reader[counter];
                            break;
                        case Constants.FIELD_MODIFY_BY:
                            instance.ModifyBy = (string)reader[counter];
                            break;
                        case Constants.FIELD_MODIFY_DATE:
                            instance.ModifyDate = (DateTime)reader[counter];
                            break;
                        case Constants.FIELD_NOTES:
                            instance.Notes = (string)reader[counter];
                            break;
                        case Constants.FIELD_TIMESTAMP:
                            instance.TimeStamp = (byte[])reader[counter];
                            break;
                        case Constants.FIELD_EVENT_DATA:
                            instance.EventData = (string)reader[counter];
                            break;
                        case Constants.FIELD_EVENT_DEFINITION_ID:
                            instance.EventDefinitionId = (long)reader[counter];
                            break;
                        case Constants.FIELD_EVENT_INSTANCE_STATUS_ID:
                            instance.Status = (Enums.EventInstanceStatus)reader[counter];
                            break;
                        case Constants.FIELD_REFERENCE_NAME:
                            instance.ReferenceName = (string)reader[counter];
                            break;
                        case Constants.FIELD_REFERENCE_VALUE:
                            instance.ReferenceValue = (string)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                results.Add(instance);
            }
        }
        #endregion
    }
}
