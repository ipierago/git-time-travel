using System;
using System.Collections.Generic;

namespace GitTimeTravel
{
    class UnityEngine {
        public class Debug {
            static public void LogError(string lineToOutput) {}
            static public void LogWarning(string lineToOutput) {}
            static public void Log(string lineToOutput) {}
            static public void LogException(Exception ex) {}
        }
    }

    // Custom logging
    class Log
    {
        public Log()
        {
            // TODO: Look at stack and save namespace
        }

        // Level that indicates the importance of the log line
        public enum Level
        {
            Error = 0,
            Warning = 1,
            Info = 2,
            Debug = 3,
            Trace = 4,
            TraceVerbose = 5,
        }

        // Global level is shared by all instances of the Log object
        public static Level globalLevel = Level.Error;

        // Local level is unique to the Log object
        public Level localLevel = Level.Error;

        // Exit when an exception occurs
        public static bool exitOnException = false;

        static T Max<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) > 0 ? a : b;
        }

        // Is log level high enough to display the input level
        public bool IfLogLevel(Level level) {
            Level maxLevel = Max<Level>(globalLevel, localLevel);
            return (maxLevel >= level);
        }

        // Is the log level high enough to display trace lines?
        public bool IfTrace()
        {
            Level maxLevel = Max<Level>(globalLevel, localLevel);
            return (maxLevel >= Level.Trace);
        }

        // Is the log level high enough to display verbose lines? 
        public bool IfTraceVerbose()
        {
            Level maxLevel = Max<Level>(globalLevel, localLevel);
            return (maxLevel >= Level.TraceVerbose);
        }

        // Is the log level high enough to display debug lines?
        public bool IfDebug()
        {
            Level maxLevel = Max<Level>(globalLevel, localLevel);
            return (maxLevel >= Level.Debug);
        }

        // Is the log level high enough to display info lines?
        public bool IfInfo()
        {
            Level maxLevel = Max<Level>(globalLevel, localLevel);
            return (maxLevel >= Level.Info);
        }

        // Is the log level high enough to display warning lines?
        public bool IfWarning()
        {
            Level maxLevel = Max<Level>(globalLevel, localLevel);
            return (maxLevel >= Level.Warning);
        }

        // Is the log level high enough to display error lines?
        public bool IfError()
        {
            Level maxLevel = Max<Level>(globalLevel, localLevel);
            return (maxLevel >= Level.Error);
        }

        // Should method begin lines be printed?
        public bool IfMethodBegin()
        {
            return IfTrace();
        }

        // Should verbose method begin lines be printed?
        public bool IfMethodBeginVerbose()
        {
            return IfTraceVerbose();
        }

        // Print a line indicating that a method has started and include arguments
        public void MethodBegin(params object[] args)
        {
            if (IfMethodBegin())
            {
                string msg = _CallerToString(2, args);
                _InternalLog(Level.Trace, null, null, msg);
            }
        }

        // Print a line indicating that a method has started and include arguments (verbose version for functions called many times)
        public void MethodBeginVerbose(params object[] args)
        {
            if (IfMethodBeginVerbose())
            {
                string msg = _CallerToString(2, args);
                _InternalLog(Level.TraceVerbose, null, null, msg);
            }
        }

        // Print an error line indicating a method has failed including arguments
        public void MethodFailed(params object[] args)
        {
            string msg = _CallerToString(2, args);
            _InternalLog(Level.Error, "FAILED", null, msg);
        }

        static readonly string[] _LogLevelToShortString = new string[] {"E", "W", "I", "D", "T", "V"};

        // Internal function for logging called by most APIs
        private static void _InternalLog(Level level, string prefix, string sufix, string format = null, params object[] args)
        {
            string lineToOutput = string.Format("[{0}] ", _LogLevelToShortString[(int)level]);
            if (prefix != null) lineToOutput += prefix;
            if (format != null)
            {
                if ((args != null) && (args.Length > 0))
                {
                    try
                    {
                        string str = string.Format(format, args);
                        lineToOutput += str;

                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
                else
                {
                    lineToOutput += format;
                }
            }
            if (sufix != null)
            {
                lineToOutput += sufix;
            }
            lock (logMessages) {
                logMessages.Add(lineToOutput);
            }
            if (level == Log.Level.Error)
            {
                UnityEngine.Debug.LogError(lineToOutput);
            }
            else if (level == Log.Level.Warning)
            {
                UnityEngine.Debug.LogWarning(lineToOutput);
            }
            else
            {
                UnityEngine.Debug.Log(lineToOutput);
            }
        }

        // Log with a custom log level
        public void LogWithLevel(Level level, string format = null, params object [] args) {
            if (IfLogLevel(level)) {
                _InternalLog(level, null, null, format, args);
            }
        }

        // Log a line at indicated level
        public void Trace(string format = null, params object[] args)
        {
            if (IfTrace())
            {
                _InternalLog(Level.Trace, null, null, format, args);
            }
        }

        // Log a line at indicated level
        public void Debug(string format = null, params object[] args)
        {
            if (IfDebug())
            {
                _InternalLog(Level.Debug, null, null, format, args);
            }
        }

        // Log a line at indicated level
        public void Info(string format = null, params object[] args)
        {
            if (IfInfo())
            {
                _InternalLog(Level.Info, null, null, format, args);
            }
        }

        // Log a line at indicated level
        public void Warning(string format = null, params object[] args)
        {
            if (IfWarning())
            {
                _InternalLog(Level.Warning, null, null, format, args);
            }
        }

        // Log a line at indicated level
        public void Error(string format = null, params object[] args)
        {
            if (IfError())
            {
                _InternalLog(Level.Error, null, null, format, args);
            }
        }

        // Log the exception that has occurred
        public void Exception(Exception exception)
        {
            lock (logMessages) {
                logMessages.Add(exception.ToString());
            }
            UnityEngine.Debug.LogException(exception);
        }

        // Global list of all the log messages
        static List<string> logMessages;

        public static string[] GetLogMessages() {
            string[] rv = null;
            lock (logMessages) {
                rv = logMessages.ToArray();
            }
            return rv;
        }

        // Main initialization function, should always be called before logging anything
        public static void Init()
        {
            // Reset to defaults
            globalLevel = Level.Debug;
            exitOnException = false;

            // Instantiate globals
            logMessages = new List<string>();

            _ParamToString((object)null);
        }

        // We use our own version of this function because the primary version uses the log
        static string MyTry_String_Format(string format, params object[] args)
        {
            string out_str = "null";
            try
            {
                if (format != null)
                {
                    out_str = format;
                    if (args != null && args.Length > 0)
                    {
                        out_str = string.Format(format, args);
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            return out_str;
        }

        // Throw if the input bool is false
        public void ThrowIf(bool b = true, string format = null, params object[] args)
        {
            if (b)
            {
                Throw(format, args);
            }
        }

        // Throw an excpetion with the message given by format and args
        public void Throw(string format = null, params object[] args)
        {
            string str = MyTry_String_Format(format, args);
            throw new Exception(str);
        }

        // Throw if the assertion is false
        public void Assert(bool b = false, string format = null, params object[] args)
        {
            if (!b)
            {
                Throw(format, args);
            }
        }

        // Convert an unknown object to a string
        static string _ParamToString(object obj)
        {
            string rv = "null";
            if (obj != null)
            {
                rv = obj.ToString();
                if (rv.Length == 0)
                {
                    rv = MyTry_String_Format("{0}:{1}", obj.GetType().Name, obj.GetHashCode());
                }
            }
            return rv;
        }

        // Convert input paramter object to a string for printing during method begin, etc
        static string _ParamToString(string in_str)
        {
            string rv = in_str;
            if (in_str.Length > 256)
            {
                rv = in_str.Substring(0, 256) + "...";
            }
            return rv;
        }

        // Convert input paramter object to a string for printing during method begin, etc
        static string _ParamToString(Type type)
        {
            string rv = "null";
            if (type != null)
            {
                rv = type.Name;
            }
            return rv;
        }

        // Walk the stack and find the caller at the given frame index and convert to method name and paramter list
        static string _CallerToString(int frameIndex, params object[] args)
        {
            // Begin line with information about the current thread 
            string rv = string.Format("T{0}:", System.Threading.Thread.CurrentThread.ManagedThreadId);

            // Walk the stack to the indicated frame
            var stackTrace = new System.Diagnostics.StackTrace();
            var stackFrames = stackTrace.GetFrames();
            if (stackFrames.Length > frameIndex)
            {
                var stackFrame = stackFrames[frameIndex];
                System.Reflection.MethodBase methodBase = stackFrame.GetMethod();
                if (methodBase != null)
                {
                    // Output name and enclosing class
                    rv += MyTry_String_Format("{0}.{1}(", methodBase.ReflectedType.Name, methodBase.Name);

                    // Use reflection information to convert paramters to strings and output them also
                    System.Reflection.ParameterInfo[] parameters = methodBase.GetParameters();
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        var parameter = parameters[i];
                        object arg = null;
                        if (args != null && args.Length > i)
                        {
                            arg = args[i];
                        }
                        string paramAsString = "";
                        if (arg == null)
                        {
                            paramAsString = "null";
                        }
                        else
                        {
                            // Choose the appropriate version of _ParamToString based on type of parameter
                            var types = new Type[] { parameter.ParameterType };
                            System.Reflection.MethodInfo methodInfo = typeof(Log).GetMethod("_ParamToString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, System.Reflection.CallingConventions.Any, types, null);
                            if (methodInfo != null)
                            {
                                // Invoke the method to convert the parameter to the string
                                object objReturn = null;
                                try
                                {
                                    objReturn = methodInfo.Invoke(null, new object[] { arg });
                                    paramAsString = objReturn as string;
                                }
                                catch (Exception e)
                                {
                                    UnityEngine.Debug.LogError("Bad parameters to Log function");
                                    UnityEngine.Debug.LogException(e);
                                }
                            }
                            else
                            {
                                // Cannot process it
                                paramAsString = "(unknown)";
                            }
                        }
                        // Add parameter value to output
                        if (i > 0) rv += ", ";
                        rv += parameter.Name + ": " + paramAsString;
                    }
                    rv += ")";
                }
            }
            return rv;
        }

        public delegate void OnLogOutput(Log.Level level, string str);

    }
}
