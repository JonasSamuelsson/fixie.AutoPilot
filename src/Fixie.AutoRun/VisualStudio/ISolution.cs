using System;
using System.Collections.Generic;

namespace Fixie.AutoRun.VisualStudio
{
	public interface ISolution : IDisposable, IEnumerable<IProject>
	{
		IProject this[string projectPath] { get; }
		event EventHandler<SolutionChangedEventArgs> Changed;
	}
}