using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Models.Instance;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SimpleTools.SimpleHooks.Repo.SQL
{
    public class EventInstanceDataRepo : IDataRepository<EventInstance>, IDataRepositoryEventInstanceStatus
    {
        public EventInstance Create(EventInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpEventInstanceAdd, (SqlConnection)connection, (SqlTransaction)transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.ParamActive, entity.Active);
            cmd.Parameters.AddWithValue(Constants.ParamBusinessId, entity.BusinessId);
            cmd.Parameters.AddWithValue(Constants.ParamCreateBy, entity.CreateBy);
            cmd.Parameters.AddWithValue(Constants.ParamCreateDate, entity.CreateDate);
            cmd.Parameters.AddWithValue(Constants.ParamEventData, entity.EventData);
            cmd.Parameters.AddWithValue(Constants.ParamEventDefinitionId, entity.EventDefinitionId);
            cmd.Parameters.AddWithValue(Constants.ParamEventInstanceStatusId, (int)entity.Status);
            cmd.Parameters.Add(Constants.ParamId, System.Data.SqlDbType.BigInt).Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.AddWithValue(Constants.ParamModifyBy, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.ParamModifyDate, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.ParamNotes, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.ParamReferenceName, entity.ReferenceName);
            cmd.Parameters.AddWithValue(Constants.ParamReferenceValue, entity.ReferenceValue);
            cmd.Parameters.AddWithValue(Constants.ParamGroupId, entity.GroupId);
            #endregion

            cmd.ExecuteNonQuery();

            entity.Id = (long)cmd.Parameters[Constants.ParamId].Value;

            return entity;
        }

        public EventInstance Remove(EventInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpEventInstanceRemove, (SqlConnection)connection, (SqlTransaction)transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.ParamId, entity.Id);
            #endregion

            cmd.ExecuteNonQuery();

            return entity;
        }

        public EventInstance Edit(EventInstance entity, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpEventInstanceEdit, (SqlConnection)connection, (SqlTransaction)transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            #region add parameters
            cmd.Parameters.AddWithValue(Constants.ParamActive, entity.Active);
            cmd.Parameters.AddWithValue(Constants.ParamBusinessId, entity.BusinessId);
            cmd.Parameters.AddWithValue(Constants.ParamEventData, entity.EventData);
            cmd.Parameters.AddWithValue(Constants.ParamEventDefinitionId, entity.EventDefinitionId);
            cmd.Parameters.AddWithValue(Constants.ParamEventInstanceStatusId, (int)entity.Status);
            cmd.Parameters.AddWithValue(Constants.ParamId, entity.Id);
            cmd.Parameters.AddWithValue(Constants.ParamModifyBy, entity.ModifyBy);
            cmd.Parameters.AddWithValue(Constants.ParamModifyDate, entity.ModifyDate);
            cmd.Parameters.AddWithValue(Constants.ParamNotes, entity.Notes);
            cmd.Parameters.AddWithValue(Constants.ParamReferenceName, entity.ReferenceName);
            cmd.Parameters.AddWithValue(Constants.ParamReferenceValue, entity.ReferenceValue);
            cmd.Parameters.AddWithValue(Constants.ParamGroupId, entity.GroupId);
            #endregion

            cmd.ExecuteNonQuery();

            return entity;
        }

        #region read data
        public List<EventInstance> Read(Dictionary<string, string> options, object connection)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            bool canReadNotProcessedOperation = options.ContainsKey(Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString());

            if (!canReadNotProcessedOperation)
            {
                throw new ArgumentException("date was not provided in Read operation");
            }
            
            bool canReadGroupId = options.ContainsKey(Constants.FieldGroupId);

            if (!canReadGroupId)
            {
                throw new ArgumentException("group id was not provided in Read operation");
            }

            DateTime runDate = DateTime.Parse(options[Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString()]);

            int groupId = int.Parse(options[Constants.FieldGroupId]);

            return ReadNotProcessed(runDate, (SqlConnection)connection, groupId);
        }

        private List<EventInstance> ReadNotProcessed(DateTime runDate, SqlConnection connection, int groupId = 1)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpEventInstanceGetNotProcessed, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(Constants.ParamDate, runDate);
            cmd.Parameters.AddWithValue(Constants.ParamGroupId, groupId);

            var reader = cmd.ExecuteReader();

            List<EventInstance> events = FillEventInstances(reader);

            reader.Close();

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
                        case Constants.FieldActive:
                            instance.Active = (bool)reader[counter];
                            break;
                        case Constants.FieldBusinessId:
                            instance.BusinessId = (Guid)reader[counter];
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
                        case Constants.FieldEventData:
                            instance.EventData = (string)reader[counter];
                            break;
                        case Constants.FieldEventDefinitionId:
                            instance.EventDefinitionId = (long)reader[counter];
                            break;
                        case Constants.FieldEventInstanceStatusId:
                            instance.Status = (Enums.EventInstanceStatus)reader[counter];
                            break;
                        case Constants.FieldReferenceName:
                            instance.ReferenceName = (string)reader[counter];
                            break;
                        case Constants.FieldReferenceValue:
                            instance.ReferenceValue = (string)reader[counter];
                            break;
                        case Constants.FieldGroupId:
                            instance.GroupId = (int)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                results.Add(instance);
            }
        }

        EventInstanceStatusBrief IDataRepositoryEventInstanceStatus.ReadByBusinessId(Guid businessId, object connection)
        {
            if (businessId == Guid.Empty)
            {
                throw new ArgumentException("businessId cannot be empty", nameof(businessId));
            }

            SqlCommand cmd = new SqlCommand(Constants.SpEventInstanceGetStatusByBusinessId, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(Constants.ParamBusinessId, businessId);

            var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                throw new InvalidOperationException($"No event instance found with businessId: {businessId}");
            }

            var result = new EventInstanceStatusBrief();

            while (reader.Read())
            {
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
                        case Constants.FieldBusinessId:
                            result.BusinessId = (Guid)reader[counter];
                            break;
                        case Constants.FieldId:
                            result.Id = (long)reader[counter];
                            break;
                        case Constants.FieldEventInstanceStatusId:
                            result.Status = (Enums.EventInstanceStatus)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }
            }

            return result;
        }
        #endregion
    }
}
