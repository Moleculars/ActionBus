using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;

namespace Bb.Logs.Serilog
{

    public class SerilogTraceListener : TraceListener
    {

        /// <summary>
        /// Initialize log with log4net
        /// </summary>
        /// <param name="name"></param>
        /// <param name="log4NetconfigPath"></param>
        public static TraceListener Initialize()
        {
            SerilogTraceListener log = new SerilogTraceListener();
            System.Diagnostics.Trace.Listeners.Add(log);
            Trace.WriteLine("Log initialized", TraceLevel.Info.ToString());
            return log;
        }


        private SerilogTraceListener()
        {
            _log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        }

        public override void Write(string message)
        {
            Log(message, TraceLevel.Info);
        }

        public override void WriteLine(string message)
        {
            Log(message, TraceLevel.Info);
        }

        public override void Write(object o)
        {
            Log(o, TraceLevel.Info);
        }

        public override void Fail(string message)
        {
            Log(message, TraceLevel.Error);
        }

        public override void Fail(string message, string detailMessage)
        {
            //Log(message, detailMessage);
        }

        public override void Write(object o, string category)
        {
            Log(o, category);
        }

        public override void WriteLine(object o)
        {
            Log(o, TraceLevel.Error);
        }

        public override void Write(string message, string category)
        {
            Log(message, category);
        }

        public override void WriteLine(object o, string category)
        {
            Log(o, category);
        }

        public override void WriteLine(string message, string category)
        {
            Log(message, category);
        }

        private void Log(object message, string category)
        {
            switch (category)
            {
                case "Error":
                    Log(message, TraceLevel.Error);
                    break;
                case "Info":
                    Log(message, TraceLevel.Info);
                    break;
                case "Off":
                    Log(message, TraceLevel.Off);
                    break;
                case "Verbose":
                    Log(message, TraceLevel.Verbose);
                    break;
                case "Warning":
                    Log(message, TraceLevel.Warning);
                    break;
                default:
                    Log(message, TraceLevel.Info);
                    break;
            };
        }

        private void Log(object message, TraceLevel category)
        {

            Console.WriteLine(message);

            //switch (category)
            //{
            //    case TraceLevel.Error:
            //        break;
            //    case TraceLevel.Info:
            //        //this._log.Information(message);
            //        break;
            //    case TraceLevel.Off:
            //        break;
            //    case TraceLevel.Verbose:
            //        break;
            //    case TraceLevel.Warning:
            //        break;
            //    default:
            //        break;
            //}

        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
            if (disposing)
            {
                _log.CloseAndFlush();
                _log.Dispose();
            }
        }


        private readonly Logger _log;

    }

}
