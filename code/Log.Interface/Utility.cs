using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
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

        public static MethodBase GetRealMethodFromAsync(MethodBase asyncMethod)
        {
            var generatedType = asyncMethod.DeclaringType;
            var originalType = generatedType.DeclaringType;
            var matching = from method in originalType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                           let attr = method.GetCustomAttribute<AsyncStateMachineAttribute>()
                where attr != null && attr.StateMachineType == generatedType
                select method;
            return matching.SingleOrDefault();
        }

        public static LogModel Clone(LogModel log)
        {
            var clonedLog = new LogModel()
            {
                CodeReference = log.CodeReference,
                Correlation = log.Correlation,
                Counter = log.Counter,
                CreateDate = log.CreateDate,
                Duration = log.Duration,
                Id = log.Id,
                Location = log.Location,
                LogType = log.LogType,
                Machine = log.Machine,
                NotesA = log.NotesA,
                NotesB = log.NotesB,
                Owner = log.Owner,
                Operation = log.Operation,
                ReferenceName = log.ReferenceName,
                ReferenceValue = log.ReferenceValue,
                Step = log.Step
            };

            return clonedLog;
        }
    }
}
