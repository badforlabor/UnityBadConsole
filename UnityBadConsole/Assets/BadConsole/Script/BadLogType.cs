using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bad_log
{
    public enum ELoggerType
    {
        Fatal,
        Error,
        Warn,
        Info,
        Debug,
        ALL
    }

    public interface ILogType
    { }
    public class FatalLogType : ILogType
    { }
    public class ErrorLogType : ILogType
    { }
    public class WarningLogType : ILogType
    { }
    public class InfoLogType : ILogType
    { }
    public class DebugLogType : ILogType
    { }
}
