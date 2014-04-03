using Fixie.AutoRun.VisualStudio;
using Shouldly;
using System.IO;

namespace Fixie.AutoRun.Tests.VisualStudio
{
   public class ProjectFileParserTests
   {
      public void ShouldParseProjectFile()
      {
         var solutionDirectory = @"x:\Test";
         var projectPath = TestDataGenerator.FooTestsProjectPath;

         var projectFile = ProjectFileParser
             .Parse(TestDataGenerator.Get(projectPath), projectPath);

         projectFile.Path.ShouldBe(projectPath);

         projectFile.Files.Count.ShouldBe(1);
         projectFile.Files.ShouldContain(Path.Combine(solutionDirectory, "Foo.Tests", "FooTypeTests.cs"));

         projectFile.ProjectReferences.Count.ShouldBe(1);
         projectFile.ProjectReferences.ShouldContain(Path.Combine(solutionDirectory, "Foo", "Foo.csproj"));

         projectFile.References.Count.ShouldBe(1);
         projectFile.References.ShouldContain(Path.Combine(solutionDirectory, @"packages\Fixie.dll"));
      }
   }
}