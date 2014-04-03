using Fixie.AutoRun.VisualStudio;
using Shouldly;

namespace Fixie.AutoRun.Tests.VisualStudio
{
   public class SolutionFileParserTests
   {
      public void ShouldGetProjectsFromSolutionFile()
      {
         var solutionFile = SolutionFileParser
            .Parse(TestDataGenerator.GetSolution(), TestDataGenerator.SolutionPath);

         solutionFile.Path.ShouldBe(TestDataGenerator.SolutionPath);
         solutionFile.Projects.Count.ShouldBe(3);
         solutionFile.Projects.ShouldContain(TestDataGenerator.FooProjectPath);
         solutionFile.Projects.ShouldContain(TestDataGenerator.FooProjectPath);
         solutionFile.Projects.ShouldContain(TestDataGenerator.FooProjectPath);
      }
   }
}