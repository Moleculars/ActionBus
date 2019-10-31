using System;
using System.Reflection;

namespace Bb.ActionBus
{
    public class BusinessLog<TContext>
        where TContext : ActionBusContext
    {


        static BusinessLog()
        {
            BusinessLog<TContext>.MethodLogResult = typeof(BusinessLog<TContext>).GetMethod("LogResult", BindingFlags.Static | BindingFlags.NonPublic);
            BusinessLog<TContext>.MethodLogResultException = typeof(BusinessLog<TContext>).GetMethod("LogResultException", BindingFlags.Static | BindingFlags.NonPublic);
        }



        /// <summary>
        /// Attention on y fait bien référence par reflexion dans la methode précédente.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static void LogResult(string ruleName, object result, TContext context, string[] names, object[] arguments)
        {

            //MessageText args = new MessageText();

            //for (int i = 0; i < names.Length; i++)
            //{
            //    var a = arguments[i];
            //    args.Add(names[i], MessageText
            //        .Text("type", a == null ? "null" : a.GetType().Name)
            //        .Add("value", a.ToString())
            //        );
            //}

            //MessageText message = MessageText
            //    .Text("incomingEvent", context.IncomingEvent.Uuid)
            //    .Add("incomingEventDate", context.IncomingEvent.EventDate)
            //    .Add("workflowUuid", context.Workflow.Uuid)
            //    .Add("workflowName", context.Workflow.WorkflowName)
            //    .Add("workflowVersion", context.Workflow.Version)
            //    .Add("workflowCreationDate", context.Workflow.CreationDate)
            //    .Add("success", "true")
            //    .Add("functionalRule", context.CurrentEvaluation.WhenRuleText)
            //    .Add("CompiledCode", context.CurrentEvaluation.WhenRuleCode)
            //    .Add("value", MessageText
            //        .Text("evaluate", ruleName)
            //        .Add("arguments", args)
            //        .Add("result", result ? "'true'" : "'false'")
            //    );

            //var position = context.CurrentEvaluation.WhenRulePosition;
            //if (position != null && position != RuleSpan.None)
            //{

            //    message.Add("positions",
            //        MessageText
            //            .Text(nameof(position.StartColumn), position.StartColumn)
            //            .Add(nameof(position.StartIndex), position.StartIndex)
            //            .Add(nameof(position.StartLine), position.StartIndex)
            //            .Add(nameof(position.StopColumn), position.StartIndex)
            //            .Add(nameof(position.StopIndex), position.StartIndex)
            //            .Add(nameof(position.StopLine), position.StartIndex)
            //        );

            //}


            //context.FunctionalLog.Add(message);

            //if (FunctionalLog == null)
            //    Trace.WriteLine(message.ToString());

            //else
            //    FunctionalLog(ruleName, result, context, names, arguments);

        }

        /// <summary>
        /// Attention on y fait bien référence par reflexion dans la methode précédente.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static void LogResultException(string ruleName, Exception result, TContext context, string[] names, object[] arguments)
        {

            //MessageText args = new MessageText();

            //for (int i = 0; i < names.Length; i++)
            //{
            //    var a = arguments[i];
            //    args.Add(names[i], MessageText
            //        .Text("type", a == null ? "null" : a.GetType().Name)
            //        .Add("value", a.ToString())
            //        );
            //}

            //MessageText message = MessageText
            //    .Text("incomingEvent", context.IncomingEvent.Uuid)
            //    .Add("incomingEventDate", context.IncomingEvent.EventDate)
            //    .Add("workflowUuid", context.Workflow.Uuid)
            //    .Add("workflowName", context.Workflow.WorkflowName)
            //    .Add("workflowVersion", context.Workflow.Version)
            //    .Add("workflowCreationDate", context.Workflow.CreationDate)
            //    .Add("success", "false")
            //    .Add("functionalRule", context.CurrentEvaluation.WhenRuleText)
            //    .Add("CompiledCode", context.CurrentEvaluation.WhenRuleCode)
            //    .Add("value", MessageText
            //        .Text("evaluate", ruleName)
            //        .Add("arguments", args)
            //        .Add("result", result.Message))
            //    .Add("exception", result.ToString())
            //    ;

            //var position = context.CurrentEvaluation.WhenRulePosition;
            //if (position != null && position != RuleSpan.None)
            //{

            //    message.Add("positions",
            //        MessageText
            //            .Text(nameof(position.StartColumn), position.StartColumn)
            //            .Add(nameof(position.StartIndex), position.StartIndex)
            //            .Add(nameof(position.StartLine), position.StartIndex)
            //            .Add(nameof(position.StopColumn), position.StartIndex)
            //            .Add(nameof(position.StopIndex), position.StartIndex)
            //            .Add(nameof(position.StopLine), position.StartIndex)
            //        );

            //}

            //context.FunctionalLog.Add(message);

            //if (FunctionalLog == null)
            //    Trace.WriteLine(message.ToString());

            //else
            //    FunctionalLogException(ruleName, result, context, names, arguments);


        }

        public static Func<string, bool, TContext, string[], object[], bool> FunctionalLog;
        public static Func<string, Exception, TContext, string[], object[], bool> FunctionalLogException;

        public static readonly MethodInfo MethodLogResult;
        public static readonly MethodInfo MethodLogResultException;

    }


}
