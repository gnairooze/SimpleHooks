using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.IO;

namespace SimpleTools.SimpleHooks.Business
{
    public abstract class LogBase
    {
        private string TypeName => this.GetType().FullName;

        private string MachineName => Environment.MachineName;

        private string UserName => Environment.UserName;

        private string CurrentAssemblyLocation
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private LogModel GetLogModelProtoType(string methodName)
        {
            int counter = 1;

            var log = new LogModel()
            {
                Correlation = Guid.NewGuid(),
                Counter = counter,
                CreateDate = DateTime.UtcNow,
                CodeReference = $"{this.TypeName}|{methodName}",
                LogType = LogModel.LogTypes.Debug,
                Location = this.CurrentAssemblyLocation,
                Machine = this.MachineName,
                Operation = methodName,
                Owner = this.UserName
            };

            return log;
        }

        protected LogModel GetLogModelMethodStart(string methodName, string referenceName, string referenceValue)
        {
            var log = this.GetLogModelProtoType(methodName);
            log.Step = LogMeta.MethodStart;
            log.ReferenceName = referenceName;
            log.ReferenceValue = referenceValue;

            return log;
        }

        protected LogModel GetLogModelMethodEnd(LogModel log, string referenceName, string referenceValue)
        {
            var timeNow = DateTime.UtcNow;

            log.Counter++;
            log.Duration = (timeNow - log.CreateDate).TotalSeconds;
            log.CreateDate = timeNow;
            log.Step = LogMeta.MethodEnd;
            log.LogType = LogModel.LogTypes.Debug;
            log.ReferenceName = referenceName;
            log.ReferenceValue = referenceValue;
            return log;
        }

        protected LogModel GetLogModelMethodEnd(LogModel log)
        {
            var timeNow = DateTime.UtcNow;

            log.Counter++;
            log.Duration = (timeNow - log.CreateDate).TotalSeconds;
            log.CreateDate = timeNow;
            log.Step = LogMeta.MethodEnd;
            log.LogType = LogModel.LogTypes.Debug;

            return log;
        }

        protected LogModel GetLogModelException(LogModel log, Exception ex)
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
