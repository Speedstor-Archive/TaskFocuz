using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskFocuz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool disableAltTab = false;

        private System.Windows.Forms.NotifyIcon trayNotifyIcon;
        private bool _isExit;
        private Timer appTimer;



        List<Process> allStartedProcess = new List<Process>();

        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/TaskFocuz";

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;

            trayNotifyIcon = new System.Windows.Forms.NotifyIcon();
            trayNotifyIcon.Icon = TaskFocuz.Properties.Resources.favicon;
            trayNotifyIcon.MouseDoubleClick += (s, args) => showMainWindow();
            trayNotifyIcon.Visible = true;


            appTimer = new Timer(blockApps, allStartedProcess, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));


            if (!File.Exists(docPath + "/blockApps.txt")) File.Create(docPath + "/blockApps.txt");
            if (!File.Exists(docPath + "/blockSites.txt")) File.Create(docPath + "/blockSites.txt");

            CreateContextMenu();
        }


        private static void blockApps(object state)
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/TaskFocuz/blockApps.txt";
            bool taskFocusRunning = true;
            if (File.Exists(docPath))
            {
                try
                {
                    taskFocusRunning = false;

                    Process[] processList = Process.GetProcesses();
                    for (int i = 0; i < processList.Length; i++)
                    {
                        Process process = processList[i];
                        if (process.ProcessName == "AppBlocker") taskFocusRunning = true;

                    }
                }
                catch
                {

                }
            }
            if (!taskFocusRunning)
            {
                Process appBlockerProcess = new Process();
                appBlockerProcess.StartInfo = new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/TaskFocuz/AppBlocker.exe");
                //p.StartInfo.WorkingDirectory = @"C:\Program Files\Chrome";\
                appBlockerProcess.StartInfo.UseShellExecute = false; 
                appBlockerProcess.StartInfo.CreateNoWindow = true;
                appBlockerProcess.Start();

                ((List<Process>) state).Add(appBlockerProcess);

                Console.WriteLine("restarted taskfocuz");
            }
            //Console.WriteLine($"check processes {taskFocusRunning}");
        }

        private void CreateContextMenu()
        {
            trayNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            trayNotifyIcon.ContextMenuStrip.Items.Add("Show...").Click += (s, e) => showMainWindow();
            trayNotifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            foreach(Process process in allStartedProcess)
            {
                process.Kill();
            }
            _isExit = true;
            this.Close();
            trayNotifyIcon.Dispose();
            trayNotifyIcon = null;
        }

        private void showMainWindow()
        {
            if (this.IsVisible)
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                }
                this.Activate();
            }
            else
            {
                this.Show();
            }
        }

        public void minimizeToTray()
        {
            trayNotifyIcon.Visible = true;
            this.Hide();
        }

        private void focusTodoInput(object sender, ExecutedRoutedEventArgs e)
        {
            //TODO: check if it is in schedule window
            scheduleControl.todoTextBox.Focus();

        }

        public void shortcutHandler(object sender, KeyEventArgs e)
        {
            /*
            if (disableAltTab)
            {
                if(e.KeyboardDevice.Modifiers == ModifierKeys.Alt && e.Key == Key.I)
                {
                    Debug.WriteLine("Caught");
                    MessageBox.Show("Are you serious?", "Awwwww, noooooo!");
                    e.Handled = true;
                }
            }*/
            //Control + L
            if(e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.L)
            {
                //Debug.WriteLine("with control key:");
            }
        }

        public void MainWindow_OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                this.Hide(); // A hidden window can be shown again, a closed one not
            }

            scheduleControl.rewriteTodo();
        }
    }

    public static class WindowCommands
    {
        public static RoutedCommand focusTodoInput = new RoutedCommand("focusTodoInput", typeof(WindowCommands));
    }
}
