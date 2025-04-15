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
            SqlCommand cmd = new SqlCommand(Constants.SpEventListenerDefinitionGetAll, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var reader = cmd.ExecuteReader();

            List<EventDefinitionListenerDefinition> definitons = FillEventListenerDefinitions(reader);

            reader.Close();

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
                        case Constants.FieldActive:
                            definition.Active = (bool)reader[counter];
                            break;
                        case Constants.FieldCreateBy:
                            definition.CreateBy = (string)reader[counter];
                            break;
                        case Constants.FieldCreateDate:
                            definition.CreateDate = (DateTime)reader[counter];
                            break;
                        case Constants.FieldId:
                            definition.Id = (long)reader[counter];
                            break;
                        case Constants.FieldModifyBy:
                            definition.ModifyBy = (string)reader[counter];
                            break;
                        case Constants.FieldModifyDate:
                            definition.ModifyDate = (DateTime)reader[counter];
                            break;
                        case Constants.FieldNotes:
                            definition.Notes = (string)reader[counter];
                            break;
                        case Constants.FieldEventDefinitionId:
                            definition.EventDefinitionId = (long)reader[counter];
                            break;
                        case Constants.FieldListenerDefinitionId:
                            definition.ListenerDefinitionId = (long)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                definitions.Add(definition);
            }

            return definitions;
        }

        public EventDefinitionListenerDefinition Remove(EventDefinitionListenerDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }
    }
}
