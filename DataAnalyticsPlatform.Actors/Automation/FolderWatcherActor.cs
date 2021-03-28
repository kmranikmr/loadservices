using Akka.Actor;
using Akka.Event;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataAnalyticsPlatform.Actors.Automation
{
    public class FolderWatcherActor: ReceiveActor
    {
        public class NewProjectFile
        {
            public ProjectAutomation AutomationModel { get; private set; }

            public string FullFilePath { get; private set; }

            public string FileName { get; private set; }

            public NewProjectFile(ProjectAutomation model, string filePath, string fileName)
            {
                AutomationModel = model;
                FullFilePath = filePath;
                FileName = fileName;
            }
        }

        public class WatchFolder
        {
            public ProjectAutomation AutomationModel { get; private set; }

            public WatchFolder(ProjectAutomation automationModel)
            {
                AutomationModel = automationModel;
            }
        }

        public class FileCreated
        {
            public object Sender { get; }
            public FileSystemEventArgs Args { get; }

            public FileCreated(object sender, FileSystemEventArgs args)
            {
                Sender = sender;
                Args = args;
            }
        }

        public class StopWatchFolder
        {
            public ProjectAutomation AutomationModel { get; private set; }

            public StopWatchFolder(ProjectAutomation model)
            {
                AutomationModel = model;
            }
        }

        private readonly ILoggingAdapter _logger = Context.GetLogger();

        private FileSystemWatcher _watcher;

        private DirectoryInfo _folderPath;

        private ProjectAutomation _automationModel;
        
        private IActorRef _coordinator;

        public FolderWatcherActor(ProjectAutomation automationModel, IActorRef automationCoordinator)
        {
            _coordinator = automationCoordinator;            
            _automationModel = automationModel;
            SetReceiveHandlers();
        }

        private void SetReceiveHandlers()
        {
            Receive<StopWatchFolder>(x =>
            {
                if(_watcher!=null)               
                _watcher?.Dispose();
                _watcher = null;

                Context.Stop(Self);
            });

            Receive<WatchFolder>(x =>
            {
                var self = Self;

                _folderPath = new DirectoryInfo(_automationModel.FolderPath);

                _automationModel = x.AutomationModel;

                _watcher = new FileSystemWatcher(_folderPath.FullName.ToString(), "*.*")
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
           
                };
                
                //_watcher.Changed += (sender, args) => self.Tell(new FileChanged(sender, args));
                _watcher.Created += (sender, args) => self.Tell(new FileCreated(sender, args));                
                _watcher.EnableRaisingEvents = true;
            });

            Receive<FileCreated>(f =>
            {
                _logger.Info($"FileName: {f.Args.Name} created for AutomationId: {_automationModel.ProjectAutomationId}");
                Shared.Helper.WaitForFile(f.Args.FullPath);
                _coordinator.Tell(new NewProjectFile(_automationModel, f.Args.FullPath, f.Args.Name));
                
            });
        }

       
    }
}
