using System;

namespace Fixie.AutoRun
{
	public class ChangedEventArgs : EventArgs
	{
		public ChangedEventArgs(string path)
		{
			Path = path;
		}

		public string Path { get; private set; }
	}
}