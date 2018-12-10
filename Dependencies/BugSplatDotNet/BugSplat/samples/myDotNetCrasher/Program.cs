using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

// Notes:
//        This sample project illustrates how to capture crashes (unhandled exceptions) in a managed application using BugSplat.
//
//        The shared sample database, 'Fred', is used in this example.
//        You may view crashes for the Fred account by logging in at https://www.bugsplat.com:
//        Account (Email Address): Fred 
//                       Password: Flintstone
//
//        In order to assure that crashes sent to the BugSplat website yield exception stack traces with file/line # information, 
//        just rebuild this project.  A Visual Studio post build event is configured to send the resulting .exe and .pdb files
//        to BugSplat via the SendPdbs utility.  If you wish to use your own account and database, you will need to modify the post build
//        event accordingly.  If you do not care about file/line # info or for any reason you do not want  to send these files, 
//        simply disable the post build event.
//
//        Important: if you would like to run the program from Visual Studio, disable the "Just My Code" option 
//        so the debugger will not interfere with the BugSplat exception handlers.
//        To enable or disable Just My Code debugging:
//          On the Tools menu, choose Options.
//          In the Options dialog box, open the Debugging node and then choose General.
//          Select or clear Enable Just My Code.
//
//        More information is available online at https://www.bugsplat.com

namespace myDotNetCrasher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 mainForm = new Form1();

            // Initialize BugSplat with our database, application and version strings
            string Database = "Fred";
            string App = Assembly.GetExecutingAssembly().GetName().Name;
            string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            BugSplat.CrashReporter.Init(Database, App, Version);

            // Let BugSplat know what events to handle
            System.AppDomain.CurrentDomain.UnhandledException += BugSplat.CrashReporter.AppDomainUnhandledExceptionHandler;
            System.Windows.Forms.Application.ThreadException += BugSplat.CrashReporter.ApplicationThreadException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += BugSplat.CrashReporter.TaskSchedulerUnobservedTaskExceptionHandler;

            // Add an application event handler
            BugSplat.CrashReporter.exceptionHandled += new BugSplat.CrashReporter.Callback(OnBugSplatEvent);

            if (args.Length > 0 && args[0] == "/crash")
            {
                BugSplat.CrashReporter.QuietMode = true;
                throw new Exception("intentional crash");
            }

            Application.Run(mainForm);
        }

        // Called from the BugSplat Event
        public static void OnBugSplatEvent(Exception e)
        {
            // An application specfic action can be taken here.  For our example program nothing happens.
            //MessageBox.Show("BugSplat Event", "OnBugSplatEvent", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }

    }
}
