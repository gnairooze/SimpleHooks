using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Log.Interface
{
    public class Utility
    {
        private static string CurrentAssemblyLocation
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static LogModel FillBasicProps(LogModel log)
        {
            log = log ?? new LogModel();
            
            log.CreateDate = DateTime.UtcNow;
            log.Machine = Environment.MachineName;
            log.Owner = Environment.UserName;
            log.Location = CurrentAssemblyLocation;

            return log;
        }

        public static LogModel SetMethodStart(LogModel log)
        {
            log = log ?? FillBasicProps(null);

            log.LogType = LogModel.LogTypes.Information;
            log.Step = Constants.MethodStart;

            return log;
        }

        public static LogModel SetMethodEnd(LogModel log)
        {
            log = log ?? FillBasicProps(null);

            var createDate = log.CreateDate;

            log.CreateDate = DateTime.UtcNow;
            log.LogType = LogModel.LogTypes.Information;
            log.Step = Constants.MethodEnd;

            log.Duration = (log.CreateDate - createDate).TotalMilliseconds;
            
            return log;
        }

        public static LogModel SetError(LogModel log, Exception ex)
        {
            ex = ex ?? throw new ArgumentNullException($"SetError requires {nameof(ex)} argument.");

            log = log ?? FillBasicProps(null);

            log.LogType = LogModel.LogTypes.Error;
            log.Step = ex.Message;
            log.NotesB = ex.ToString();

            return log;
        }
    }
}
