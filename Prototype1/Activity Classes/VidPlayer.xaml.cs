using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Threading;
using System.Net.Sockets;

namespace Prototype1.Activity_Classes
{
    /// <summary>
    /// Interaction logic for VidPlayer.xaml
    /// </summary>
    public partial class VidPlayer : Window
    {
        private bool userIsDraggingSlider = false;
        private DispatcherTimer timer;
        private bool mouseup = true;
        private Thickness lastMargin = new Thickness(0);
        System.Windows.Point m_start;
        private bool reset = true;
        private bool multiscreen = false;
        private ActiveAppSpace aa;
        private static double rescaleFactor = System.Windows.SystemParameters.PrimaryScreenWidth / 1080;
        private double oldValue = 0;
        private bool updated = false;

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
                                            typeof(VideoAudioPlayer));

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

        public bool IsMultiScreen
        {
            get
            {
                return multiscreen;
            }
            set
            {
                multiscreen = value;
                if (!multiscreen)
                {
                    grid.SizeChanged += grid_SizeChanged;
                    button.MouseDown += button_Click;
                    sliProgress.IsMoveToPointEnabled = true;
                }
                else
                {
                    grid.SizeChanged -= grid_SizeChanged;
                    button.MouseDown -= button_Click;
                    sliProgress.IsMoveToPointEnabled = false;
                }
            }
        }

        public static readonly DependencyProperty IsPausedProperty =
                DependencyProperty.Register("IsPaused",
                                            typeof(bool),
                                            typeof(VideoAudioPlayer));

        public VidPlayer(ActiveAppSpace aa)
        {
            Console.Out.WriteLine("VidPlayer Created");
            InitializeComponent();
            this.IsVisibleChanged += VidPlayer_IsVisibleChanged;
            this.aa = aa;
        }

