using Log.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Business
{
    public abstract class LogBase
    {
        public string TypeName
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        public string MachineName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        public string UserName
        {
            get
            {
                return Environment.UserName;
            }
        }

        public string CurrentAssemblyLocation
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public LogModel GetLogModelProtoType(string methodName)
        {
            int counter = 1;

            var log = new Log.Interface.LogModel()
            {
                Correlation = Guid.NewGuid(),
                Counter = counter++,
                CreateDate = DateTime.UtcNow,
                CodeReference = $"{this.TypeName}|{methodName}",
                LogType = LogModel.LogTypes.Information,
                Location = this.CurrentAssemblyLocation,
                Machine = this.MachineName,
                Operation = methodName,
                Owner = this.UserName
            };

            return log;
        }

        public LogModel GetLogModelMethodStart(string methodName)
        {
            var log = this.GetLogModelProtoType(methodName);
            log.Step = LogMeta.METHOD_START;

            return log;
        }
        public LogModel GetLogModelMethodEnd(LogModel log)
        {
            var timeNow = DateTime.UtcNow;

            log.Counter++;
            log.Duration = (timeNow - log.CreateDate).TotalSeconds;
            log.CreateDate = timeNow;
            log.Step = LogMeta.METHOD_END;
            return log;
        }

        public LogModel GetLogModelException(LogModel log, Exception ex)
        {
            var timeNow = DateTime.UtcNow;

            log.Counter++;
            log.CreateDate = timeNow;
            log.Step = ex.Message;
            log.NotesB = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
            log.LogType = LogModel.LogTypes.Error;
            return log;
        }
    }
}
