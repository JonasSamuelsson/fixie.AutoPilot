namespace Fixie.AutoPilot
{
    internal class StartEvent
    {
        public StartEvent(string solution)
        {
            Solution = solution;
        }

        public string Solution { get; private set; }
    }
}