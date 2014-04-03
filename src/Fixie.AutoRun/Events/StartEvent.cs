namespace Fixie.AutoRun.Events
{
   internal class StartEvent
   {
      public StartEvent(string solutionPath)
      {
         SolutionPath = solutionPath;
      }

      public string SolutionPath { get; private set; }
   }
}