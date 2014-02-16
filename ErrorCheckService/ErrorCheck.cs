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
        private static String rootDirectory;
        private static Double startHour;
        private static Double startMinute;
        private static int uniquePrefixLength;

        public ErrorCheck()
        {
            InitializeComponent();

            EventLog.WriteEntry("Application", "InitializeComponent() Done", EventLogEntryType.Information);
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("Application", "OnStart() Started", EventLogEntryType.Information);
            try
            {
                // Notify that the application service has started
                log.Info("INFO : Error check service started.");

                // read and assign private variables
                rootDirectory = ConfigurationManager.AppSettings["rootDirectory"];
                startHour = Convert.ToDouble(ConfigurationManager.AppSettings["startHour"]);
                startMinute = Convert.ToDouble(ConfigurationManager.AppSettings["startMinute"]);
                uniquePrefixLength = Convert.ToInt32(ConfigurationManager.AppSettings["uniquePrefixLength"]);

                // get today's date at start time (ie : 10:00 AM)   
                DateTime startTime = DateTime.Today.AddHours(startHour);
                startTime = startTime.AddMinutes(startMinute);

                // if 10:00 AM has passed, get tomorrow at start time (ie : 10:00 AM)     
                if (DateTime.Now > startTime) startTime = startTime.AddDays(1);

                // calculate milliseconds until the next start time (ie : 10:00 AM)      
                int timeToFirstExecution = (int)startTime.Subtract(DateTime.Now).TotalMilliseconds;

                // calculate the number of milliseconds in 24 hours.    
                int timeBetweenCalls = (int)new TimeSpan(24, 0, 0).TotalMilliseconds;

                // set the method to execute when the timer executes.    
                TimerCallback methodToExecute = ProcessDirectory;

                // start the timer.  The timer will execute "ProcessDirectory" when the number of seconds between now and    
                // the next 10:00 AM elapse.  After that, it will execute every 24 hours.    
                Timer timer = new Timer(methodToExecute, null, timeToFirstExecution, timeBetweenCalls);

                // Block the main thread forever.  The timer will continue to execute.    
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", ex.ToString(), EventLogEntryType.Error);

                // invalid arguments
                log.Error("ERROR : Invalid arguments. Check the config file.");
            }
        }

        protected override void OnStop()
        {
        }

        // Define the event handlers. 
        private static void ProcessDirectory(object obj) 
        {
            log.Info("ProcessDirectory : " + rootDirectory);
            Console.WriteLine("ProcessDirectory : " + rootDirectory);
            try
            {
                ArrayList validDirectories = GetDirectories(rootDirectory);

                foreach (string dir in validDirectories)
                {
                    int fileCount = 0;
                    // get all pdf files in the directory.
                    string[] filesArray = Directory.GetFiles(dir, "*.pdf");
                    // remove duplicate file versions from the array
                    ArrayList cleanedFilesList = CleanFilesList(filesArray);
                    // now get the files count
                    fileCount = cleanedFilesList.Count;
                    // do the logic here
                    if (fileCount % 4 != 0)
                    {
                        // errors found (send the email)
                    }
                    else
                    {
                        // no errors found
                    }

                    log.Info("BEFORE : " + filesArray.Length + " | AFTER : " + cleanedFilesList.Count);
                    foreach (string file in filesArray)
                    {
                        log.Info(file);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("The process failed : " + ex);
            }
        }

        /// <summary>
        /// Only get subdirectories that has dd-dd-dd pattern (ie : 24-01-14)
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <returns></returns>               
        static private ArrayList GetDirectories(string rootDirectory)
        {
            ArrayList directoriesList = new ArrayList();
            Regex regex = new Regex(@"\d\d-\d\d-\d\d");
            var directories = Directory.GetDirectories(rootDirectory).Select(directory => new DirectoryInfo(directory)).Where(directory => regex.IsMatch(directory.Name));
            foreach (var directoryInfo in directories.ToArray())
            {
                directoriesList.Add(directoryInfo.FullName);
            }
            return directoriesList;
        }

        /// <summary>
        /// Remove duplicate files form the list (Same files with different revisions)
        /// </summary>
        /// <param name="rowFiles"></param>
        /// <returns></returns>
        static private ArrayList CleanFilesList(string[] rowFiles)
        {
            ArrayList cleanFilesList = new ArrayList();
            ArrayList prefixList = new ArrayList();
            string fileName;
            string prefix;
            foreach (string fileDirectory in rowFiles)
            {
                // get the file name (ie : A_10_NYH_06_Z1_Ed1_V1.pdf)
                fileName = fileDirectory.Substring(fileDirectory.LastIndexOf('\\') + 1);
                // get the compareable prefix (ie : A_10)
                prefix = fileName.Substring(0, uniquePrefixLength);
                if (!prefixList.Contains(prefix))
                {
                    prefixList.Add(prefix);
                    cleanFilesList.Add(fileDirectory);
                }
            }
            return cleanFilesList;
        }
    }
}
