using Interfaces;
using Models.Instance;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Repo.SQL
{
    public class ListenerInstanceDataRepo : IDataRepository<ListenerInstance>
    {
        public ListenerInstance Create(ListenerInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_LISTENER_INSTANCE_ADD, (SqlConnection)connection, (SqlTransaction)transaction);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ACTIVE, entity.Active);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_BY, entity.CreateBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_CREATE_DATE, entity.CreateDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_ID, System.Data.SqlDbType.BigInt).Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_BY, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_DATE, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_NOTES, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.PARAM_TIMESTAMP, System.Data.SqlDbType.Timestamp).Direction = System.Data.ParameterDirection.InputOutput;
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_INSTANCE_ID, entity.EventInstanceId);
            cmd.Parameters.AddWithValue(Constants.PARAM_LISTENER_DEFINITION_ID, entity.ListenerDefinitionId);
            cmd.Parameters.AddWithValue(Constants.PARAM_LISTENER_INSTANCE_STATUS_ID, entity.Status);
            cmd.Parameters.AddWithValue(Constants.PARAM_REMAINING_TRIAL_COUNT, entity.RemainingTrialCount);
            cmd.Parameters.AddWithValue(Constants.PARAM_NEXT_RUN, entity.NextRun);

            #endregion

            cmd.ExecuteNonQuery();

            entity.Id = long.Parse(cmd.Parameters[Constants.PARAM_ID].Value.ToString());

            int intTimestamp = (int)cmd.Parameters[Constants.PARAM_TIMESTAMP].Value;
            byte[] intBytes = BitConverter.GetBytes(intTimestamp);
            Array.Reverse(intBytes);
            entity.TimeStamp = intBytes;

            return entity;
        }

        public ListenerInstance Edit(ListenerInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_LISTENER_INSTANCE_EDIT, (SqlConnection)connection, (SqlTransaction)transaction);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ACTIVE, entity.Active);
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_INSTANCE_STATUS_ID, entity.Status);
            cmd.Parameters.AddWithValue(Constants.PARAM_ID, entity.Id);
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_BY, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.PARAM_MODIFY_DATE, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.PARAM_NOTES, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.PARAM_TIMESTAMP, entity.TimeStamp).Direction = System.Data.ParameterDirection.InputOutput;
            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_INSTANCE_ID, entity.EventInstanceId);
            cmd.Parameters.AddWithValue(Constants.PARAM_LISTENER_DEFINITION_ID, entity.ListenerDefinitionId);
            cmd.Parameters.AddWithValue(Constants.PARAM_LISTENER_INSTANCE_STATUS_ID, entity.Status);
            cmd.Parameters.AddWithValue(Constants.PARAM_REMAINING_TRIAL_COUNT, entity.RemainingTrialCount);
            cmd.Parameters.AddWithValue(Constants.PARAM_NEXT_RUN, entity.NextRun);
            #endregion

            cmd.ExecuteNonQuery();

            entity.TimeStamp = (byte[])cmd.Parameters[Constants.PARAM_TIMESTAMP].Value;

            return entity;
        }

        public List<ListenerInstance> Read(Dictionary<string, string> options, object connection, object transaction)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            bool isReadByEventInstanceIdOperation = options.ContainsKey(Models.Instance.Enums.ListenerInstanceReadOperations.ReadByEventInstanceId.ToString());
            if (isReadByEventInstanceIdOperation)
            {
                long eventInstanceId = long.Parse(options[Models.Instance.Enums.ListenerInstanceReadOperations.ReadByEventInstanceId.ToString()]);
                return ReadByEventInstanceId(eventInstanceId, (SqlConnection)connection);
            }

            return new List<ListenerInstance>();
        }

        private List<ListenerInstance> ReadByEventInstanceId(long eventInstanceId, SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_LISTENER_INSTANCE_GET_BY_EVENT_INSTANCE_ID, (SqlConnection)connection);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(Constants.PARAM_EVENT_INSTANCE_ID, eventInstanceId);

            var reader = cmd.ExecuteReader();

            List<ListenerInstance> listenerInstances = ListenerInstanceDataRepo.FillListenerInstances(reader);

            reader.Close();

            return listenerInstances;
        }

        public static List<ListenerInstance> FillListenerInstances(SqlDataReader reader)
        {
            List<ListenerInstance> listenerInstances = new List<ListenerInstance>();

            while (reader.Read())
            {
                ListenerInstance instance = new ListenerInstance();

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
                        case Constants.FIELD_EVENT_INSTANCE_ID:
                            instance.EventInstanceId = (long)reader[counter];
                            break;
                        case Constants.FIELD_LISTENER_DEFINITION_ID:
                            instance.ListenerDefinitionId = (long)reader[counter];
                            break;
                        case Constants.FIELD_LISTENER_INSTANCE_STATUS_ID:
                            instance.Status = (Enums.ListenerInstanceStatus)reader[counter];
                            break;
                        case Constants.FIELD_REMAINING_TRIAL_COUNT:
                            instance.RemainingTrialCount = (int)reader[counter];
                            break;
                        case Constants.FIELD_NEXT_RUN:
                            instance.NextRun = (DateTime)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                listenerInstances.Add(instance);
            }

            return listenerInstances;
        }

        public ListenerInstance Remove(ListenerInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_LISTENER_INSTANCE_REMOVE, (SqlConnection)connection, (SqlTransaction)transaction);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.PARAM_ID, entity.Id);
            cmd.Parameters.AddWithValue(Constants.PARAM_TIMESTAMP, entity.TimeStamp);
            #endregion

            cmd.ExecuteNonQuery();

            return entity;
        }
    }
}
