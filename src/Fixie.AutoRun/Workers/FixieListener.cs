using Fixie.Execution;
using System;

namespace Fixie.AutoRun.Workers
{
   [Serializable]
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
         _proxy.TestCompleted(GetTestResult(result.MethodGroup, TestStatus.Fail, result.Exceptions.CompoundStackTrace.Trim()));
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