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
			_proxy.TestCompleted(new TestResult
			                     {
				                     Assembly = string.Empty,
				                     Class = result.Case.Class.Name,
				                     Namespace = result.Case.Class.Namespace,
				                     Reason = result.Reason,
				                     Status = TestStatus.Skip,
				                     Test = result.Case.Name
			                     });
		}

		public void CasePassed(PassResult result)
		{
			_proxy.TestCompleted(new TestResult
			                     {
				                     Assembly = string.Empty,
				                     Class = result.Case.Class.Name,
				                     Namespace = result.Case.Class.Namespace,
				                     Status = TestStatus.Pass,
				                     Test = result.Case.Name
			                     });
		}

		public void CaseFailed(FailResult result)
		{
			_proxy.TestCompleted(new TestResult
			                     {
				                     Assembly = string.Empty,
				                     Class = result.Case.Class.Name,
				                     Namespace = result.Case.Class.Namespace,
				                     Reason = new StringBuilder()
					                     .AppendLine(result.ExceptionSummary.Type)
					                     .AppendLine(result.ExceptionSummary.StackTrace)
					                     .ToString()
					                     .Trim(),
				                     Status = TestStatus.Fail,
				                     Test = result.Case.Name
			                     });
		}

		public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
		{
		}
	}
}