        private void VidPlayer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                ActiveAppSpace.resetTimer = true;
                if (!IsVisible)
                {
                    if (timer != null)
                    {
                        timer.Stop();
                        timer = null;
                    }
                    mePlayer.Stop();
                    mePlayer.Source = null;
                    mePlayer.Close();
                }
            }
            catch (Exception) { }
        }


        private void MePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((mePlayer.NaturalDuration.HasTimeSpan))
                {
                    sliProgress.Minimum = 0;
                    sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                }
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(1);
                timer.Tick += timer_Tick;
                timer.Start();
                Console.Out.WriteLine("VideoPlayer Playing");
            }
            catch (Exception) { }
        }

        private void MePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }
                if (multiscreen)
                {
                    aa.multiScreenClosebutton_MouseDown(null, null);
                }
                else
                {
                    Hide();
                }
            }
            catch (Exception) { }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
                {
                    updated = true;
                    sliProgress.Value = mePlayer.Position.TotalMilliseconds;
                    TimeSpan newPosition = TimeSpan.FromMilliseconds(sliProgress.Value);
                    lblProgressStatus.Text = newPosition.ToString(@"hh\:mm\:ss");
                    updated = false;
                }
            }
            catch (Exception) { }
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
            oldValue = sliProgress.Value;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (userIsDraggingSlider)
            {
                if (IsMultiScreen)
                {
                    sliProgress_MouseDown(null, new MouseButtonEventArgs(Mouse.PrimaryDevice, new TimeSpan(DateTime.Now.Ticks).Milliseconds, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonDownEvent
                    });
                }
                else
                {
                    updatePosition();
                }
            }
            userIsDraggingSlider = false;
            e.Handled = true;
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //updatePosition();
            if (!updated)
            {
                Console.Out.WriteLine("Updating:...");
                TimeSpan newPosition = TimeSpan.FromMilliseconds(sliProgress.Value);
                lblProgressStatus.Text = newPosition.ToString(@"hh\:mm\:ss");
                if (!userIsDraggingSlider)
                {
                    updatePosition();
                }
            }

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
            Hide();
        }
        private void button_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Image).Height += 10;
            (sender as Image).Width += 10;
        }

        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Image).Height -= 10;
            (sender as Image).Width -= 10;
        }

        /// <summary>
        /// Update the player with the value gathered from the slider
        /// </summary>
        private void updatePosition()
        {
            try
            {
                TimeSpan newPosition = TimeSpan.FromMilliseconds(sliProgress.Value);
                lblProgressStatus.Text = newPosition.ToString(@"hh\:mm\:ss");
                mePlayer.Position = newPosition;
            }
            catch (Exception) { }
        }

        private void mePlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMultiScreen)
            {
                byte[] data = new byte[1];
                if (IsPaused)
                {
                    data[0] = 1;
                    IsPaused = false;
                }
                else
                {
                    data[0] = 0;
                    IsPaused = true;
                }
                TcpClient masterUpdateSocket = new TcpClient(App.masterIP, 3005);
                masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                NetworkStream masterUpdate = masterUpdateSocket.GetStream();
                masterUpdate.Write(data, 0, data.Length);
                masterUpdate.Dispose();
                masterUpdateSocket.Close();
            }
            else
            {
                if (IsPaused)
                {
                    mePlayer.Play();
                    IsPaused = false;
                }
                else
                {
                    mePlayer.Pause();
                    IsPaused = true;
                }
            }
        }
        private void multiScreenButton_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (reset)
                {
                    this.reset = false;
                    multiscreenButton.IsEnabled = false;
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += Bw_DoWork;
                    bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
                    bw.RunWorkerAsync();
                }
            }
            catch (Exception) { }
        }
        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                mouseUp();
            }
            catch (Exception) { }
        }
        private void mouseUp()
        {
            try
            {
                Thread.Sleep(500);
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        multiscreenButton.IsEnabled = true;
                    }));
                }
                this.reset = true;
            }
            catch (Exception) { }
        }
        private void Grid_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mouseup = false;
            (sender as Grid).Margin = lastMargin;
            m_start = e.GetPosition(this);
            if (!(sender as Grid).IsMouseCaptured)
            {
                (sender as Grid).CaptureMouse();
            }
            e.Handled = true;
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!mouseup && (sender as Grid).IsMouseCaptured)
            {
                Vector offset = System.Windows.Point.Subtract(e.GetPosition(this), m_start);
                if (((sender as Grid).PointToScreen(new System.Windows.Point(0, 0)).Y >= 0 || -offset.Y <= (sender as Grid).Margin.Bottom - lastMargin.Bottom || offset.Y >= 0) && ((sender as Grid).Margin.Bottom >= 0 || (sender as Grid).Margin.Bottom - offset.Y >= 0 || offset.Y <= lastMargin.Bottom || offset.Y <= 0))
                {
                    (sender as Grid).Margin = new Thickness(0, 0, 0, lastMargin.Bottom - offset.Y);
                }
            }
            e.Handled = true;
        }

        private void Grid_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.mouseup = true;
            lastMargin = (sender as Grid).Margin;
            (sender as Grid).ReleaseMouseCapture();
            e.Handled = true;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Console.Out.WriteLine("Closing Called");
                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }
                mePlayer.Stop();
                mePlayer.Source = null;
                mePlayer.Close();
            }
            catch (Exception) { }
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Back.Height = (sender as Grid).ActualHeight + 2 * ((10 * rescaleFactor) + button.ActualHeight);
            }
            catch (Exception) { }
        }

        private void sliProgress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.Out.WriteLine("MSEDWNNN");
            if (IsMultiScreen)
            {
                double update = (e.GetPosition(sliProgress).X / sliProgress.ActualWidth) * sliProgress.Maximum;
                byte[] data = BitConverter.GetBytes(update);
                Console.Out.WriteLine("SENT UPDATE: " + update);
                Console.Out.WriteLine("Player Position: " + sliProgress.Value);
                TcpClient masterUpdateSocket = new TcpClient(App.masterIP, 3004);
                masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                NetworkStream masterUpdate = masterUpdateSocket.GetStream();
                masterUpdate.Write(data, 0, data.Length);
                masterUpdate.Dispose();
                masterUpdateSocket.Close();
            }
        }
    }
}
