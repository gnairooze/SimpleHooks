using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Models.Definition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SimpleTools.SimpleHooks.Repo.SQL
{
    public class ListenerTypeDataRepo : IDataRepository<ListenerType>
    {
        public ListenerType Create(ListenerType entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public ListenerType Edit(ListenerType entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }

        public List<ListenerType> Read(Dictionary<string, string> options, object connection)
        {
            SqlCommand cmd = new SqlCommand(Constants.SpListenerTypeGetAll, (SqlConnection)connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var reader = cmd.ExecuteReader();

            List<ListenerType> listenerTypes = FillListenerTypes(reader);

            reader.Close();

            return listenerTypes;
        }

        private List<ListenerType> FillListenerTypes(SqlDataReader reader)
        {
            List<ListenerType> listenerTypes = new List<ListenerType>();

            while (reader.Read())
            {
                ListenerType listenerType = new ListenerType();

                int counter = 0;

                while (counter < reader.FieldCount)
                {
                    if (reader[counter] == DBNull.Value)
                    {
                        counter++;
                        continue;
                    }

                    #region read listener type fields
                    switch (reader.GetName(counter))
                    {
                        case Constants.FieldActive:
                            listenerType.Active = (bool)reader[counter];
                            break;
                        case Constants.FieldCreateBy:
                            listenerType.CreateBy = (string)reader[counter];
                            break;
                        case Constants.FieldCreateDate:
                            listenerType.CreateDate = (DateTime)reader[counter];
                            break;
                        case Constants.FieldId:
                            listenerType.Id = (int)reader[counter];
                            break;
                        case Constants.FieldModifyBy:
                            listenerType.ModifyBy = (string)reader[counter];
                            break;
                        case Constants.FieldModifyDate:
                            listenerType.ModifyDate = (DateTime)reader[counter];
                            break;
                        case Constants.FieldNotes:
                            listenerType.Notes = (string)reader[counter];
                            break;
                        case Constants.FieldName:
                            listenerType.Name = (string)reader[counter];
                            break;
                        case Constants.FieldPath:
                            listenerType.Path = (string)reader[counter];
                            break;
                    }
                    #endregion

                    counter++;
                }

                listenerTypes.Add(listenerType);
            }

            return listenerTypes;
        }

        public ListenerType Remove(ListenerType entity, object connection, object transaction)
        {
            throw new NotImplementedException();
        }
    }
}

