using Shouldly;
using System.IO;

namespace Fixie.AutoRun.Tests
{
   public class ProjectFileParserTests
   {
      public void ShouldParseProjectFile()
      {
         var solutionDirectory = @"c:\";
         var projectPath = Path.Combine(solutionDirectory, "Foo.Tests", "Foo.Tests.csproj");

         var projectFile = new ProjectFileParser()
             .Parse(TestDataGenerator.GetFooTestProject(), projectPath);

         projectFile.Path.ShouldBe(projectPath);

         projectFile.Files.Count.ShouldBe(1);
         projectFile.Files.ShouldContain(Path.Combine(solutionDirectory, "Foo.Tests", "MyTypeTests.cs"));

         projectFile.ProjectReferences.Count.ShouldBe(1);
         projectFile.ProjectReferences.ShouldContain(Path.Combine(solutionDirectory, "Foo", "Foo.csproj"));

         projectFile.References.Count.ShouldBe(1);
         projectFile.References.ShouldContain(Path.Combine(solutionDirectory, @"packages\Fixie.dll"));
      }
   }
}