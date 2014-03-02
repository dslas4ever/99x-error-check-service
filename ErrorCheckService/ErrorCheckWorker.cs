using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;

namespace ErrorCheckService
{
    public class ErrorCheckWorker
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static String rootDirectory;
        private static Double startHour;
        private static Double startMinute;
        private static int uniquePrefixLength;
        private volatile bool _shouldStop;

        /// <summary>
        /// This method will be called when the thread is started. 
        /// </summary>
        public void DoWork()
        {
            EventLog.WriteEntry("ErrorCheckService", "service started", EventLogEntryType.Information);
            try
            {
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

                // Block this thread forever.  The timer will continue to execute.    
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", ex.ToString(), EventLogEntryType.Error);

                // invalid arguments
                log.Error("ERROR : Invalid arguments. Check the config file.");
            }
        }

        /// <summary>
        /// This method will be called when the thread is stopping
        /// </summary>
        public void RequestStop()
        {
            _shouldStop = true;
        }

        // Define the event handlers. 
        private static void ProcessDirectory(object obj)
        {
            EventLog.WriteEntry("Application", "ProcessDirectory : " + rootDirectory, EventLogEntryType.Information);
            try
            {
                // get the list of folders
                List<DirectoryInfo> validDirectories = GetDirectories(rootDirectory);
                // sort them in the order of the Creation time
                validDirectories.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.CreationTime, y.CreationTime));
                validDirectories.Reverse();

                // get all pdf files in the directory.
                string[] filesArray = Directory.GetFiles(validDirectories[0].FullName, "*.pdf");

                // remove duplicate file versions from the array
                List<string> cleanedFilesList = CleanFilesList(filesArray);
                cleanedFilesList.Sort();

                string dirName = validDirectories[0].FullName;
                int countWithDuplicates = filesArray.Length;
                int countWithoutDuplicates = cleanedFilesList.Count;
                string fileNameTemplate = cleanedFilesList[0].Substring(cleanedFilesList[0].LastIndexOf('\\') + 1);


                List<Int32> missing = new List<int>();

                // Find the missing array items
                for (var i = 0; i < cleanedFilesList.Count - 1; i++)
                {
                    Int32 numThis = 0;
                    if (i != 0) numThis = Convert.ToInt32(cleanedFilesList[i].Substring(cleanedFilesList[i].LastIndexOf('\\') + 1).Substring(2, 2));

                    Int32 numNext = Convert.ToInt32(cleanedFilesList[i].Substring(cleanedFilesList[i].LastIndexOf('\\') + 1).Substring(2, 2));
                    if (i != 0) numNext = Convert.ToInt32(cleanedFilesList[i + 1].Substring(cleanedFilesList[i + 1].LastIndexOf('\\') + 1).Substring(2, 2));

                    Int32 diff = numNext - numThis;
                    Int32 count = 0;

                    while ((diff - count) > 1)
                    {
                        missing.Add(numThis + count + 1);
                        count++;
                    }
                }

                if (missing.Count != 0)
                {
                    // errors found
                    try
                    {
                        string missingFilesString = "";
                        foreach (int i in missing)
                        {
                            if (i < 10) missingFilesString = missingFilesString += (Environment.NewLine + "\t A_0" + i + "_...");
                            else missingFilesString = missingFilesString += (Environment.NewLine + "\t A_" + i + "_...");
                        }

                        string emailSubject = "Buy and Read : Issues notification (" +
                                              DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm tt") + ")";

                        string emailBody = "Status For \t\t: " + dirName + Environment.NewLine + Environment.NewLine +
                                           "Files Processed \t\t: " + countWithDuplicates + Environment.NewLine +
                                           "Duplicated Found \t\t: " + (countWithDuplicates - countWithoutDuplicates) +
                                           Environment.NewLine +
                                           "Errors Found \t\t: " + "TRUE" + Environment.NewLine +
                                           "Missing Files \t\t: " + missingFilesString;

                        Console.WriteLine(emailBody);
                        Email email = new Email();
                        email.Send(emailSubject, emailBody, null);
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("Application", "Email issue", EventLogEntryType.Error);
                    }
                }
                else
                {
                    // no errors found
                    try
                    {
                        string emailSubject = "Buy and Read : Issues notification (" +
                                              DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm tt") + ")";

                        string emailBody = "Status For \t\t: " + dirName + Environment.NewLine + Environment.NewLine +
                                           "Files Processed \t\t: " + countWithDuplicates + Environment.NewLine +
                                           "Duplicated Found \t\t: " + (countWithDuplicates - countWithoutDuplicates) +
                                           Environment.NewLine +
                                           "Errors Found \t\t: " + "FALSE" + Environment.NewLine;

                        Email email = new Email();
                        email.Send(emailSubject, emailBody, null);
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("Application", "Email issue", EventLogEntryType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("The process failed : " + ex);
                EventLog.WriteEntry("Application", "The process failed : " + ex, EventLogEntryType.Information);
            }
        }

        /// <summary>
        /// Only get subdirectories that has dd-dd-dd pattern (ie : 24-01-14)
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <returns></returns>               
        static private List<DirectoryInfo> GetDirectories(string rootDirectory)
        {
            List<DirectoryInfo> directoriesList = new List<DirectoryInfo>();
            Regex regex = new Regex(@"\d\d-\d\d-\d\d");
            var directories = Directory.GetDirectories(rootDirectory).Select(directory => new DirectoryInfo(directory)).Where(directory => regex.IsMatch(directory.Name));
            foreach (var directoryInfo in directories.ToArray())
            {
                directoriesList.Add(directoryInfo);
            }
            return directoriesList;
        }

        /// <summary>
        /// Remove duplicate files form the list (Same files with different revisions)
        /// </summary>
        /// <param name="rowFiles"></param>
        /// <returns></returns>
        static private List<string> CleanFilesList(string[] rowFiles)
        {
            List<string> cleanFilesList = new List<string>();
            List<string> prefixList = new List<string>();
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
