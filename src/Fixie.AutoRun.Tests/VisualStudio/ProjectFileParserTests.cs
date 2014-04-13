using System.Collections.Generic;
using System.Linq;
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

         projectFile.AssemblyName.ShouldBe("Foo.Tests");
         projectFile.DefaultConfiguration.ShouldBe("Debug");
         projectFile.DefaultPlatform.ShouldBe("AnyCPU");
         projectFile.OutputType.ShouldBe("Library");

         projectFile.BuildProfiles.Count.ShouldBe(2);
         projectFile.BuildProfiles.ElementAt(0).Configuration.ShouldBe("Debug");
         projectFile.BuildProfiles.ElementAt(0).Platform.ShouldBe("AnyCPU");
         projectFile.BuildProfiles.ElementAt(0).OutputPath.ShouldBe(@"bin\Debug");
         projectFile.BuildProfiles.ElementAt(1).Configuration.ShouldBe("Release");
         projectFile.BuildProfiles.ElementAt(1).Platform.ShouldBe("AnyCPU");
         projectFile.BuildProfiles.ElementAt(1).OutputPath.ShouldBe(@"bin\Release");

         projectFile.Files.Count.ShouldBe(1);
         projectFile.Files.ShouldContain(Path.Combine(solutionDirectory, "Foo.Tests", "FooTypeTests.cs"));

         projectFile.ProjectReferences.Count.ShouldBe(1);
         projectFile.ProjectReferences.ShouldContain(Path.Combine(solutionDirectory, "Foo", "Foo.csproj"));

         projectFile.References.Count.ShouldBe(1);
         projectFile.References.ShouldContain(Path.Combine(solutionDirectory, @"packages\Fixie.dll"));
      }
   }

   public class ProjectTests
   {
      public void ShouldGetOutputAssemblyPath()
      {
         var project = (IProject)new ProjectFile
                                  {
                                     AssemblyName = Path.GetFileNameWithoutExtension(TestDataGenerator.FooProjectPath),
                                     BuildProfiles = new[]
                                                     {
                                                        new ProjectFile.BuildProfile
                                                        {
                                                           Configuration = "Debug",
                                                           OutputPath = "Debug",
                                                           Platform = "AnyCPU"
                                                        },
                                                        new ProjectFile.BuildProfile
                                                        {
                                                           Configuration = "Release",
                                                           OutputPath = @"Release\AnyCPU",
                                                           Platform = "AnyCPU"
                                                        },
                                                        new ProjectFile.BuildProfile
                                                        {
                                                           Configuration = "Release",
                                                           OutputPath = @"Release\x86",
                                                           Platform = "x86"
                                                        }
                                                     }.ToSet(),
                                     DefaultConfiguration = "Debug",
                                     DefaultPlatform = "AnyCPU",
                                     OutputType = "Library",
                                     Path = TestDataGenerator.FooProjectPath
                                  };

         var filename = "Foo.dll";
         var directory = Path.GetDirectoryName(TestDataGenerator.FooProjectPath);

         project.GetOutputAssemblyPath("Debug", "AnyCPU").ShouldBe(Path.Combine(directory, @"Debug", filename));
         project.GetOutputAssemblyPath("Debug", "xyz").ShouldBe(Path.Combine(directory, @"Debug", filename));
         project.GetOutputAssemblyPath("xyz", "AnyCPU").ShouldBe(Path.Combine(directory, @"Debug", filename));
         project.GetOutputAssemblyPath("xyz", "x86").ShouldBe(Path.Combine(directory, @"Debug", filename));
         project.GetOutputAssemblyPath("Release", "AnyCPU").ShouldBe(Path.Combine(directory, @"Release\AnyCPU", filename));
         project.GetOutputAssemblyPath("Release", "x86").ShouldBe(Path.Combine(directory, @"Release\x86", filename));
         project.GetOutputAssemblyPath("Release", "xyz").ShouldBe(Path.Combine(directory, @"Release\AnyCPU", filename));
         project.GetOutputAssemblyPath("xyz", "xyz").ShouldBe(Path.Combine(directory, @"Debug", filename));
      }
   }
}