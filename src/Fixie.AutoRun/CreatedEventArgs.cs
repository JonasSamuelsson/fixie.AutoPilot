using System;

namespace Fixie.AutoRun
{
	public class CreatedEventArgs : EventArgs
	{
		public CreatedEventArgs(string path)
		{
			Path = path;
		}

		public string Path { get; private set; }
	}
}