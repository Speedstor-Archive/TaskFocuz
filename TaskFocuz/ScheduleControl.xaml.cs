using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using System.IO;
using Path = System.IO.Path;
using System.Windows.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace TaskFocuz
{
    /// <summary>
    /// Interaction logic for ScheduleControl.xaml
    /// </summary>
    public partial class ScheduleControl : UserControl
    {
        MainWindow mainWindow = (MainWindow) Application.Current.MainWindow;
        public ObservableCollection<TodoItem> items;
        private String STORE_FILE_LOC = "TaskFocuzDB.txt";
        private int todoIDFlag = 0;
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/TaskFocuz/";

        DispatcherTimer dt = new DispatcherTimer();
        Stopwatch sw = new Stopwatch();
        string currentTime = string.Empty;

        public ScheduleControl()
        {
            InitializeComponent();
            initTodoList();
            initClockText();
        }

        #region interactives
        public void fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if(mainWindow.WindowState == WindowState.Maximized)
            {
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;

                mainWindow.disableAltTab = false;
            }
            else
            {
                mainWindow.Visibility = Visibility.Collapsed;
                mainWindow.WindowState = WindowState.Maximized;
                mainWindow.WindowStyle = WindowStyle.None;

                mainWindow.disableAltTab = true;

                mainWindow.Visibility = Visibility.Visible;
                //mainWindow.ResizeMode = ResizeMode.NoResize;
            }
        }

        public void toTray_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.minimizeToTray();
        }

        public void stopwatchButton_Click(object sender, RoutedEventArgs e)
        {
            Button swButton = ((Button) sender);
            switch (swButton.Content.ToString().ToLower())
            {
                default:
                case "start":
                    startSW();
                    swButton.Content = "Stop";
                    break;
                case "stop":
                    stopSW();
                    swButton.Content = "Reset";
                    break;
                case "reset":
                    resetSW();
                    swButton.Content = "Start";
                    break;
            }
        }

        private void startSW()
        {
            sw.Start();
            dt.Start();
        }

        private void stopSW()
        {
            if (sw.IsRunning)
            {
                sw.Stop();
            }
        }

        private void resetSW()
        {
            sw.Reset();
            stopwatch.Text = "00.00";
        }

        private void addTodoButton(object sender, RoutedEventArgs e)
        {
            addTodoItem(todoTextBox.Text);
            todoTextBox.Text = "";
        }

        private void deleteTodoItem(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if(button.DataContext is TodoItem)
            {
                deleteTodoItem((TodoItem) button.DataContext);
            }
        }

        private void todoTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                addTodoItem(todoTextBox.Text);
                todoTextBox.Text = "";
            }
        }
        #endregion

        #region TodoListManipulation
        public void addTodoItem(String todoString)
        {
            if(todoString != "" && !String.IsNullOrWhiteSpace(todoString)) {
                TodoItem todoItem = new TodoItem() { id = todoIDFlag, Title = todoString };
                items.Add(todoItem);
                writeInsertTodo(todoItem);
                todoIDFlag++;
            }
        }

        public void deleteTodoItem(TodoItem deleteItem)
        {
            items.Remove(deleteItem);
            writeDeleteTodo(deleteItem.id);
        }

        public void writeInsertTodo(TodoItem todoItem)
        {
            if (!File.Exists(docPath)) System.IO.Directory.CreateDirectory(docPath);
            using (StreamWriter outputFile = new StreamWriter(docPath + STORE_FILE_LOC, true))
            {
                if (todoItem.Title.Contains("\t")) todoItem.Title.Replace("\t", " ");
                String writeLine = $"{todoItem.id}\t{todoItem.Title}\t{todoItem.Completion}";
                
                outputFile.WriteLine(writeLine);
            }
        }

        public void writeDeleteTodo(int todoId)
        {
            string[] lines = System.IO.File.ReadAllLines(Path.Combine(docPath, STORE_FILE_LOC));

            for(int i = 0; i < lines.Length; i++)
            {
                string[] entry = lines[i].Split('\t');

                try {
                    int entryId = Int32.Parse(entry[0]);
                    if (entryId == todoId)
                    {
                        lines[i] = null;
                        break;
                    }
                }
                catch
                {
                    //entryId is not an int, DB format error
                }
            }

            if (!File.Exists(docPath)) System.IO.Directory.CreateDirectory(docPath);
            using (StreamWriter outputFile = new StreamWriter(docPath + STORE_FILE_LOC, false))
            {
                foreach (string line in lines)
                    if (line != null)
                        outputFile.WriteLine(line);
            }
        }
        
        //rewrite the whole todo file with continous id
        public void rewriteTodo()
        {
            rewriteTodo(items);
        }
        public void rewriteTodo(ObservableCollection<TodoItem> todoList)
        {
            int writeId = 0;
            if (!File.Exists(docPath)) System.IO.Directory.CreateDirectory(docPath);
            using (StreamWriter outputFile = new StreamWriter(docPath + STORE_FILE_LOC, false))
            {
                foreach(TodoItem todoItem in todoList)
                {
                    if (todoItem.Title.Contains("\t")) todoItem.Title.Replace("\t", " ");
                    String writeLine = $"{writeId}\t{todoItem.Title}\t{todoItem.Completion}";

                    outputFile.WriteLine(writeLine);
                    writeId++;
                }
            }

        }
        #endregion


        #region init definition
        public void initTodoList()
        {
            items = new ObservableCollection<TodoItem>();
            todoIDFlag = 0;

            Console.WriteLine(Path.Combine(docPath, STORE_FILE_LOC));

            if (File.Exists(Path.Combine(docPath, STORE_FILE_LOC)))
            {
                string[] lines = System.IO.File.ReadAllLines(Path.Combine(docPath, STORE_FILE_LOC));

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] entry = lines[i].Split('\t');

                    if(entry.Length >= 3)
                    {
                        try
                        {
                            int entryId = Int32.Parse(entry[0]);
                            items.Add(new TodoItem() { id = entryId, Title = entry[1], Completion = bool.Parse(entry[2]) });

                            if (entryId > todoIDFlag) todoIDFlag = entryId;
                        }
                        catch
                        {
                            //entryId is not an int, DB format error
                        }
                    }
                }
            }

            toDoList.ItemsSource = items;
        }

        public void initClockText()
        {
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                this.dateText_HHMM.Text = DateTime.Now.ToString("hh:mm", CultureInfo.InvariantCulture);
                this.dateText_SS.Text = DateTime.Now.ToString(":ss");
                this.dateText_TT.Text = DateTime.Now.ToString("tt", CultureInfo.InvariantCulture);
            }, this.Dispatcher);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Interval = new TimeSpan(0, 0, 0);
        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (sw.IsRunning)
            {
                TimeSpan ts = sw.Elapsed;
                currentTime = String.Format("{0:00}.{1:00}", ts.Minutes, ts.Seconds);
                stopwatch.Text = currentTime;
            }
        }
        #endregion
    }
    public class TodoItem
    {
        public int id { get; set; } = -1;
        public string Title { get; set; } = "";
        public bool Completion { get; set; } = false;
    }
}
