using Fixie.AutoRun.VisualStudio;
using Shouldly;

namespace Fixie.AutoRun.Tests.VisualStudio
{
   public class SolutionFileParserTests
   {
      public void ShouldGetConfigurationsFromSolutionFile()
      {
         var solutionFile = SolutionFileParser
            .Parse(TestDataGenerator.GetSolution(), TestDataGenerator.SolutionPath);

         solutionFile.Configurations.Count.ShouldBe(2);
         solutionFile.Configurations.ShouldContain("Debug");
         solutionFile.Configurations.ShouldContain("Release");
      }

      public void ShouldGetPathFromSolutionFile()
      {
         var solutionFile = SolutionFileParser
            .Parse(TestDataGenerator.GetSolution(), TestDataGenerator.SolutionPath);

         solutionFile.Path.ShouldBe(TestDataGenerator.SolutionPath);
      }

      public void ShouldGetPlatformsFromSolutionFile()
      {
         var solutionFile = SolutionFileParser
            .Parse(TestDataGenerator.GetSolution(), TestDataGenerator.SolutionPath);

         solutionFile.Platforms.Count.ShouldBe(1);
         solutionFile.Platforms.ShouldContain("Any CPU");
      }

      public void ShouldGetProjectsFromSolutionFile()
      {
         var solutionFile = SolutionFileParser
            .Parse(TestDataGenerator.GetSolution(), TestDataGenerator.SolutionPath);

         solutionFile.Projects.Count.ShouldBe(3);
         solutionFile.Projects.ShouldContain(TestDataGenerator.FooProjectPath);
         solutionFile.Projects.ShouldContain(TestDataGenerator.FooProjectPath);
         solutionFile.Projects.ShouldContain(TestDataGenerator.FooProjectPath);
      }
   }
}