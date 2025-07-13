using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SimpleTools.SimpleHooks.Repo.SQL
{
    public class ListenerDefinitionDataRepo : IDataRepository<ListenerDefinition>
    {
        public ListenerDefinition Create(ListenerDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public ListenerDefinition Edit(ListenerDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public List<ListenerDefinition> Read(Dictionary<string, string> options, object connection)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpListenerDefinitionGetAll, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var reader = cmd.ExecuteReader();

            List<ListenerDefinition> definitions = FillListenerDefinitions(reader);

            reader.Close();

            return definitions;
        }

        private List<ListenerDefinition> FillListenerDefinitions(SqlDataReader reader)
        {
            List<ListenerDefinition> definitions = new List<ListenerDefinition>();

            while (reader.Read())
            {
                ListenerDefinition definition = new ListenerDefinition();

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
                        case Constants.FieldHeaders:
                            definition.Headers.AddRange(ConvertToHeaders((string)reader[counter]));
                            break;
                        case Constants.FieldUrl:
                            definition.Url = (string)reader[counter];
                            break;
                        case Constants.FieldRetrialDelay:
                            definition.RetrialDelay = (int)reader[counter];
                            break;
                        case Constants.FieldTimeout:
                            definition.Timeout = (int)reader[counter];
                            break;
                        case Constants.FieldTrialCount:
                            definition.TrialCount = (int)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                definitions.Add(definition);
            }

            return definitions;
        }

        private List<string> ConvertToHeaders(string v)
        {
            return v.Split(Environment.NewLine.ToCharArray()).ToList();
        }

        public ListenerDefinition Remove(ListenerDefinition entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }
    }
}
