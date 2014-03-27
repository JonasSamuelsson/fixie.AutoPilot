using Shouldly;
using System.IO;

namespace Fixie.AutoRun.Tests
{
   public class SolutionFileParserTests
   {
      public void ShouldGetProjectsFromSolutionFile()
      {
         var solutionDirectory = @"c:\";
         var solutionPath = Path.Combine(solutionDirectory, "Solution.sln");

         var solutionFile = new SolutionFileParser()
            .Parse(TestDataGenerator.GetSolution(), solutionPath);

         solutionFile.Path.ShouldBe(solutionPath);
         solutionFile.Projects.Count.ShouldBe(3);
         solutionFile.Projects.ShouldContain(Path.Combine(solutionDirectory, @"Foo\Foo.csproj"));
         solutionFile.Projects.ShouldContain(Path.Combine(solutionDirectory, @"Foo.Tests\Foo.Tests.csproj"));
         solutionFile.Projects.ShouldContain(Path.Combine(solutionDirectory, @"Foobar\Foobar.csproj"));
      }
   }
}