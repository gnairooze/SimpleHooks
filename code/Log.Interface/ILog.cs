using System;

namespace Log.Interface
{
    public interface ILog
    {
        LogModel.LogTypes MinLogType { get; set; }
        LogModel Add(LogModel model);
    }
}
