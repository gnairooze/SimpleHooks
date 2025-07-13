using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Models.Instance;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SimpleTools.SimpleHooks.Repo.SQL
{
    public class ListenerInstanceDataRepo : IDataRepository<ListenerInstance>
    {
        public ListenerInstance Create(ListenerInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpListenerInstanceAdd, (SqlConnection)connection, (SqlTransaction)transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.ParamActive, entity.Active);
            cmd.Parameters.AddWithValue(Constants.ParamCreateBy, entity.CreateBy);
            cmd.Parameters.AddWithValue(Constants.ParamCreateDate, entity.CreateDate);
            cmd.Parameters.AddWithValue(Constants.ParamId, System.Data.SqlDbType.BigInt).Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.AddWithValue(Constants.ParamModifyBy, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.ParamModifyDate, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.ParamNotes, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.ParamEventInstanceId, entity.EventInstanceId);
            cmd.Parameters.AddWithValue(Constants.ParamListenerDefinitionId, entity.ListenerDefinitionId);
            cmd.Parameters.AddWithValue(Constants.ParamListenerInstanceStatusId, (int)entity.Status);
            cmd.Parameters.AddWithValue(Constants.ParamRemainingTrialCount, entity.RemainingTrialCount);
            cmd.Parameters.AddWithValue(Constants.ParamNextRun, entity.NextRun);

            #endregion

            cmd.ExecuteNonQuery();

            entity.Id = long.Parse(cmd.Parameters[Constants.ParamId].Value.ToString());

            return entity;
        }

        public ListenerInstance Edit(ListenerInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpListenerInstanceEdit, (SqlConnection)connection, (SqlTransaction)transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.ParamActive, entity.Active);
            cmd.Parameters.AddWithValue(Constants.ParamId, entity.Id);
            cmd.Parameters.AddWithValue(Constants.ParamModifyBy, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.ParamModifyDate, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.ParamNotes, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.ParamEventInstanceId, entity.EventInstanceId);
            cmd.Parameters.AddWithValue(Constants.ParamListenerDefinitionId, entity.ListenerDefinitionId);
            cmd.Parameters.AddWithValue(Constants.ParamListenerInstanceStatusId, (int)entity.Status);
            cmd.Parameters.AddWithValue(Constants.ParamRemainingTrialCount, entity.RemainingTrialCount);
            cmd.Parameters.AddWithValue(Constants.ParamNextRun, entity.NextRun);
            #endregion

            cmd.ExecuteNonQuery();

            return entity;
        }

        public List<ListenerInstance> Read(Dictionary<string, string> options, object connection)
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
            SqlCommand cmd = new SqlCommand(Constants.SpListenerInstanceGetByEventInstanceId, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(Constants.ParamEventInstanceId, eventInstanceId);

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
                        case Constants.FieldActive:
                            instance.Active = (bool)reader[counter];
                            break;
                        case Constants.FieldCreateBy:
                            instance.CreateBy = (string)reader[counter];
                            break;
                        case Constants.FieldCreateDate:
                            instance.CreateDate = (DateTime)reader[counter];
                            break;
                        case Constants.FieldId:
                            instance.Id = (long)reader[counter];
                            break;
                        case Constants.FieldModifyBy:
                            instance.ModifyBy = (string)reader[counter];
                            break;
                        case Constants.FieldModifyDate:
                            instance.ModifyDate = (DateTime)reader[counter];
                            break;
                        case Constants.FieldNotes:
                            instance.Notes = (string)reader[counter];
                            break;
                        case Constants.FieldTimestamp:
                            instance.TimeStamp = (byte[])reader[counter];
                            break;
                        case Constants.FieldEventInstanceId:
                            instance.EventInstanceId = (long)reader[counter];
                            break;
                        case Constants.FieldListenerDefinitionId:
                            instance.ListenerDefinitionId = (long)reader[counter];
                            break;
                        case Constants.FieldListenerInstanceStatusId:
                            instance.Status = (Enums.ListenerInstanceStatus)reader[counter];
                            break;
                        case Constants.FieldRemainingTrialCount:
                            instance.RemainingTrialCount = (int)reader[counter];
                            break;
                        case Constants.FieldNextRun:
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
            SqlCommand cmd = new SqlCommand(Constants.SpListenerInstanceRemove, (SqlConnection)connection, (SqlTransaction)transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.ParamId, entity.Id);
            #endregion

            cmd.ExecuteNonQuery();

            return entity;
        }
    }
}
