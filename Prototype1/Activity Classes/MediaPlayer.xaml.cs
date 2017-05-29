using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;


namespace Prototype1.Activity_Classes
{
    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        private enum SlideshowDirection { Forward, Back }

        /// <summary>
        /// milliseconds * seconds * minutes * hours
        /// </summary>
        private const double timerInterval = ((1000.00 * 5.00) * 1.00) * 1.00;

        /// <summary>
        /// Timer to automatically go to next element in slideshow after <see cref="timerInterval"/> amount of time has ellapsed
        /// </summary>
        private Timer timer;

        /// <summary>
        /// List of media elements to play in slideshow
        /// </summary>
        private List<Uri> mediaElements;

        /// <summary>
        /// Current element index in slideshow
        /// </summary>
        private int index = 0;

        private List<string> imageExtensions;
        private List<string> videoExtensions;

        private bool _isInteractive;

        /// <summary>
        /// Flag to dictate wether or not interactive objects should be displayed
        /// </summary>
        public bool IsInteractive
        {
            get { return _isInteractive; }
            private set
            {
                _isInteractive = value;
                NotifyPropertyChanged("IsInteractive");
                NotifyPropertyChanged("InteractiveElementsVisibility");
                NotifyPropertyChanged("IsSlideshow");
            }
        }

        /// <summary>
        /// Used to bind the visibility of interactive objects, based on the IsInteractive flag
        /// </summary>
        public Visibility InteractiveElementsVisibility
        {
            get
            {
                return _isInteractive ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Visibility to dictate whether or not controls will be visible related to slideshow 'functionality'. Based on the the 
        /// IsInteractive flas and on how many media elements are loaded in the player
        /// </summary>
        public Visibility IsSlideshow
        {
            get
            {
                return (_isSlideshow & _isInteractive) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        /// <summary>
        /// Is True if there is more than 1 mediaElement to show, False if there is only one. This will ensure that the fade in/out animation
        /// will not occur
        /// </summary>
        private bool _isSlideshow;

        /// <summary>
        /// Used to hold the host of the current slideshow element
        /// </summary>
        private FrameworkElement currentSlideshowHost;

        public MediaPlayer(List<Uri> mediaFiles, List<string> videoExt, List<string> imageExt, bool isInteractive)
        {
            InitializeComponent();

            mediaElements = mediaFiles;
            imageExtensions = imageExt;
            videoExtensions = videoExt;
            mediaSlideshowVideo.mePlayer.MediaEnded += MediaSlideshow_MediaEnded;
        }

        /// <summary>
        /// Begins playing the media slideshow
        /// </summary>
        public void play()
        {
            FadeOut_Completed(null, null);
        }

        private void MediaSlideshow_MediaEnded(object sender, RoutedEventArgs e)
        {
            MoveSlideshow(SlideshowDirection.Forward);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MoveSlideshow(SlideshowDirection.Forward);
        }

        private void PlaySlideShow()
        {
            if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => PlaySlideShow()));
                return;
            }

            //Fade out current element before moving to next 
            if (currentSlideshowHost != null)
            {
                if (_isSlideshow)
                {
                    Storyboard fadeOut = FindResource("FadeOut") as Storyboard;
                    fadeOut.Begin(currentSlideshowHost);
                }
                else
                {
                    FadeOut_Completed(null, null);
                }
            }
        }

        private void FadeOut_Completed(object sender, EventArgs e)
        {
            Storyboard fadeIn = FindResource("FadeIn") as Storyboard;

            //Fade in next slideshow elment, and set up image or video based on the extension of the file
            if (imageExtensions.Any(a => mediaElements[index].OriginalString.EndsWith(a)))
            {
                mediaSlideshowImage.Visibility = Visibility.Visible;
                mediaSlideshowVideo.Visibility = Visibility.Hidden;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = mediaElements[index];
                image.EndInit();
                mediaSlideshowImage.Source = image;
                currentSlideshowHost = mediaSlideshowImage;
                timer.Start();
            }
            else
            {
                mediaSlideshowImage.Visibility = Visibility.Hidden;
                mediaSlideshowVideo.Visibility = Visibility.Visible;
                mediaSlideshowVideo.mePlayer.Source = mediaElements[index];
                mediaSlideshowVideo.mePlayer.Play();
                currentSlideshowHost = mediaSlideshowVideo;
                Console.Out.WriteLine("I am Playing");
            }
            if (_isSlideshow)
            {
                fadeIn.Begin(currentSlideshowHost);
            }
        }

        public event EventHandler Closing;

        public event PropertyChangedEventHandler PropertyChanged;

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

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSlideshow(SlideshowDirection.Forward);
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSlideshow(SlideshowDirection.Back);
        }

        /// <summary>
        /// Move forward or backwards an element based on parameter
        /// </summary>
        /// <param name="dir"></param>
        private void MoveSlideshow(SlideshowDirection dir)
        {
            timer.Stop();
            if (currentSlideshowHost is VideoAudioPlayer)
            {
                mediaSlideshowVideo.mePlayer.Stop();
                mediaSlideshowVideo.mePlayer.Close();
            }
            if (dir == SlideshowDirection.Forward)
            {
                if (index == mediaElements.Count - 1)
                {
                    this.Close();
                }
                else
                {
                    index++;
                }
            }
            else
            {
                if (index == 0)
                {
                    index = mediaElements.Count - 1;
                }
                else
                {
                    index--;
                }
            }
            PlaySlideShow();
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
    }
}
