using Fixie.AutoRun.FixieRunner.Contracts;
using Fixie.Execution;
using System;
using System.Text;

namespace Fixie.AutoRun.FixieRunner
{
   [Serializable]
   internal class NamedPipesListener : MarshalByRefObject, Listener
   {
      private readonly IService _proxy;
      private AssemblyInfo _assembly;

      public NamedPipesListener(IService proxy)
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
         var reason = new StringBuilder()
            .AppendLine(result.Exceptions.PrimaryException.Type)
            .AppendLine(result.Exceptions.CompoundStackTrace)
            .ToString()
            .Trim();
         _proxy.TestCompleted(GetTestResult(result.MethodGroup, TestStatus.Fail, reason));
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