namespace Fixie.AutoRun
{
   internal class TestResult
   {
      public string Class { get; set; }
      public string FailReason { get; set; }
      public string Namespace { get; set; }
      public string Test { get; set; }
      public TestStatus Status { get; set; }

      public string Name
      {
         get { return string.Join(".", Namespace, Class, Test); }
      }
   }
}