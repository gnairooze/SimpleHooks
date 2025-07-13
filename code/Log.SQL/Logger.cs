using SimpleTools.SimpleHooks.Log.Interface;
using System.Data.SqlClient;

namespace SimpleTools.SimpleHooks.Log.SQL
{
    public class Logger : ILog
    {
        public LogModel.LogTypes MinLogType { get; set; }
        public string ConnectionString { get; set; }
        public string FunctionName { get; set; }

        public LogModel Add(LogModel model)
        {
            if (this.MinLogType > model.LogType) return null;

            var conn = new SqlConnection(this.ConnectionString);

            var cmd = new SqlCommand(this.FunctionName, conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Id", System.Data.SqlDbType.BigInt).Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.AddWithValue("@CodeReference", model.CodeReference);
            cmd.Parameters.AddWithValue("@LogType", (int)model.LogType);
            cmd.Parameters.AddWithValue("@Correlation", model.Correlation);
            cmd.Parameters.AddWithValue("@Counter", model.Counter);
            cmd.Parameters.AddWithValue("@CreateDate", model.CreateDate);
            cmd.Parameters.AddWithValue("@Step", model.Step);
            cmd.Parameters.AddWithValue("@NotesB", model.NotesB);
            cmd.Parameters.AddWithValue("@Duration", model.Duration);
            cmd.Parameters.AddWithValue("@Location", model.Location);
            cmd.Parameters.AddWithValue("@Machine", model.Machine);
            cmd.Parameters.AddWithValue("@NotesA", model.NotesA);
            cmd.Parameters.AddWithValue("@Operation", model.Operation);
            cmd.Parameters.AddWithValue("@Owner", model.Owner);
            cmd.Parameters.AddWithValue("@ReferenceName", model.ReferenceName);
            cmd.Parameters.AddWithValue("@ReferenceValue", model.ReferenceValue);

            conn.Open();

            cmd.ExecuteNonQuery();

            model.Id = long.Parse(cmd.Parameters["@Id"].Value.ToString());

            conn.Close();
            conn.Dispose();

            return model;
        }
    }
}
