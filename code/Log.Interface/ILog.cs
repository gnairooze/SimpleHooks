namespace SimpleTools.SimpleHooks.Log.Interface
{
    public interface ILog
    {
        LogModel.LogTypes MinLogType { get; set; }
        string ConnectionString { get; set; }
        string FunctionName { get; set; }
        LogModel Add(LogModel model);
    }
}
