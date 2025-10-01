using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SimpleTools.SimpleHooks.Repo.SQL
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

        public List<EventDefinition> Read(Dictionary<string, string> options, object connection)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpEventDefinitionGetAll, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var reader = cmd.ExecuteReader();

            List<EventDefinition> definitons = FillEventDefinitions(reader);

            reader.Close();

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
                        case Constants.FieldName:
                            definition.Name = (string)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                definitions.Add(definition);
            }

            return definitions;
        }

        public EventDefinition Remove(EventDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }
    }
}
