using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using log4net;

namespace SampleWorker
{
    partial class JobQueueWindowsService: ServiceBase
    {
        private readonly ILog Logger = LogManager.GetLogger("start");

        private JobQueueWorkerService _service;

        public JobQueueWindowsService()
        {
            ServiceName = "Abaci JobQueue Worker service ";
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Worker starting");
            _service = new JobQueueWorkerService();
            _service.Start();
            Logger.Info("Worker started");

        }

        protected override void OnStop()
        {
            Logger.Info("Worker stopping");
            _service.Stop();
            Logger.Info("Worker stoped");
        }
    }
}
