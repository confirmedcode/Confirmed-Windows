using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace TrustWindowsWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.
                Application.Current.Shutdown();
                return;
            }

            // Initialize BugSplat with our database, application and version strings
            string Database = "confirmed";
            string App = Assembly.GetExecutingAssembly().GetName().Name;
            string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            try
            {
                BugSplat.CrashReporter.Init(Database, App, Version, System.Windows.Forms.Application.StartupPath + "\\BsSndRpt.exe");
                // Let BugSplat know what events to handle
                System.AppDomain.CurrentDomain.UnhandledException += BugSplat.CrashReporter.AppDomainUnhandledExceptionHandler;
                System.Windows.Forms.Application.ThreadException += BugSplat.CrashReporter.ApplicationThreadException;
                //System.Threading.Tasks.TaskScheduler.UnobservedTaskException += BugSplat.CrashReporter.TaskSchedulerUnobservedTaskExceptionHandler;
                // Add an application event handler
                BugSplat.CrashReporter.QuietMode = true;
                BugSplat.CrashReporter.exceptionHandled += new BugSplat.CrashReporter.Callback(OnBugSplatEvent);
            }
            catch(Exception ex)
            {

            }
            
            base.OnStartup(e);
        }

        // Called from the BugSplat Event
        public static void OnBugSplatEvent(Exception e)
        {
            // An application specfic action can be taken here.  For our example program nothing happens.
            //MessageBox.Show("BugSplat Event", "OnBugSplatEvent", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);

            /*string tmpName = Path.GetTempFileName();
            tmpName = Path.ChangeExtension(tmpName, ".txt");

            StreamWriter sw = new StreamWriter(tmpName);
            sw.WriteLine("Windows Version: " + System.Environment.OSVersion);
            sw.Close();

            BugSplat.CrashReporter.AdditionalFiles.Add(tmpName);*/
        }
    }
}