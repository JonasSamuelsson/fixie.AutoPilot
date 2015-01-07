using System.Runtime.Serialization;

namespace Fixie.AutoRun.Workers
{
   [DataContract]
   public class TestResult
   {
      [DataMember]
      public TestStatus Status { get; set; }

      [DataMember]
      public string Assembly { get; set; }

      [DataMember]
      public string Namespace { get; set; }

      [DataMember]
      public string Class { get; set; }

      [DataMember]
      public string Method { get; set; }

      [DataMember]
      public string Test { get; set; }

      [DataMember]
      public string Reason { get; set; }
   }
}