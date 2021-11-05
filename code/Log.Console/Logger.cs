using Log.Interface;
using System;

namespace Log.Console
{
    public class Logger : Log.Interface.ILog
    {
        public LogModel.LogTypes MinLogType { get; set; }

        static int _Counter = 0;
        public LogModel Add(LogModel model)
        {
            if (this.MinLogType < model.LogType) return null;
            
            model.Id = Logger._Counter++;
            System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(model));

            return model;
        }
    }
}
