using Interfaces;
using Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Repo.SQL
{
    public class AppOptionDataRepo : IDataRepository<AppOption>
    {
        public AppOption Create(AppOption entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public AppOption Edit(AppOption entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public List<AppOption> Read(Dictionary<string, string> options, object connection, object transaction)
        {
            SqlCommand cmd = new SqlCommand(Constants.SP_EVENT_APP_OPTION_GET_ALL, (SqlConnection)connection);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            var reader = cmd.ExecuteReader();

            List<AppOption> definitons = FillAppOptionDefinitions(reader);

            reader.Close();

            return definitons;
        }

        private List<AppOption> FillAppOptionDefinitions(SqlDataReader reader)
        {
            List<AppOption> definitions = new List<AppOption>();

            while (reader.Read())
            {
                AppOption definition = new AppOption();

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
                        case Constants.FIELD_CATEGORY:
                            definition.Category = (string)reader[counter];
                            break;
                        case Constants.FIELD_VALUE:
                            definition.Value = (string)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                definitions.Add(definition);
            }

            return definitions;
        }

        public AppOption Remove(AppOption entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }
    }
}
