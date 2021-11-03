using Log.Interface;
using System;

namespace Log.Console
{
    public class Logger : Log.Interface.ILog
    {
        static int _Counter = 0;
        public LogModel Add(LogModel model)
        {
            model.Id = Logger._Counter++;
            System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(model));

            return model;
        }
    }
}
