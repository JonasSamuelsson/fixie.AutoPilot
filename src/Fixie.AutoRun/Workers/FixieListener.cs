using Fixie.Execution;
using System;
using System.Text.RegularExpressions;

namespace Fixie.AutoRun.Workers
{
   public class FixieListener : MarshalByRefObject, Listener
   {
      private readonly IService _proxy;
      private AssemblyInfo _assembly;

      public FixieListener(IService proxy)
      {
         _proxy = proxy;
      }

      public void AssemblyStarted(AssemblyInfo assembly)
      {
         _assembly = assembly;
      }

      public void CaseSkipped(SkipResult result)
      {
         _proxy.TestCompleted(GetTestResult(result.MethodGroup, TestStatus.Skip, result.SkipReason));
      }

      public void CasePassed(PassResult result)
      {
         _proxy.TestCompleted(GetTestResult(result.MethodGroup, TestStatus.Pass));
      }

      public void CaseFailed(FailResult result)
      {
         _proxy.TestCompleted(GetTestResult(result.MethodGroup, TestStatus.Fail, FormatFailReason(result)));
      }

      private static string FormatFailReason(FailResult result)
      {
         var reason = result.Exceptions.CompoundStackTrace.Trim();
         var pattern = @"^([ ]*)at ([a-z._()]+) in ([a-z.:\\_ ]+:line \d+)$";
         var replacement = @"$1at $2" + Environment.NewLine + @"$1in $3";
         return Regex.Replace(reason, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
      }

      public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
      {
      }

      private TestResult GetTestResult(MethodGroup methodGroup, TestStatus status, string reason = null)
      {
         return new TestResult
         {
            Assembly = _assembly.Name,
            Class = methodGroup.Class,
            Method = methodGroup.Method,
            Namespace = methodGroup.FullName,
            Reason = reason,
            Status = status,
            Test = string.Format("{0}.{1}", methodGroup.Class, methodGroup.Method)
         };
      }
   }
}