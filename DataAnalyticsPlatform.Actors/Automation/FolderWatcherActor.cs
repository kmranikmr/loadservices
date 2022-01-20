using Akka.Actor;
using Akka.Event;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

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

        private PhysicalFileProvider physicalFileProvider;

        private IChangeToken _fileChangeToken;
      
        private IActorRef Myself;
      
        private bool Init; 

         private readonly ConcurrentDictionary<string, DateTime> _files = new ConcurrentDictionary<string, DateTime>();

        public FolderWatcherActor(ProjectAutomation automationModel, IActorRef automationCoordinator)
        {
            _coordinator = automationCoordinator;            
            _automationModel = automationModel;
            Myself = Self;
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
                 /*
                _watcher = new FileSystemWatcher(_folderPath.FullName.ToString(), "*.*")
                {
                    IncludeSubdirectories = false,
                    //NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                    
                  NotifyFilter =  NotifyFilters.Attributes |
                                NotifyFilters.CreationTime |
                                NotifyFilters.FileName |
                                NotifyFilters.LastAccess |
                                NotifyFilters.LastWrite |
                                NotifyFilters.Size |
                                NotifyFilters.Security |
                                NotifyFilters.DirectoryName        
                };
                
                _watcher.Changed += (sender, args) => self.Tell(new FileCreated(sender, args));
                //_watcher.Created += (sender, args) => self.Tell(new FileCreated(sender, args));                
                _watcher.EnableRaisingEvents = true;
                */
                Console.WriteLine("olderpath at watch folder " + _folderPath.FullName.ToString());
                physicalFileProvider = new PhysicalFileProvider(_folderPath.FullName.ToString());
                self.Tell(new WatchForFiles(_folderPath.FullName.ToString()));
            });


            Receive<WatchForFiles>(f =>
            {
                Console.WriteLine(" warch for file " + f.Path);
                IEnumerable<string> files = Directory.EnumerateFiles(f.Path, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (_files.TryGetValue(file, out DateTime existingTime))
                    {
                        _files.TryUpdate(file, File.GetLastWriteTime(file), existingTime);
                    }
                    else
                    {
                        if (File.Exists(file))
                        {
                           
                            _files.TryAdd(file, File.GetLastWriteTime(file));
                           Console.WriteLine("fie exists " + file );
                           if ( Init) 
			   {
                              string fileItem = Path.GetFileName(file);
                              if ( !fileItem.Contains(".hea") && !fileItem.Contains(".dat") && !fileItem.Contains(".atr") )
                             {
                             Console.WriteLine($"file name new project { Path.GetDirectoryName(file)} {Path.GetFileName(file)}");
                             _coordinator.Tell(new NewProjectFile(_automationModel, Path.GetDirectoryName(file), Path.GetFileName(file)));
                             }
                           }
		           else
                           {
                             Console.WriteLine("not init");
                           }
                         }
                    }
                }
                 Init = true;
                _fileChangeToken = physicalFileProvider.Watch("*.*");
                _fileChangeToken.RegisterChangeCallback(_ => Myself.Tell(new Notify()), default);
            });

            Receive<FileCreated>(f =>
            {
                _logger.Info($"FileName: {f.Args.Name} created for AutomationId: {_automationModel.ProjectAutomationId}");
                Shared.Helper.WaitForFile(f.Args.FullPath);
                 var filePath = Path.GetDirectoryName(f.Args.FullPath);
                _coordinator.Tell(new NewProjectFile(_automationModel, filePath, f.Args.Name));
                
            });

            Receive<Notify>(f =>
            {
                Self.Tell(new WatchForFiles(_folderPath.FullName.ToString()));
                //_logger.Info($"FileName: {f..ame} created for AutomationId: {_automationModel.ProjectAutomationId}");
                //Shared.Helper.WaitForFile(f.Args.FullPath);
                //var filePath = Path.GetDirectoryName(f.Args.FullPath);
                //_coordinator.Tell(new NewProjectFile(_automationModel, filePath, f.Args.Name));
            });
        }

       
    }


    internal class Notify
    {
        public Notify()
        {
        }
    }

    internal class WatchForFiles
    {
        public string Path { get; set; }
        public WatchForFiles()
        {

        }
        public WatchForFiles(string filePath)
        {
            Path = filePath;
        }
    }
}
