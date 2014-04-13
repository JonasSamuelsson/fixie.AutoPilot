using System.Reflection;
using System.Text;
using Fixie.AutoRun.FixieRunner.Contracts;
using Fixie.Results;

namespace Fixie.AutoRun.FixieRunner
{
   internal class Listener : Fixie.Listener
   {
      private readonly IService _proxy;

      public Listener(IService proxy)
      {
         _proxy = proxy;
      }

      public void AssemblyStarted(Assembly assembly)
      {
      }

      public void CaseSkipped(SkipResult result)
      {
         _proxy.TestCompleted(GetTestResult(result.Case, TestStatus.Skip, result.Reason));
      }

      public void CasePassed(PassResult result)
      {
         _proxy.TestCompleted(GetTestResult(result.Case, TestStatus.Pass));
      }

      public void CaseFailed(FailResult result)
      {
         var reason = new StringBuilder()
            .AppendLine(result.ExceptionSummary.Type)
            .AppendLine(result.ExceptionSummary.StackTrace)
            .ToString()
            .Trim();
         _proxy.TestCompleted(GetTestResult(result.Case, TestStatus.Fail, reason));
      }

      private static TestResult GetTestResult(Case @case, TestStatus status, string reason = null)
      {
         return new TestResult
                {
                   Assembly = @case.Class.Assembly.GetName().Name,
                   Class = @case.Class.Name,
                   Method = @case.Name.Replace(@case.Class.FullName + ".", string.Empty),
                   Namespace = @case.Class.Namespace,
                   Reason = reason,
                   Status = status,
                   Test = @case.Name
                };
      }

      public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
      {
      }
   }
}