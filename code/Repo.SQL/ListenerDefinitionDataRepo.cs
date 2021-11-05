using Interfaces;
using Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Repo.SQL
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

        public List<ListenerDefinition> Read(Dictionary<string, string> options, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_LISTENER_DEFINITION_GET_ALL, (SqlConnection)connection);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            var reader = cmd.ExecuteReader();

            List<ListenerDefinition> definitons = FillListenerDefinitions(reader);

            reader.Close();

            return definitons;
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
                        case Constants.FIELD_HEADERS:
                            definition.Headers.AddRange(ConvertToHeaders((string)reader[counter]));
                            break;
                        case Constants.FIELD_URL:
                            definition.URL = (string)reader[counter];
                            break;
                        case Constants.FIELD_RETRIAL_DELAY:
                            definition.RetrialDelay = (int)reader[counter];
                            break;
                        case Constants.FIELD_TIMEOUT:
                            definition.Timeout = (int)reader[counter];
                            break;
                        case Constants.FIELD_TRIAL_COUNT:
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
