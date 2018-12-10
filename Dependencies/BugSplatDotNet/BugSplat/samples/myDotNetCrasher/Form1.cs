using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using ManagedTestDll;

namespace myDotNetCrasher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void MemoryAccessViolationButton_Click(object sender, System.EventArgs e)
        {
            Object o = null;
            MessageBox.Show(o.ToString());
        }

        private void DivideByZeroButton_Click(object sender, System.EventArgs e)
        {
            ErrorExamples ex = new ErrorExamples();
            ex.IntDivideByZero();
        }

        private void ManagedThreadExceptionButton_Click(object sender, System.EventArgs e)
        {
            ErrorExamples y = new ErrorExamples();

            Thread t = new Thread(
                new ThreadStart(y.IntDivideByZero));
            t.Start();
            t.Join();
        }

        private void ManagedDllExceptionButton_Click(object sender, EventArgs e)
        {
          ManagedTestDll.Test.MemoryException();
        }

        private void NativeDllExceptionButton_Click(object sender, EventArgs e)
        {
          NativeTestDll.MemoryException();
        }

        // An example of catching an exception, and then reporting it with BugSplat.createReport();
        private void CatchExceptionButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                Object o = null;
                MessageBox.Show(o.ToString());
            }
            catch (Exception excpt)
            {
                BugSplat.CrashReporter.createReport(excpt);
            }
        }

        private void SendAdditionalFilesButton_Click(object sender, System.EventArgs e)
        {
            string tmpName = Path.GetTempFileName();
            tmpName = Path.ChangeExtension(tmpName, ".xml");

            StreamWriter sw = new StreamWriter(tmpName);
            sw.WriteLine("<tag>This is a test file.  Apps can put anything here.</tag>");
            sw.Close();

            BugSplat.CrashReporter.AdditionalFiles.Add(tmpName);

            Object o = null;
            MessageBox.Show(o.ToString());
        }

        private void InnerExceptionButton_Click(object sender, System.EventArgs e)
        {
            FuncLevel1();
        }

        private void FuncLevel1()
        {
            FuncLevel2();
        }
        private void FuncLevel2()
        {
            FuncLevel3();
        }
        private void FuncLevel3()
        {
            try
            {
                FuncLevel4();
            }
            catch (Exception e)
            {
                throw new Exception("Test Outer Exception", e);
            }
        }
        private void FuncLevel4()
        {
            throw new Exception("Test Exception Thrown by sample code");
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void unwatchedExceptionButton_Click(object sender, EventArgs e)
        {
            Task[] TaskArray = new Task[] { run() };
            var tasks = new HashSet<Task>(TaskArray);
            tasks.Remove(await Task.WhenAny(tasks));
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public async Task<int> run()
        {
            return await Task.Run(() => {
                throw new Exception("broke");
                return 0;
            });
        }
    }

    internal class ErrorExamples
    {
        public void IntDivideByZero()
        {
            int x, y;

            x = 5;
            y = 0;
            int nRes = x / y;
        }
    }

    internal class NativeTestDll
    {
      [DllImport("NativeTestDll.dll")]
      public static extern void MemoryException();
    }
  }
