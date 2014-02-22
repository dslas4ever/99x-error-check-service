using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;

namespace ErrorCheckService
{
    public partial class ErrorCheck : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ErrorCheckWorker workerObject = null;
        Thread workerThread = null;

        public ErrorCheck()
        {
            InitializeComponent();

            // Create the thread object. but does not start the thread.
            workerObject = new ErrorCheckWorker();
            workerThread = new Thread(workerObject.DoWork);
        }

        protected override void OnStart(string[] args)
        {
            // Now start the worker thread.
            workerThread.Start();
            log.Info("INFO : MAIN_THREAD : Starting worker thread...");

            // Loop until worker thread activates. 
            while (!workerThread.IsAlive);
        }

        protected override void OnStop()
        {
            // Request that the worker thread stop itself:
            workerObject.RequestStop();

            // Request that oThread be stopped
            workerThread.Abort();

            // Use the Join method to block the current thread  
            // until the object's thread terminates.
            workerThread.Join();
            log.Info("INFO : MAIN_THREAD : Worker thread has terminated....");
        }
    }
}
