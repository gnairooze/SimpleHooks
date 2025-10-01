using SimpleTools.SimpleHooks.Log.Interface;

namespace SimpleTools.SimpleHooks.Log.Console
{
    public class Logger : ILog
    {
        public LogModel.LogTypes MinLogType { get; set; }
        public string ConnectionString { get; set; }
        public string FunctionName { get; set; }

        static int _counter;
        public LogModel Add(LogModel model)
        {
            if (MinLogType > model.LogType) return null;
            
            _counter++;
            System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(model));

            return model;
        }
    }
}
