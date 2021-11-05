using Interfaces;
using Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Repo.SQL
{
    public class EventDefinitionDataRepo : IDataRepository<EventDefinition>
    {
        public EventDefinition Create(EventDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public EventDefinition Edit(EventDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public List<EventDefinition> Read(Dictionary<string, string> options, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENT_DEFINITION_GET_ALL, (SqlConnection)connection);

            var reader = cmd.ExecuteReader();

            List<EventDefinition> definitons = FillEventDefinitions(reader);

            return definitons;
        }

        private List<EventDefinition> FillEventDefinitions(SqlDataReader reader)
        {
            List<EventDefinition> definitions = new List<EventDefinition>();

            while (reader.Read())
            {
                EventDefinition definition = new EventDefinition();

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
                        case Constants.FIELD_NAME:
                            definition.Name = (string)reader[counter];
                            break;
                    }
                    #endregion

                    definitions.Add(definition);
                    counter++;
                }
            }

            return definitions;
        }

        public EventDefinition Remove(EventDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }
    }
}
