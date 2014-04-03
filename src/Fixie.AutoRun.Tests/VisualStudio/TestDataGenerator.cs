using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Fixie.AutoRun.Tests.VisualStudio
{
   public class TestDataGenerator
   {
      private const string SolutionDir = @"x:\Test";
      public static readonly string SolutionPath = Path.Combine(SolutionDir, @"Test.sln");
      public static readonly string FooProjectPath = Path.Combine(SolutionDir, @"Foo\Foo.csproj");
      public static readonly string FooTestsProjectPath = Path.Combine(SolutionDir, @"Foo.Tests\Foo.Tests.csproj");
      public static readonly string FoobarProjectPath = Path.Combine(SolutionDir, @"Foobar\Foobar.csproj");

      public static string Get(string path)
      {
         return new Dictionary<string, Func<string>>
                {
                   {SolutionPath, GetSolution},
                   {FooProjectPath, GetFooProject},
                   {FooTestsProjectPath, GetFooTestProject},
                   {FoobarProjectPath, GetFoobarProject}
                }[path]();
      }

      public static string GetSolution()
      {
         return new StringBuilder()
            .AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00")
            .AppendLine("# Visual Studio 2013")
            .AppendLine("VisualStudioVersion = 12.0.30110.0")
            .AppendLine("MinimumVisualStudioVersion = 10.0.40219.1")
            .AppendLine("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Foo\", \"Foo\\Foo.csproj\", \"{43AE4FB0-183D-46AB-A0E6-8A9331FF751F}\"")
            .AppendLine("EndProject")
            .AppendLine("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Foo.Tests\", \"Foo.Tests\\Foo.Tests.csproj\", \"{DFA9374C-9065-4AE7-813A-18910EA112BC}\"")
            .AppendLine("EndProject")
            .AppendLine("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Foobar\", \"Foobar\\Foobar.csproj\", \"{D59CDF10-4637-4C01-8B1A-E8BDB3A6A13D}\"")
            .AppendLine("EndProject")
            .AppendLine("Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\") = \"NuGet\", \"NuGet\", \"{09910DA9-9690-455C-B899-5FEF6852BD17}\"")
            .AppendLine("	ProjectSection(SolutionItems) = preProject")
            .AppendLine("		NuGet\\Sample.nuspec = NuGet\\Sample.nuspec")
            .AppendLine("	EndProjectSection")
            .AppendLine("EndProject")
            .AppendLine("Global")
            .AppendLine("	GlobalSection(SolutionConfigurationPlatforms) = preSolution")
            .AppendLine("		Debug|Any CPU = Debug|Any CPU")
            .AppendLine("		Release|Any CPU = Release|Any CPU")
            .AppendLine("	EndGlobalSection")
            .AppendLine("	GlobalSection(ProjectConfigurationPlatforms) = postSolution")
            .AppendLine("		{43AE4FB0-183D-46AB-A0E6-8A9331FF751F}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
            .AppendLine("		{43AE4FB0-183D-46AB-A0E6-8A9331FF751F}.Debug|Any CPU.Build.0 = Debug|Any CPU")
            .AppendLine("		{43AE4FB0-183D-46AB-A0E6-8A9331FF751F}.Release|Any CPU.ActiveCfg = Release|Any CPU")
            .AppendLine("		{43AE4FB0-183D-46AB-A0E6-8A9331FF751F}.Release|Any CPU.Build.0 = Release|Any CPU")
            .AppendLine("		{DFA9374C-9065-4AE7-813A-18910EA112BC}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
            .AppendLine("		{DFA9374C-9065-4AE7-813A-18910EA112BC}.Debug|Any CPU.Build.0 = Debug|Any CPU")
            .AppendLine("		{DFA9374C-9065-4AE7-813A-18910EA112BC}.Release|Any CPU.ActiveCfg = Release|Any CPU")
            .AppendLine("		{DFA9374C-9065-4AE7-813A-18910EA112BC}.Release|Any CPU.Build.0 = Release|Any CPU")
            .AppendLine("		{D59CDF10-4637-4C01-8B1A-E8BDB3A6A13D}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
            .AppendLine("		{D59CDF10-4637-4C01-8B1A-E8BDB3A6A13D}.Debug|Any CPU.Build.0 = Debug|Any CPU")
            .AppendLine("		{D59CDF10-4637-4C01-8B1A-E8BDB3A6A13D}.Release|Any CPU.ActiveCfg = Release|Any CPU")
            .AppendLine("		{D59CDF10-4637-4C01-8B1A-E8BDB3A6A13D}.Release|Any CPU.Build.0 = Release|Any CPU")
            .AppendLine("	EndGlobalSection")
            .AppendLine("	GlobalSection(SolutionProperties) = preSolution")
            .AppendLine("		HideSolutionNode = FALSE")
            .AppendLine("	EndGlobalSection")
            .AppendLine("EndGlobal")
            .ToString();
      }

      private static string GetFooProject()
      {
         return new XElement("Project",
                             new XElement("ItemGroup",
                                          new XElement("Reference",
                                                       new XAttribute("Include", "System"))),
                             new XElement("ItemGroup",
                                          new XElement("Compile",
                                                       new XAttribute("Include", "FooType.cs"))))
            .ToString();
      }

      private static string GetFooTestProject()
      {
         return new XElement("Project",
                             new XElement("ItemGroup",
                                          new XElement("Reference",
                                                       new XAttribute("Include", "Fixie, Version=1.2.3.4, Culture=neutral, processorArchitecture=MSIL"),
                                                       new XElement("HintPath", @"..\packages\Fixie.dll")),
                                          new XElement("Reference",
                                                       new XAttribute("Include", "System"))),
                             new XElement("ItemGroup",
                                new XElement("ProjectReference",
                                   new XAttribute("Include", @"..\Foo\Foo.csproj"))),
                             new XElement("ItemGroup",
                                          new XElement("Compile",
                                                       new XAttribute("Include", "FooTypeTests.cs"))))
            .ToString();
      }

      private static string GetFoobarProject()
      {
         return new XElement("Project",
                             new XElement("ItemGroup",
                                          new XElement("Reference",
                                                       new XAttribute("Include", "System"))),
                             new XElement("ItemGroup",
                                new XElement("ProjectReference",
                                   new XAttribute("Include", @"..\Foo\Foo.csproj"))),
                             new XElement("ItemGroup",
                                          new XElement("Compile",
                                                       new XAttribute("Include", "FoobarType.cs"))))
            .ToString();
      }
   }
}