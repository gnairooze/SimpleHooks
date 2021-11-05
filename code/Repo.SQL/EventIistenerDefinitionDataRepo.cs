using Interfaces;
using Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Repo.SQL
{
    public class EventIistenerDefinitionDataRepo : IDataRepository<EventDefinitionListenerDefinition>
    {
        public EventDefinitionListenerDefinition Create(EventDefinitionListenerDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public EventDefinitionListenerDefinition Edit(EventDefinitionListenerDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public List<EventDefinitionListenerDefinition> Read(Dictionary<string, string> options, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENT_LISTENER_DEFINITION_GET_ALL, (SqlConnection)connection);

            var reader = cmd.ExecuteReader();

            List<EventDefinitionListenerDefinition> definitons = FillEventListenerDefinitions(reader);

            return definitons;
        }

        private List<EventDefinitionListenerDefinition> FillEventListenerDefinitions(SqlDataReader reader)
        {
            List<EventDefinitionListenerDefinition> definitions = new List<EventDefinitionListenerDefinition>();

            while (reader.Read())
            {
                EventDefinitionListenerDefinition definition = new EventDefinitionListenerDefinition();

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
                            definition.Active = (bool)reader[counter];
                            break;
                        case Constants.FIELD_CREATE_BY:
                            definition.CreateBy = (string)reader[counter];
                            break;
                        case Constants.FIELD_CREATE_DATE:
                            definition.CreateDate = (DateTime)reader[counter];
                            break;
                        case Constants.FIELD_ID:
                            definition.Id = (long)reader[counter];
                            break;
                        case Constants.FIELD_MODIFY_BY:
                            definition.ModifyBy = (string)reader[counter];
                            break;
                        case Constants.FIELD_MODIFY_DATE:
                            definition.ModifyDate = (DateTime)reader[counter];
                            break;
                        case Constants.FIELD_NOTES:
                            definition.Notes = (string)reader[counter];
                            break;
                        case Constants.FIELD_EVENT_DEFINITION_ID:
                            definition.EventDefinitiontId = (long)reader[counter];
                            break;
                        case Constants.FIELD_LISTENER_DEFINITION_ID:
                            definition.ListenerDefinitionId = (long)reader[counter];
                            break;
                    }
                    #endregion

                    definitions.Add(definition);
                    counter++;
                }
            }

            return definitions;
        }

        public EventDefinitionListenerDefinition Remove(EventDefinitionListenerDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }
    }
}
