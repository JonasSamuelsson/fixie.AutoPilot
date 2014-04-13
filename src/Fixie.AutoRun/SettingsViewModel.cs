using Fixie.AutoRun.Events;
using Fixie.AutoRun.Infrastructure;
using Fixie.AutoRun.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Fixie.AutoRun
{
   public class SettingsViewModel : IViewModel
   {
      private readonly EventBus _eventBus;
      private SolutionSettings _settings;

      public SettingsViewModel(EventBus eventBus)
      {
         _eventBus = eventBus;

         Fixie = new Observable<FixieViewModel>();
         MsBuild = new Observable<MsBuildViewModel>();
         Projects = new Observable<IEnumerable<ProjectViewModel>>();
         Visible = new Observable<bool>();

         AcceptCommand = new RelayCommand(Accept);
         CancelCommand = new RelayCommand(Cancel);
      }

      public Observable<FixieViewModel> Fixie { get; private set; }
      public Observable<MsBuildViewModel> MsBuild { get; private set; }
      public Observable<IEnumerable<ProjectViewModel>> Projects { get; private set; }
      public Observable<bool> Visible { get; private set; }

      public ICommand AcceptCommand { get; private set; }
      public ICommand CancelCommand { get; private set; }

      public void Run()
      {
         _eventBus.Subscribe<ShowSettingsEvent>(Show);
      }

      private void Show(ShowSettingsEvent @event)
      {
         _settings = @event.Settings;
         var fixieSettings = _settings.Fixie;
         Fixie.Value = new FixieViewModel
                       {
                          Args = fixieSettings.Args
                       };
         var msBuildSettings = _settings.MsBuild;
         MsBuild.Value = new MsBuildViewModel
                         {
                            Args = msBuildSettings.Args,
                            Configuration = msBuildSettings.Configuration,
                            Configurations = @event.Configurations,
                            Platform = msBuildSettings.Platform,
                            Platforms = @event.Platforms,
                            Verbosity = msBuildSettings.Verbosity
                         };
         Projects.Value = _settings.Projects.Select(x => new ProjectViewModel
                                                               {
                                                                  IsTestProject = x.IsTestProject,
                                                                  Path = x.Path
                                                               });
         Visible.Value = true;
      }

      private void Accept()
      {
         _settings.Fixie.Args = Fixie.Value.Args;
         _settings.MsBuild.Args = MsBuild.Value.Args;
         _settings.MsBuild.Configuration = MsBuild.Value.Configuration;
         _settings.MsBuild.Platform = MsBuild.Value.Platform;
         _settings.MsBuild.Verbosity = MsBuild.Value.Verbosity;

         foreach (var project in _settings.Projects)
         {
            var pvm = Projects.Value.FirstOrDefault(x => x.Path.Equals(project.Path, StringComparison.CurrentCultureIgnoreCase));
            if (pvm == null) continue;
            project.IsTestProject = pvm.IsTestProject;
         }

         _settings.Save();

         Visible.Value = false;
         _eventBus.Publish<SettingsAcceptedEvent>();
      }

      private void Cancel()
      {
         Visible.Value = false;
         _eventBus.Publish<SettingsDiscardedEvent>();
      }

      public class ProjectViewModel
      {
         public string Path { get; set; }
         public bool IsTestProject { get; set; }
      }

      public class MsBuildViewModel
      {
         public string Args { get; set; }
         public string Configuration { get; set; }
         public IEnumerable<string> Configurations { get; set; }
         public string Platform { get; set; }
         public IEnumerable<string> Platforms { get; set; }
         public MsBuildVerbosity Verbosity { get; set; }
      }

      public class FixieViewModel
      {
         public string Args { get; set; }
      }
   }
}