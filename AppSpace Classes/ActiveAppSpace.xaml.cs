using NewMaster.Packets;
using Prototype1.Activity_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Drawing;
using WpfAnimatedGif;
//using AxAXVLC;
using System.Windows.Forms;
using System.Windows.Controls.Primitives;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for ActiveAppSpace.xaml
    /// </summary>
    public partial class ActiveAppSpace : System.Windows.Controls.UserControl
    {

        /// <summary>
        /// Activity currently sellected by user. Otherwise null
        /// </summary>
        private Activity _selectedActivity;

        /// <summary>
        /// Vertical offset to set scroll to when auto-scrolling description text box
        /// </summary>
        private int autoScrollVerticalOffset = 1;

        /// <summary>
        /// Timer to allow auto-scroll ability within description text box
        /// </summary>
        private System.Timers.Timer descriptionAutoScrollTimer;

        System.Windows.Point m_start;

        /// <summary>
        /// Process corresponding to the current Game activity being run if one is running. Otherwise null
        /// </summary>
        private Process currentActivityProcess;

        private System.Windows.Controls.Image image;
        private SwipeToExit _swipetoexit;
        private MessageBox mb = null;
        private Activity_Classes.VidPlayer singlePlayer = null;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        const UInt32 WM_CLOSE = 0x0010;
        private int SW_Hide = 0;
        public static bool _swipeClose = false;

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0001;
        const uint SWP_SHOWWINDOW = 0x0040;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        /// <summary>
        /// MArgin to set around activity, this is to allow the "force exit" bar of the right size of the screen to show up
        /// </summary>
        public double ActivityMargin
        {
            get
            {
                return (double)GetValue(ActivityMarginProperty);
            }
            private set
            {
                SetValue(ActivityMarginProperty, value);
            }
        }

        public static readonly DependencyProperty ActivityMarginProperty =
                DependencyProperty.Register("ActivityMargin",
                                            typeof(double),
                                            typeof(ActiveAppSpace),
                                            new PropertyMetadata(50.0));

        /// <summary>
        /// Indicates whether or not an activity is currently being run
        /// </summary>
        public bool ActivityActive
        {
            get
            {
                return (bool)GetValue(ActivityActiveProperty);
            }
            set
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => SetValue(ActivityActiveProperty, value)));
                    return;
                }
                SetValue(ActivityActiveProperty, value);
            }
        }

        public static bool resetTimer
        {
            set
            {
                Console.Out.WriteLine("MouseDown Control");
                if (inputTimer != null)
                {
                    inputTimer.Stop();
                    inputTimer.Interval = inputTimerInterval;
                    inputTimer.Start();
                }
            }
        }

        public Socket UCMSocket { get; private set; }

        public static readonly DependencyProperty ActivityActiveProperty =
                DependencyProperty.Register("ActivityActive",
                                            typeof(bool),
                                            typeof(ActiveAppSpace),
                                            new PropertyMetadata(false));

        public SwipeToExit swipetoexit
        {
            get { return _swipetoexit; }
            set { _swipetoexit = value ?? new SwipeToExit(); }
        }
        public static bool swipeClose
        {
            get { return _swipeClose; }
            set { _swipeClose = value; }
        }


        /// <summary>
        /// Used to bind the visibility of interactive objects
        /// </summary>
        public bool InteractiveElementsVisibility
        {
            get
            {
                return (bool)GetValue(InteractiveElementsVisibilityProperty);
            }
            set
            {
                SetValue(InteractiveElementsVisibilityProperty, value);
            }
        }

        /// <summary>
        /// Dependency Property to make InteractiveElementsVisibility bindable in Xaml
        /// </summary>
        public static readonly DependencyProperty InteractiveElementsVisibilityProperty =
                DependencyProperty.Register("InteractiveElementsVisibility",
                                            typeof(bool),
                                            typeof(ActiveAppSpace));

        /// <summary>
        /// Flag indicating whether or not the video is paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return (bool)GetValue(IsPausedProperty);
            }
            set
            {
                SetValue(IsPausedProperty, value);
            }
        }

        public static readonly DependencyProperty IsPausedProperty =
                DependencyProperty.Register("IsPaused",
                                            typeof(bool),
                                            typeof(ActiveAppSpace));

        public ActiveAppSpace(double width, double height)
        {
            Console.Out.WriteLine("Consructor Called");
            InitializeComponent();
            Width = width;
            Height = height;
            this.singlePlayer = new VidPlayer(this);
            singlePlayer.Visibility = Visibility.Hidden;
            singlePlayer.Closed += SinglePlayer_Closed;
            mb = new MessageBox();
            mb.Visibility = Visibility.Hidden;
            mb.Topmost = false;
            mb.textBox.Text = "Please wait while your request is processed";
            mb.OkButton.Visibility = Visibility.Collapsed;
            mb.CancelButton.Visibility = Visibility.Collapsed;
            mb.textBox.Margin = new Thickness(10, 0, 10, 0);
            mb.textBox.VerticalAlignment = VerticalAlignment.Center;
            image = new System.Windows.Controls.Image();
            image.BeginInit();
            image.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/gray.jpg"));
            image.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            image.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            image.Stretch = Stretch.Fill;
            image.EndInit();
            if (App.monitorPosition == 1)
            {
                endPointA = new IPEndPoint(IPAddress.Parse(App.slave1IP), 8002);
            }
            else if (App.monitorPosition == 2)
            {
                endPointA = new IPEndPoint(IPAddress.Parse(App.slave2IP), 8002);
            }
            else if (App.monitorPosition == 3)
            {
                endPointA = new IPEndPoint(IPAddress.Parse(App.slave3IP), 8002);
            }
            else if (App.monitorPosition == 4)
            {
                endPointA = new IPEndPoint(IPAddress.Parse(App.slave4IP), 8002);
            }
            updateListener = new TcpListener(IPAddress.Any, 4079);
            Console.WriteLine("Listening...");
            updateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            updateListener.Start();
            updateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenClose), updateListener);
            masterUpdateListener = new TcpListener(IPAddress.Any, 4080);
            Console.WriteLine("Listening for Master Updates...");
            masterUpdateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            masterUpdateListener.Start();
            masterUpdateListener.BeginAcceptTcpClient(new AsyncCallback(MasterUpdate), masterUpdateListener);
            multiScreenRequestListener = new TcpListener(IPAddress.Any, 4086);
            Console.WriteLine("Listening for Master Updates...");
            multiScreenRequestListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            multiScreenRequestListener.Start();
            multiScreenRequestListener.BeginAcceptTcpClient(new AsyncCallback(MultiScreenRequest), multiScreenRequestListener);
            multiScreenUpdateListener = new TcpListener(IPAddress.Any, 3004);
            Console.WriteLine("Listening...");
            multiScreenUpdateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            multiScreenUpdateListener.Start();
            multiScreenUpdateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenUpdate), multiScreenUpdateListener);
            playPauseListener = new TcpListener(IPAddress.Any, 3005);
            Console.WriteLine("Listening...");
            playPauseListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            playPauseListener.Start();
            playPauseListener.BeginAcceptSocket(new AsyncCallback(PlayPauseUpdate), playPauseListener);
            transitionAnimations = new List<Uri>();
            transitionAnimations.Add(new Uri("Resources/Idle Interaction Transition/MENU 1.mov", UriKind.Relative));
            //transitionAnimations.Add(new Uri("Resources/Idle Interaction Transition/MENU 2.mov", UriKind.Relative));
            transition.Source = transitionAnimations[0];
            transition.Play();
            transitionState++;
        }

        private void Swipetoexit_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Console.Out.WriteLine("Swipe Visibility Changed");
                if (!(sender as SwipeToExit).IsVisible)
                {
                    Console.Out.WriteLine("Swipe Hidden");
                    (sender as SwipeToExit).timer.Enabled = false;
                    if (currentActivityProcess != null && !currentActivityProcess.HasExited)
                    {
                        currentActivityProcess.Kill();
                        currentActivityProcess.Close();
                    }
                    else
                    {
                        Console.Out.WriteLine("Activity Null");
                    }
                    currentActivityProcess = null;
                    ActivityActive = false;
                }
                else
                {
                    Console.Out.WriteLine("Swipe Visible");
                    (sender as SwipeToExit).timer.Enabled = true;
                }
            }
            catch (Exception) { }
        }

        private void MultiScreenRequest(IAsyncResult ar)
        {
            try
            {
                if (inputTimer != null)
                {
                    inputTimer.Stop();
                }
                byte[] buffer = new byte[15];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);
                string packet = System.Text.Encoding.UTF8.GetString(buffer);
                Console.Out.WriteLine("got FileName: " + packet);
                String[] info = packet.Split('\t');
                if (info[0].Equals("MultiScreen"))
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            mb.Visibility = Visibility.Hidden;
                            MessageBox msgBox = new MessageBox();
                            msgBox.Topmost = true;
                            msgBox.ButtonClickedEvent += (sender, UserChoice) => { Mb_ButtonClickedEvent(sender, UserChoice, this.swipetoexit); };
                            msgBox.textBox.Text = "A User has Initiated Multi-Screen Mode. This means you will no longer have access to this screen for a period of time. Do you Approve?";
                            msgBox.textBox.Margin = new Thickness(10, 0, 10, 0);
                            msgBox.Show();
                        }));
                    }
                    else
                    {
                        mb.Visibility = Visibility.Hidden;
                        MessageBox msgBox = new MessageBox();
                        msgBox.Topmost = true;
                        msgBox.ButtonClickedEvent += (sender, UserChoice) => { Mb_ButtonClickedEvent(sender, UserChoice, this.swipetoexit); };
                        msgBox.textBox.Text = "A User has Initiated Multi-Screen Mode. This means you will no longer have access to this screen for a period of time. Do you Approve?";
                        msgBox.textBox.Margin = new Thickness(10, 0, 10, 0);
                        msgBox.Show();
                    }
                }
                multiScreenRequestListener.BeginAcceptTcpClient(new AsyncCallback(MultiScreenRequest), multiScreenRequestListener);
            }
            catch(Exception e)
            {
                resetTimer = true;
            }
        }

        private void Bw6_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                UserChoice choice = (UserChoice)e.UserState;
                if (choice.Approved)
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (!mb.IsVisible)
                            {
                                mb.Visibility = Visibility.Visible;
                            }
                            if (tmr != null)
                            {
                                tmr.Stop();
                                tmr.Dispose();
                                tmr = null;
                                Console.Out.WriteLine("MultiScreen Timer Disposed");
                            }
                            singlePlayer.Visibility = Visibility.Hidden;
                        }));
                    }
                    else
                    {
                        mb.Activate();
                        if (!mb.IsVisible)
                        {
                            mb.Visibility = Visibility.Visible;
                        }
                        if (tmr != null)
                        {
                            tmr.Stop();
                            tmr.Dispose();
                            tmr = null;
                            Console.Out.WriteLine("MultiScreen Timer Disposed");
                        }
                        singlePlayer.Visibility = Visibility.Hidden;
                    }
                    var reply = new byte[2];
                    reply[0] = 1;
                    reply[1] = App.monitorPosition;

                    TcpClient masterUpdateSocket = new TcpClient(App.masterIP, 4086);
                    masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slave1Update = masterUpdateSocket.GetStream();
                    slave1Update.Write(reply, 0, reply.Length);
                    Console.Out.WriteLine("Approved Master's Multi-Screen Mode");
                }
                else
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            mb.Visibility = Visibility.Hidden;
                        }));
                    }
                    else
                    {
                        mb.Visibility = Visibility.Hidden;
                    }
                    var reply = new byte[2];
                    reply[0] = 0;
                    reply[1] = App.monitorPosition;

                    TcpClient masterUpdateSocket = new TcpClient(App.masterIP, 4086);
                    masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slave1Update = masterUpdateSocket.GetStream();
                    slave1Update.Write(reply, 0, reply.Length);
                    Console.Out.WriteLine("Disapproved Master's Multi-Screen Mode");
                    resetTimer = true;
                }
            }
            catch (Exception)
            {
                resetTimer = true;
            }
        }

        private void Bw6_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            (sender as BackgroundWorker).Dispose();
        }

        private void Bw6_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                UserChoice choice = (UserChoice)e.Argument;
                BackgroundWorker worker = (BackgroundWorker)sender;
                worker.ReportProgress(0, choice);
            }
            catch (Exception te) { }
        }

        private void Mb_ButtonClickedEvent(object sender, UserChoice e, SwipeToExit ste)
        {
            BackgroundWorker bw6 = new BackgroundWorker();
            bw6.DoWork += Bw6_DoWork;
            bw6.RunWorkerCompleted += Bw6_RunWorkerCompleted;
            bw6.WorkerReportsProgress = true;
            bw6.ProgressChanged += Bw6_ProgressChanged;
            bw6.RunWorkerAsync(e);
            
        }
        /// <summary>
        /// Event to trigger when user closes Active Interaction. This exists so the Interaction Controller can know and
        /// subscribe to the event when the Active Interaction closes.
        /// </summary>
        public event EventHandler Closing;

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
            {
                //remove the current coaxer
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => CloseButton_Click(null, null)));
                return;
            }
            try
            {
                if (swipetoexit != null)
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            swipetoexit.Close();
                        }));
                    }
                    else
                    {
                        swipetoexit.Close();
                    }
                }
                if(inputTimer != null)
                {
                    inputTimer.Stop();
                    inputTimer.Dispose();
                }
                //grid.Height = Height;
                //MainMenu.Visibility = Visibility.Collapsed;
                //grid.Background = System.Windows.Media.Brushes.Black;
                Closing.BeginInvoke(this, new EventArgs(), null, null);
            }
            catch (Exception) { }
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                resetTimer = true;
                mouseup = false;
                grid.Margin = lastMargin;
                m_start = e.GetPosition(this);
                if (!grid.IsMouseCaptured)
                {
                    grid.CaptureMouse();
                }
                e.Handled = true;
            }
            catch (Exception) { }
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (!mouseup && grid.IsMouseCaptured && smallerGrid)
                {
                    Vector offset = System.Windows.Point.Subtract(e.GetPosition(this), m_start);
                    if ((grid.PointToScreen(new System.Windows.Point(0, 0)).Y >= 0 || -offset.Y <= grid.Margin.Bottom - lastMargin.Bottom || offset.Y >= 0) && (grid.Margin.Bottom >= 0 || grid.Margin.Bottom - offset.Y >= 0 || offset.Y <= lastMargin.Bottom || offset.Y <= 0))
                    {
                        grid.Margin = new Thickness(0, 0, 0, lastMargin.Bottom - offset.Y);
                    }
                }
                e.Handled = true;
            }
            catch (Exception) { }
        }

        private void Grid_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                resetTimer = true;
                this.mouseup = true;
                lastMargin = grid.Margin;
                grid.ReleaseMouseCapture();
                e.Handled = true;
            }
            catch (Exception) { }

        }


        /// <summary>
        /// Handle the routed event of a user selecting a specific activity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActivitySection_ActivityActivated(object sender, RoutedEventArgs e)
        {
            ActivityView activityViewSelected = (ActivityView)e.OriginalSource;
            _selectedActivity = (Activity)activityViewSelected.DataContext;
            RunActivity();

            e.Handled = true;
        }

        /// <summary>
        /// Launches the selected game activity as a external process. Sets the appropriate command line arguments 
        /// following the specification of game activities for the system
        /// </summary>
        private void beginGameActivity()
        {
            try
            {
                if (inputTimer != null)
                {
                    inputTimer.Stop();
                }
                // StringBuilder to create command line arguments
                StringBuilder arglist = new StringBuilder();

                arglist.Append("-pos-x " + Canvas.GetLeft(this) + " "); //x position
                arglist.Append("-pos-y " + Canvas.GetTop(this) + " "); //y position
                arglist.Append("-screen-height " + Height + " "); //height
                arglist.Append("-screen-width " + (Width - ActivityMargin));    //width

                ProcessStartInfo psi = new ProcessStartInfo(
                    Environment.CurrentDirectory + "\\Activities\\" + _selectedActivity.Type + "\\" + _selectedActivity.Name + "\\" + _selectedActivity.Files[0],
                    arglist.ToString());
                var proc = Process.Start(psi);
                proc.WaitForInputIdle();
                SetWindowPos(proc.MainWindowHandle, new IntPtr(-2), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                currentActivityProcess = proc;
                ActivityActive = true;
                proc.Exited += Proc_Exited;
                proc.EnableRaisingEvents = true;
                swipeClose = false;
                swipetoexit = new SwipeToExit();
                swipetoexit.Closed += Swipetoexit_Closed;
                swipetoexit.Show();
            }
            catch (Exception)
            {
                resetTimer = true;
            }

        }
        private void beginBrowserActivity()
        {
            try
            {
                if (inputTimer != null)
                {
                    inputTimer.Stop();
                }
                // StringBuilder to create command line arguments
                StringBuilder arglist = new StringBuilder();
                String url = _selectedActivity.Url;
                Console.Out.WriteLine(url);
                arglist.Append("-pos-x " + Canvas.GetLeft(this) + " "); //x position
                arglist.Append("-pos-y " + Canvas.GetTop(this) + " "); //y position
                arglist.Append("-screen-height " + Height + " "); //height
                arglist.Append("-screen-width " + (Width - ActivityMargin));    //width
                ProcessStartInfo psi = new ProcessStartInfo(
                    Environment.CurrentDirectory + "\\Activities\\" + _selectedActivity.Type + "\\" + "WpfApplication1.exe",
                    url);
                Process proc = Process.Start(psi);
                currentActivityProcess = proc;
                ActivityActive = true;
                proc.Exited += Proc_Exited;
                proc.EnableRaisingEvents = true;
                swipeClose = false;
                swipetoexit = new SwipeToExit();
                swipetoexit.Closed += Swipetoexit_Closed;
                swipetoexit.Show();
            }
            catch (Exception)
            {
                resetTimer = true;
            }
        }

        private void Swipetoexit_Closed(object sender, EventArgs e)
        {
            try
            {
                (sender as SwipeToExit).Closed -= Swipetoexit_Closed;
                if (currentActivityProcess != null && !currentActivityProcess.HasExited)
                {
                    currentActivityProcess.Kill();
                    currentActivityProcess.Close();
                    currentActivityProcess = null;
                    ActivityActive = false;
                }
                swipetoexit = null;
            }
            catch (Exception) { }
            resetTimer = true;
        }

        //TODO:Centralize the location of available image and video extensions
        private static List<string> imageExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
        private static List<string> videoExtensions = new List<string> { ".mpeg", ".wmv", ".mp4" };

        /// <summary>
        /// Launch a MediaPLayer with the specific media items specified by the selected activity
        /// </summary>
        private void beginMediaActivity()
        {
            if (inputTimer != null)
            {
                inputTimer.Stop();
            }
            double newMediaHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double totalScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double offset = 0;
            try
            {
                List<Uri> media = _selectedActivity.Files.Select(
                x => new Uri("pack://siteoforigin:,,,/Activities/" + _selectedActivity.Type + "/" + _selectedActivity.Name + "/" + x)).ToList();
                int originalMediaHeight = _selectedActivity.Height;
                int originalMediaWidth = _selectedActivity.Width;
                Console.Out.WriteLine(originalMediaHeight);
                Console.Out.WriteLine(originalMediaWidth);
                double expandedMediaWidth = (newMediaHeight * originalMediaWidth) / originalMediaHeight;
                while (expandedMediaWidth > totalScreenWidth)
                {
                    newMediaHeight -= 50;
                    expandedMediaWidth = (newMediaHeight * originalMediaWidth) / originalMediaHeight;
                }
                Console.Out.WriteLine("New Height: " + newMediaHeight);
                offset = (totalScreenWidth - expandedMediaWidth) / 2;
                //singlePlayer.mediaSlideshowVideo.mePlayer.Loaded += MePlayer_MediaOpened;
                singlePlayer.IsMultiScreen = false;
                singlePlayer.mePlayer.Source = media[0];
                //singlePlayer.mePlayer.Width = totalScreenWidth;
                singlePlayer.mePlayer.Height = newMediaHeight;
                singlePlayer.Width = totalScreenWidth;
                singlePlayer.grid.Width = totalScreenWidth;
                singlePlayer.grid.Margin = new Thickness(0);
                singlePlayer.Back.Margin = grid.Margin;
                singlePlayer.Activate();
                singlePlayer.multiscreenButton.Visibility = Visibility.Visible;
                singlePlayer.multiscreenButton.MouseDown += UserControl_MouseSingleClick;
                if (!singlePlayer.IsVisible)
                {
                    singlePlayer.Visibility = Visibility.Visible;
                }
                singlePlayer.mePlayer.Play();
            }
            catch (ArgumentException)
            {
                Console.Out.WriteLine("Player Not Playing");
                resetTimer = true;
            }
        }

        private void SinglePlayer_Closed(object sender, EventArgs e)
        {
            Console.Out.WriteLine("Player Closed");
            (sender as VidPlayer).Closed -= SinglePlayer_Closed;
            singlePlayer = null;
            resetTimer = true;
        }

        private void OnReceiveMultiScreenClose(IAsyncResult asyn)
        {
            try
            {
                Console.Out.WriteLine("Multi-Screen Closing Message Received");
                byte[] statusData = new byte[1];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(statusData, 0, statusData.Length);
                replyStream.Dispose();
                client.Close();
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (statusData[0] == 0)
                        {
                            if (tmr != null)
                            {
                                tmr.Stop();
                                tmr.Dispose();
                                tmr = null;
                                Console.Out.WriteLine("MultiScreen Timer Disposed");
                            }
                            singlePlayer.Visibility = Visibility.Hidden;
                            resetTimer = true;
                        }
                    }));
                }
                else
                {
                    if (statusData[0] == 0)
                    {
                        if (tmr != null)
                        {
                            tmr.Stop();
                            tmr.Dispose();
                            tmr = null;
                            Console.Out.WriteLine("MultiScreen Timer Disposed");
                        }
                        singlePlayer.Visibility = Visibility.Hidden;
                        resetTimer = true;
                    }
                }
                updateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenClose), updateListener);
            }
            catch(Exception e)
            {
                resetTimer = true;
            }
        }

        private void OnReceiveMultiScreenUpdate(IAsyncResult asyn)
        {
            multiScreenUpdateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenUpdate), multiScreenUpdateListener);
            try
            {
                Console.Out.WriteLine("Multi-Screen Player Change Received");
                byte[] buffer = new byte[500];

                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);
                byte[] statusData = new byte[bytesRead];
                Array.Copy(buffer, 0, statusData, 0, bytesRead);
                replyStream.Dispose();
                client.Close();
                multiStartTime = multiStartTime - BitConverter.ToDouble(statusData, 0);
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        singlePlayer.mePlayer.Position = TimeSpan.FromMilliseconds(DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds - multiStartTime);
                    }));
                }
                else
                {
                    singlePlayer.mePlayer.Position = TimeSpan.FromMilliseconds(DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds - multiStartTime);
                }
                Console.Out.WriteLine("newStartTime: " + multiStartTime);
            }
            catch (Exception e) { }
        }

        private void PlayPauseUpdate(IAsyncResult asyn)
        {
            playPauseListener.BeginAcceptSocket(new AsyncCallback(PlayPauseUpdate), playPauseListener);
            try
            {
                Console.Out.WriteLine("Pause-Play Received");
                byte[] buffer = new byte[10];

                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);
                byte[] statusData = new byte[bytesRead];
                Array.Copy(buffer, 0, statusData, 0, bytesRead);
                if(bytesRead == 1)
                {
                    if(statusData[0] == 0)
                    {
                        if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                        {
                            //remove the current coaxer
                            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                tmr.Stop();
                                singlePlayer.mePlayer.Pause();
                            }));
                        }
                        else
                        {
                            tmr.Stop();
                            singlePlayer.mePlayer.Pause();
                        }
                    }
                }
                else
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            multiStartTime = multiStartTime + BitConverter.ToDouble(statusData, 0);
                            tmr.Start();
                            singlePlayer.mePlayer.Play();
                        }));
                    }
                    else
                    {
                        multiStartTime = multiStartTime + BitConverter.ToDouble(statusData, 0);
                        tmr.Start();
                        singlePlayer.mePlayer.Play();
                    }
                }
                replyStream.Dispose();
                client.Close();
            }
            catch (Exception e) { }
        }
        private void UserControl_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (reset)
                {
                    if (inputTimer != null)
                    {
                        inputTimer.Stop();
                    }
                    this.reset = false;
                    mb.Topmost = true;
                    mb.Activate();
                    if (!mb.IsVisible)
                    {
                        mb.Visibility = Visibility.Visible;
                    }
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    try
                    {

                        TcpClient masterUpdateSocket = new TcpClient(App.masterIP, 4078);
                        masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        NetworkStream masterUpdate = masterUpdateSocket.GetStream();
                        //sb.Append("MultiScreen").Append("\t").Append(App.monitorPosition.ToString()).Append("\t");
                        sb.Append("MultiScreen").Append("\t").Append(_selectedActivity.Type).Append("\t").Append(_selectedActivity.Name).Append("\t").Append(_selectedActivity.Files[0]).Append("\t");
                        masterUpdate.Write(Encoding.ASCII.GetBytes(sb.ToString()), 0, Encoding.ASCII.GetBytes(sb.ToString()).Length);
                        masterUpdate.Dispose();
                        masterUpdateSocket.Close();
                        //masterUpdateListener.BeginAcceptTcpClient(new AsyncCallback(MasterUpdate), masterUpdateListener);
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("An error occured. Please try again.",

                            "Initialization Failed!",

                            MessageBoxButton.OK,

                            MessageBoxImage.Error,

                            MessageBoxResult.OK,

                            System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                        resetTimer = true;
                        return;

                    }
                }

                e.Handled = true;
            }
            catch (Exception)
            {
                resetTimer = true;
            }
        }

        public void MasterUpdate(IAsyncResult ar)
        {
            try
            {
                if (inputTimer != null)
                {
                    inputTimer.Stop();
                }
                byte[] buffer = new byte[500];
                Console.Out.WriteLine("Master Update Received");
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (tmr != null)
                        {
                            tmr.Stop();
                            tmr.Dispose();
                            tmr = null;
                            Console.Out.WriteLine("MultiScreen Timer Disposed");
                        }
                        singlePlayer.Visibility = Visibility.Hidden;
                    }));
                }
                else
                {
                    if (tmr != null)
                    {
                        tmr.Stop();
                        tmr.Dispose();
                        tmr = null;
                        Console.Out.WriteLine("MultiScreen Timer Disposed");
                    }
                    singlePlayer.Visibility = Visibility.Hidden;
                }
                BackgroundWorker bw4 = new BackgroundWorker();
                bw4.DoWork += Bw4_DoWork;
                bw4.RunWorkerCompleted += Bw4_RunWorkerCompleted;
                bw4.WorkerReportsProgress = true;
                bw4.ProgressChanged += Bw4_ProgressChanged;
                bw4.RunWorkerAsync(buffer);
                Console.Out.WriteLine("Master Update Length" + bytesRead);
                replyStream.Dispose();
                client.Close();
                masterUpdateListener.BeginAcceptTcpClient(new AsyncCallback(MasterUpdate), masterUpdateListener);
            }
            catch (Exception)
            {
                resetTimer = true;
            }
        }

        private void Bw4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                byte[] buffer = (byte[])e.UserState;
                ensurePlayerIsPlaying(buffer, buffer.Length, true);
            }
            catch (Exception) { }
        }

        private void Bw4_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            (sender as BackgroundWorker).Dispose();
        }

        private void Bw4_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                byte[] buffer = (byte[])e.Argument;
                e.Result = buffer;
                BackgroundWorker worker = (BackgroundWorker)sender;
                worker.ReportProgress(0, buffer);
            }
            catch (Exception) { }
        }

        public void ensurePlayerIsPlaying(byte[] buffer, int bytesRead, bool first)
        {
            if (inputTimer != null)
            {
                inputTimer.Stop();
            }
            string[] info = null;
            double newMediaHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double totalScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth * App.monitorCount;
            double offset = 0;
            byte[] actualData = new byte[bytesRead];
            Array.Copy(buffer, 0, actualData, 0, bytesRead);
            try
            {
                string packet = System.Text.Encoding.UTF8.GetString(actualData);
                info = packet.Split('\t');
                Console.Out.WriteLine("Folder Name: " + info[0]);
                Console.Out.WriteLine("File Name: " + info[2]);
                String res = System.IO.File.ReadAllText(Environment.CurrentDirectory + "\\Activities\\" + info[2] + "\\" + info[3] + "\\" + "Resolution.txt");
                string[] vidRes = res.Split('t');
                int originalMediaHeight = System.Convert.ToInt32(vidRes[1]);
                int originalMediaWidth = System.Convert.ToInt32(vidRes[0]);
                List<Uri> mediaElement = new List<Uri>(1);
                mediaElement.Add(new Uri("pack://siteoforigin:,,,/Activities/" + info[2] + "/" + info[3] + "/" + info[4]));
                double expandedMediaWidth = (newMediaHeight * originalMediaWidth) / originalMediaHeight;
                while (expandedMediaWidth > totalScreenWidth)
                {
                    newMediaHeight -= 50;
                    expandedMediaWidth = (newMediaHeight * originalMediaWidth) / originalMediaHeight;
                }
                Console.Out.WriteLine("New Height: " + newMediaHeight);
                offset = (totalScreenWidth - expandedMediaWidth) / 2;
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Console.Out.WriteLine("URI: " + mediaElement[0].ToString());
                        singlePlayer.IsMultiScreen = true;
                        singlePlayer.mePlayer.Source = mediaElement[0];
                        singlePlayer.multiscreenButton.Visibility = Visibility.Collapsed;
                        singlePlayer.Back.Margin = new Thickness(0);
                        singlePlayer.Back.Height = Double.NaN;
                        singlePlayer.grid.Width = expandedMediaWidth;
                        singlePlayer.mePlayer.Height = newMediaHeight;
                        singlePlayer.Closed += SinglePlayer_Closed;
                        singlePlayer.button.MouseDown += multiScreenClosebutton_MouseDown;
                        if (offset > 0)
                        {
                            if (App.monitorPosition == 1)
                            {
                                //singlePlayer.Margin = new Thickness(offset, 0, 0, 0);
                                singlePlayer.grid.Margin = new Thickness(offset, 0, 0, 0);
                            }
                            else
                            {
                                singlePlayer.grid.Margin = new Thickness((((App.monitorPosition - 1) * offset) - Screen.PrimaryScreen.WorkingArea.Width), 0, 0, 0);
                            }
                        }
                        multiStartTime = Convert.ToInt64(info[1]);
                        playTimer = new System.Timers.Timer();
                        playTimer.AutoReset = false;
                        playTimer.Interval = (multiStartTime - DateTime.Now.Ticks) / 10000;
                        playTimer.Enabled = true;
                        playTimer.Elapsed += PlayTimer_Elapsed;

                    }));
                }
                else
                {
                    Console.Out.WriteLine("URI1: " + mediaElement[0].ToString());
                    singlePlayer.IsMultiScreen = true;
                    singlePlayer.mePlayer.Source = mediaElement[0];
                    singlePlayer.multiscreenButton.Visibility = Visibility.Collapsed;
                    singlePlayer.Back.Margin = new Thickness(0);
                    singlePlayer.Back.Height = Double.NaN;
                    singlePlayer.grid.Width = expandedMediaWidth;
                    singlePlayer.mePlayer.Height = newMediaHeight;
                    singlePlayer.Closed += SinglePlayer_Closed;
                    singlePlayer.button.MouseDown += multiScreenClosebutton_MouseDown;
                    if (offset > 0)
                    {
                        if (App.monitorPosition == 1)
                        {
                            //singlePlayer.Margin = new Thickness(offset, 0, 0, 0);
                            singlePlayer.grid.Margin = new Thickness(offset, 0, 0, 0);
                        }
                        else
                        {
                            singlePlayer.grid.Margin = new Thickness((((App.monitorPosition - 1) * offset) - Screen.PrimaryScreen.WorkingArea.Width), 0, 0, 0);
                        }
                    }
                    multiStartTime = Convert.ToDouble(info[1]);
                    playTimer = new System.Timers.Timer();
                    playTimer.AutoReset = false;
                    playTimer.Interval = (multiStartTime - DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds);
                    playTimer.Enabled = true;
                    playTimer.Elapsed += PlayTimer_Elapsed;
                }


            }
            catch (Exception)
            {
                Console.Out.WriteLine("Player Not Playing");
                resetTimer = true;
            }

        }

        private void PlayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                (sender as System.Timers.Timer).Enabled = false;
                BackgroundWorker bw5 = new BackgroundWorker();
                bw5.DoWork += Bw5_DoWork;
                bw5.RunWorkerCompleted += Bw5_RunWorkerCompleted;
                bw5.WorkerReportsProgress = true;
                bw5.ProgressChanged += Bw5_ProgressChanged;
                bw5.RunWorkerAsync();
            }
            catch (Exception te) { }

        }
        private void Play()
        {
            if (inputTimer != null)
            {
                inputTimer.Stop();
            }
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
            {
                //remove the current coaxer
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Console.Out.WriteLine("Player bout Playing");
                    swipeClose = true;
                    mb.Visibility = Visibility.Hidden;
                    singlePlayer.Activate();
                    if (!singlePlayer.IsVisible)
                    {
                        singlePlayer.Visibility = Visibility.Visible;
                    }
                    singlePlayer.mePlayer.Play();
                    Console.Out.WriteLine("Player Playing: " + singlePlayer.IsVisible);
                }));
            }
            else
            {
                Console.Out.WriteLine("Player bout Playing 2");
                swipeClose = true;
                mb.Visibility = Visibility.Hidden;
                singlePlayer.Activate();
                if (!singlePlayer.IsVisible)
                {
                    singlePlayer.Visibility = Visibility.Visible;
                }
                singlePlayer.mePlayer.Play();
                Console.Out.WriteLine("Player Playing: " + singlePlayer.IsVisible);
            }
            tmr = new System.Timers.Timer();
            tmr.AutoReset = true;
            tmr.Interval = 5000;
            tmr.Enabled = true;
            tmr.Elapsed += (sender1, e1) => Tmr_Elapsed(sender1, e1, multiStartTime);
        }

        private void Bw5_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Play();
        }

        private void Bw5_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            (sender as BackgroundWorker).Dispose();
        }

        private void Bw5_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker worker = (BackgroundWorker)sender;
                worker.ReportProgress(0);
            }
            catch (Exception te) { }
        }

        private void Tmr_Elapsed(object sender1, ElapsedEventArgs e1, double startTime)
        {
            try
            {
                //Console.Out.WriteLine("Updating Player Position");
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (singlePlayer != null)
                        {
                            Console.Out.WriteLine("Updating Player Position: " + (DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds - multiStartTime));
                            singlePlayer.mePlayer.Position = TimeSpan.FromMilliseconds(DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds - multiStartTime);
                        }
                        else
                        {
                            (sender1 as System.Timers.Timer).Enabled = false;
                            (sender1 as System.Timers.Timer).Stop();
                            (sender1 as System.Timers.Timer).Dispose();

                        }
                    }));
                }
                else
                {
                    if (singlePlayer != null)
                    {
                        Console.Out.WriteLine("Updating Player Position: " + (DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds - multiStartTime));
                        singlePlayer.mePlayer.Position = TimeSpan.FromMilliseconds(DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds - multiStartTime);
                    }
                    else
                    {
                        (sender1 as System.Timers.Timer).Enabled = false;
                        (sender1 as System.Timers.Timer).Stop();
                        (sender1 as System.Timers.Timer).Dispose();

                    }

                }
            }
            catch (Exception te) { }
        }

        private void Proc_Exited(object sender, EventArgs e)
        {
            try
            {
                currentActivityProcess = null;
                ActivityActive = false;
                if (swipetoexit != null)
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            swipetoexit.Close();
                        }));
                    }
                    else
                    {
                        swipetoexit.Close();
                    }
                }
            }
            catch (Exception) { }
            resetTimer = true;
        }

        /// <summary>
        /// Allow user to forcefully stop game activities. this is put in place as a "fall-back" if the game is not allowing the user to
        /// exit it. We may also end up just using this as a means of quitting game activities
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunActivity()
        {
            if (_selectedActivity != null)
            {
                switch (_selectedActivity.Type)
                {
                    case "Game":
                        beginGameActivity();
                        break;
                    case "Media":
                        beginMediaActivity();
                        break;
                    case "Browser":
                        beginBrowserActivity();
                        break;
                    default:
                        throw new Exception("Invalid Activity type");
                }
            }
        }
        #region transitioning

        /// <summary>
        /// Flag to keep track of state of transition, going from:
        ///     0: has not started
        ///     1: Running first transition animation
        ///     2: Waiting for user to confirm transition
        /// </summary>
        private int transitionState = 0;

        /// <summary>
        /// List of animations to be run throughout different stages of transition
        /// </summary>
        private List<Uri> transitionAnimations;
        private bool mouseup = true;
        private bool smallerGrid = false;
        private bool reset = true;
        private BackgroundWorker bw;
        private IPEndPoint endPointA;
        private TcpListener updateListener;
        private TcpListener masterUpdateListener;
        private byte[] masterData = new byte[5];
        private TcpListener multiScreenRequestListener;
        private TcpListener multiScreenUpdateListener;
        private TcpListener playPauseListener;
        private Thickness lastMargin = new Thickness(0);
        private static double rescaleFactor = System.Windows.SystemParameters.PrimaryScreenWidth / 1080;
        private double multiStartTime;
        private System.Timers.Timer tmr;
        private System.Timers.Timer playTimer;
        private static System.Timers.Timer inputTimer;
        private static int inputTimerInterval = 180000;

        private void FadeOut_Completed(object sender, EventArgs e)
        {
            transitionGrid.Visibility = Visibility.Collapsed;
        }

        private void transition_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (transitionState)
                {
                    case 1: //Move to next state when first animation is over
                            /*transition.Close();
                            transition.Source = transitionAnimations[1];
                            transitionState++;*/
                        Storyboard fadeOut = FindResource("FadeOut") as Storyboard;
                        fadeOut.Completed += FadeOut_Completed;
                        fadeOut.Begin(transitionGrid);
                        transitionState = 0;
                        transition.Close();
                        grid.Height = 800 * rescaleFactor;
                        smallerGrid = true;
                        grid.Visibility = Visibility.Visible;
                        var converter = new System.Windows.Media.BrushConverter();
                        var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#b7b7b7");
                        grid.Background = brush;
                        inputTimer = new System.Timers.Timer();
                        inputTimer.AutoReset = false;
                        inputTimer.Interval = inputTimerInterval;
                        inputTimer.Elapsed += InputTimer_Elapsed;
                        inputTimer.Enabled = true;
                        break;
                    case 0:
                        break;
                    default:
                        throw new Exception("Invalid transition state");
                }
            }
            catch (Exception) { }
            e.Handled = true;
        }

        private void InputTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            (sender as System.Timers.Timer).Stop();
            CloseButton_Click(null, null);
        }

        #endregion

        /// <summary>
        /// Stop window from moving with scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void descriptionTextBox_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        public void multiScreenClosebutton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                (sender as System.Windows.Controls.Image).MouseDown -= multiScreenClosebutton_MouseDown;
            }
            byte[] status = new byte[1];
            status[0] = (byte)0;
            try
            {

                TcpClient masterUpdateSocket = new TcpClient(App.masterIP, 4079);
                NetworkStream masterUpdate = masterUpdateSocket.GetStream();
                masterUpdate.Write(status, 0, status.Length);
            }
            catch { }
        }
        public void Close()
        {
            try
            {
                if (currentActivityProcess != null && !currentActivityProcess.HasExited)
                {
                    currentActivityProcess.Kill();
                    currentActivityProcess.Close();
                }
                currentActivityProcess = null;
                ActivityActive = false;
                swipeClose = true;
                if (swipetoexit != null)
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            swipetoexit.Close();
                            swipetoexit = null;
                        }));
                    }
                    else
                    {
                        swipetoexit.Close();
                        swipetoexit = null;
                    }
                }
                if (singlePlayer != null)
                {
                    singlePlayer.Close();
                }
                if (mb != null)
                {
                    mb.Close();
                }
                updateListener.Stop();
                updateListener.Server.Dispose();
                masterUpdateListener.Stop();
                masterUpdateListener.Server.Dispose();
                multiScreenRequestListener.Stop();
                multiScreenRequestListener.Server.Dispose();
                header.Close();
            }
            catch (Exception) { }
        }

        private void userControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            resetTimer = true;
        }
    }
}


