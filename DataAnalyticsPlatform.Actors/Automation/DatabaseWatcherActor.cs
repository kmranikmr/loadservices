/*
 * This file defines the DatabaseWatcherActor class, which is responsible for monitoring a database
 * for changes to automation models. The actor retrieves, caches, and updates automation models,
 * coordinating with the FolderWatcherActor to manage file system monitoring for the specified models.
 *
 * The following functionalities are implemented:
 * - Fetching automation models from the database.
 * - Adding and removing automation models based on changes in the database.
 * - Coordinating with the FolderWatcherActor to start or stop monitoring folders.
 * - Scheduling periodic checks for updates to the automation models.
 */


using Akka.Actor;
using Akka.Event;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors.Automation
{
    class DatabaseWatcherActor : ReceiveActor
    {

        public class ModelsFromDatabase
        {
            public List<ProjectAutomation> Models { get; private set; }

            public ModelsFromDatabase(List<ProjectAutomation> models)
            {
                Models = models;
            }
        }

        public class GetAutomationModels
        {
            public GetAutomationModels()
            {

            }
        }

        public class RemoveAutomation
        {
            public ProjectAutomation AutomationModel { get; private set; }

            public RemoveAutomation(ProjectAutomation model)
            {
                AutomationModel = model;
            }
        }

        public class AddAutomation
        {
            public ProjectAutomation AutomationModel { get; private set; }

            public AddAutomation(ProjectAutomation model)
            {
                AutomationModel = model;
            }
        }

        private CancellationTokenSource _cancelToken;
        private ICancelable _cancelable;

        private readonly ILoggingAdapter _logger = Context.GetLogger();

        private readonly string _connectionString;

        private Dictionary<int, ProjectAutomation> _automationModels;

        private Dictionary<int, IActorRef> _folderMonitorActors;

        private IRepository _efRepository;

        private IActorRef _automationCoordinator;

        public DatabaseWatcherActor(string connectionString, IActorRef coordinator)
        {
            _connectionString = connectionString;

            _automationCoordinator = coordinator;

            SetReceiveHandlers();

        }

        private void SetReceiveHandlers()
        {
            Receive<ModelsFromDatabase>(f =>
            {
                //stop automations  for model removed from db 

                var automationtoStop = GetModelsThatNoLongerExistInDatabaseButInCache(_automationModels); // left join in memory list and database list - stop the corresponding actors.

                if (automationtoStop != null)
                {
                    foreach (var a in automationtoStop)
                    {
                        _automationModels.Remove(a.ProjectAutomationId);

                        _logger.Info($"Deleteing {a.ProjectAutomationId} from cache.  No longer need to automate for automationId {a.ProjectAutomationId}");

                        _automationCoordinator.Tell(new RemoveAutomation(a));
                    }
                }

                foreach (var a in f.Models)
                {
                    if (_automationModels.ContainsKey(a.ProjectAutomationId) == false)
                    {
                        _logger.Info($"Added {a.ProjectAutomationId} to cache.  Send to automate for automationId {a.ProjectAutomationId}");

                        _automationModels.Add(a.ProjectAutomationId, a);

                        _automationCoordinator.Tell(new AddAutomation(a));
                    }
                }

                _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(60), Self, new GetAutomationModels(), Self);

            });

            Receive<GetAutomationModels>(f =>
            {
                GetAutomationSchema();
            });
        }

        private List<ProjectAutomation> GetModelsThatNoLongerExistInDatabaseButInCache(Dictionary<int, ProjectAutomation> automationModels)
        {
            // throw new NotImplementedException();
            return null;
        }

        protected override void PostStop()
        {
            _cancelable?.Cancel(false);
            _cancelToken?.Cancel(false);
            _cancelToken?.Dispose();
            base.PostStop();
        }

        protected override void PreStart()
        {
            _automationModels = new Dictionary<int, ProjectAutomation>();
            _cancelToken = new CancellationTokenSource();
            _folderMonitorActors = new Dictionary<int, IActorRef>();
        }

        private void GetAutomationSchema()
        {
            var self = Self;

            Task.Run(() =>
            {
                var modelsFromDb = new List<ProjectAutomation>();
                try
                {
                    //locations = _efRepository.GetAutomationModels().ToList();
                    modelsFromDb = GetModels(_connectionString);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error getting locations.");
                    throw;
                }
                return new ModelsFromDatabase(modelsFromDb);

            }, _cancelToken.Token).ContinueWith(x =>
            {
                switch (x.Status)
                {
                    case TaskStatus.RanToCompletion:
                        Console.WriteLine("Successfully checked for models.");
                        // _logger.Info("Successfully checked for models.");
                        break;
                    case TaskStatus.Canceled:
                        Console.WriteLine("Task was canceled.");
                        // _logger.Error(x.Exception, "Task was canceled.");
                        break;
                    case TaskStatus.Faulted:
                        Console.WriteLine("Task faulted.");
                        //_logger.Error(x.Exception, "Task faulted.");
                        break;
                }

                return x.Result;

            }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self);
        }

        private List<ProjectAutomation> GetModels(string connectionString)
        {
            //TODO: use repo
            var models = new List<ProjectAutomation>();
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            using (var dbContext = new DAPDbContext(options, _connectionString))
            {
                var repo = new Repository(dbContext, null);
                var list = repo.GetProjectAutomations();
                foreach (var item in list.Result)
                {
                    //var gg = item;
                    models.Add(item);
                }
            }
            //  models.Add(new ProjectAutomation { ProjectAutomationId = 1, FolderPath = @"d:\watch", ProjectId = 1114, ProjectSchemaId = 101, CreatedBy = 2, ReaderId =  2});

            return models;
        }
    }

    //temp model - will discuss to have the right one.
    public class ProjectAutomation1
    {
        public int ProjectAutomationId { get; set; }
        public int ProjectId { get; set; }
        public int ReaderId { get; set; }
        public int ProjectSchemaId { get; set; }
        public string FolderPath { get; set; }
        public int CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
