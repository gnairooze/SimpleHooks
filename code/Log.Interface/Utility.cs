using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleTools.SimpleHooks.Log.Interface
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
            
            log.Machine = Environment.MachineName;
            log.Owner = Environment.UserName;
            log.Location = CurrentAssemblyLocation;

            return log;
        }

        public static LogModel SetCounterCreateDate(LogModel log)
        {
            log = log ?? FillBasicProps(null);

            log.Counter++;
            log.CreateDate = DateTime.UtcNow;

            return log;
        }

        public static LogModel SetMethodStart(LogModel log)
        {
            log = log ?? FillBasicProps(null);
            log = SetCounterCreateDate(log);

            log.LogType = LogModel.LogTypes.Information;
            log.Step = Constants.MethodStart;
            log.NotesB = string.Empty;

            return log;
        }

        public static LogModel SetMethodEnd(LogModel log, DateTime start)
        {
            log = log ?? FillBasicProps(null);

            log = SetCounterCreateDate(log);

            log.LogType = LogModel.LogTypes.Information;
            log.Step = Constants.MethodEnd;

            log.Duration = (log.CreateDate - start).TotalMilliseconds;
            log.NotesB = string.Empty;

            return log;
        }

        public static LogModel SetError(LogModel log, Exception ex)
        {
            ex = ex ?? throw new ArgumentNullException($"SetError requires {nameof(ex)} argument.");

            log = log ?? FillBasicProps(null);
            log = SetCounterCreateDate(log);

            log.LogType = LogModel.LogTypes.Error;
            log.Step = ex.Message;
            log.NotesB = ex.ToString();

            return log;
        }

        public static LogModel SetError(LogModel log, string message)
        {
            log = log ?? FillBasicProps(null);
            log = SetCounterCreateDate(log);

            log.LogType = LogModel.LogTypes.Error;
            log.Step = message;
            log.NotesB = message;

            return log;
        }

        public static LogModel SetInformationMessage(LogModel log, string message)
        {
            log = log ?? FillBasicProps(null);
            log = SetCounterCreateDate(log);

            log.LogType = LogModel.LogTypes.Information;
            log.Step = message;
            log.NotesB = string.Empty;

            return log;
        }

        public static LogModel SetArgumentsToNotesA(LogModel log, Dictionary<string, string> args)
        {
            log = log ?? FillBasicProps(null);

            args = args ??
                   throw new ArgumentNullException(
                       $"SetPArametersToNotesA requires {nameof(args)} argument.");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("---arguments start---");
            foreach (var arg in args)
            {
                sb.AppendLine($"{arg.Key}: {arg.Value}");
            }
            sb.AppendLine("---arguments end---");
            sb.AppendLine();

            log.NotesA += sb.ToString();

            return log;
        }
    }
}
