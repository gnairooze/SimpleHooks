using Interfaces;
using Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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
            SqlCommand cmd = new SqlCommand(Constants.SpEventAppOptionGetAll, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var reader = cmd.ExecuteReader();

            List<AppOption> definitions = FillAppOptionDefinitions(reader);

            reader.Close();

            return definitions;
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
                        case Constants.FieldCategory:
                            definition.Category = (string)reader[counter];
                            break;
                        case Constants.FieldValue:
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
