using System;

namespace Fixie.AutoRun
{
	public class DeletedEventArgs : EventArgs
	{
		public DeletedEventArgs(string path)
		{
			Path = path;
		}

		public string Path { get; private set; }
	}
}