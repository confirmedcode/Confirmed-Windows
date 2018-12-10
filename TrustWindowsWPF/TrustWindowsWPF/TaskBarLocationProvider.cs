using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace TrustWindowsWPF
{
    class TaskBarLocationProvider
    {
        // P/Invoke goo:
        private const int ABM_GETTASKBARPOS = 5;

        [DllImport("shell32.dll")]
        private static extern IntPtr SHAppBarMessage(int msg, ref AppBarData data);

        /// <summary>
        /// Where is task bar located (at top of the screen, at bottom (default), or at the one of sides)
        /// </summary>
        private enum Dock
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        private struct Rect
        {
            public int Left, Top, Right, Bottom;
        }

        private struct AppBarData
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public Dock Dock;
            public Rect rc;
            public IntPtr lParam;
        }

        private static Rectangle GetTaskBarCoordinates(Rect rc)
        {
            return new Rectangle(rc.Left, rc.Top,
                rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        private static AppBarData GetTaskBarLocation()
        {
            var data = new AppBarData();
            data.cbSize = Marshal.SizeOf(data);

            IntPtr retval = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);

            if (retval == IntPtr.Zero)
            {
                throw new Win32Exception("WinAPi Error: does'nt work api method SHAppBarMessage");

            }

            return data;
        }

        private static Screen FindScreenWithTaskBar(Rectangle taskBarCoordinates)
        {
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.Contains(taskBarCoordinates))
                {
                    return screen;
                }
            }
            
            return Screen.PrimaryScreen;
        }

        /// <summary>
        /// Calculate wpf window position for place it near to taskbar area
        /// </summary>
        /// <param name="windowWidth">target window height</param>
        /// <param name="windowHeight">target window width</param>
        /// <param name="left">Result left coordinate <see cref="System.Windows.Window.Left"/></param>
        /// <param name="top">Result top coordinate <see cref="System.Windows.Window.Top"/></param>
        public static void CalculateWindowPositionByTaskbar(double windowWidth, double windowHeight, Visual visual, int padding, out double left, out double top)
        {
            var taskBarLocation = GetTaskBarLocation();
            //var taskBarRectangle = GetTaskBarCoordinates(taskBarLocation.rc);
            //var screen = FindScreenWithTaskBar(taskBarRectangle);

            var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            var visualObject = PresentationSource.FromVisual(visual);
            if (visualObject != null)
            {
                var transform = visualObject.CompositionTarget.TransformFromDevice;
                System.Windows.Point corner;

                switch(taskBarLocation.Dock)
                {
                    case Dock.Bottom:
                        corner = transform.Transform(new System.Windows.Point(workingArea.Right, workingArea.Bottom));
                        left = corner.X - windowWidth - padding;
                        top = corner.Y - windowHeight - padding;
                        break;
                    case Dock.Right:
                        corner = transform.Transform(new System.Windows.Point(workingArea.Right, workingArea.Bottom));
                        left = corner.X - windowWidth - padding;
                        top = corner.Y - windowHeight - padding;
                        break;
                    case Dock.Top:
                        corner = transform.Transform(new System.Windows.Point(workingArea.Right, workingArea.Top));
                        left = corner.X - windowWidth - padding;
                        top = corner.Y + padding;
                        break;
                    case Dock.Left:
                        corner = transform.Transform(new System.Windows.Point(workingArea.Left, workingArea.Bottom));
                        left = corner.X + padding;
                        top = corner.Y - windowHeight - padding;
                        break;
                    default:
                        left = padding;
                        top = padding;
                        break;
                }

                //this.Left = corner.X - this.ActualWidth - 100;
                //this.Top = corner.Y - this.ActualHeight;

                /*left = taskBarLocation.Dock == Dock.Left
                    ? screen.Bounds.X + taskBarRectangle.Width + padding
                    : screen.Bounds.X + screen.WorkingArea.Width - (windowWidth + padding);

                top = taskBarLocation.Dock == Dock.Top
                    ? screen.Bounds.Y + taskBarRectangle.Height + padding
                    : screen.Bounds.Y + screen.WorkingArea.Height - (windowHeight + padding);*/
            }
            else
            {
                left = padding;
                top = padding;
            }
        }
    }
}
