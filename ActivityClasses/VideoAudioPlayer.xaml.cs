using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Prototype1.Activity_Classes
{
    /// <summary>
    /// Interaction logic for VideoAudioPLayer.xaml
    /// </summary>
    public partial class VideoAudioPlayer : UserControl
    {

        private bool userIsDraggingSlider = false;
        public MediaElement mePlayer = null;

        private DispatcherTimer timer;

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

        public static readonly DependencyProperty IsPausedProperty =
                DependencyProperty.Register("IsPaused",
                                            typeof(bool),
                                            typeof(VideoAudioPlayer));

        public VideoAudioPlayer()
        {
            InitializeComponent();
            this.mePlayer = new MediaElement();
            mePlayer.LoadedBehavior = MediaState.Manual;
            mePlayer.Stretch = System.Windows.Media.Stretch.Fill;
            mePlayer.ScrubbingEnabled = true;
            mePlayer.MouseDown += mePlayer_MouseDown;
            mePlayer.MediaEnded += MePlayer_MediaEnded;
            mePlayer.MediaOpened += MePlayer_MediaOpened;
            grid.Children.Add(mePlayer);
            Grid.SetRow(mePlayer, 0);
        }

        private void MePlayer_MediaOpened(object sender, RoutedEventArgs e)
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

        private void MePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer = null;
            Closing.Invoke(this, null);
            if (e != null)
                e.Handled = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Value = mePlayer.Position.TotalMilliseconds;
            }
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            updatePosition();
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //updatePosition();
        }
        public event EventHandler Closing;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Closing.Invoke(this, null);
            if (e != null)
                e.Handled = true;
        }
        public void Close()
        {
            button_Click(null, null);
        }
        private void button_MouseEnter(object sender, MouseEventArgs e)
        {
            button.Height += 10;
            button.Width += 10;
        }

        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            button.Height -= 10;
            button.Width -= 10;
        }

        /// <summary>
        /// Update the player with the value gathered from the slider
        /// </summary>
        private void updatePosition()
        {
            TimeSpan newPosition = TimeSpan.FromMilliseconds(sliProgress.Value);
            lblProgressStatus.Text = newPosition.ToString(@"hh\:mm\:ss");
            mePlayer.Position = newPosition;
        }

        private void mePlayer_MouseDown(object sender, MouseButtonEventArgs e)
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
}
