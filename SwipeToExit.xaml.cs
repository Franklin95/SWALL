using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for SwipeToExit.xaml
    /// </summary>
    public partial class SwipeToExit : Window
    {
        private Point m_start;

        double bottom = 500;

        public Timer timer;
        private double timerInterval = 70;
        private static double rescaleFactor = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 1080;
        public SwipeToExit()
        {
            InitializeComponent();
            bottom = bottom * rescaleFactor;
            timer = new Timer();

            timer.Interval = timerInterval;

            timer.Elapsed += Timer_Elapsed;

            timer.AutoReset = true;

            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)

        {
            try
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess()) //Must execute this method on the UI thread

                {

                    //remove the current coaxer

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>

                    {
                        try
                        {
                            Console.Out.WriteLine("SwipeClose: " + ActiveAppSpace.swipeClose);
                            if (ActiveAppSpace.swipeClose)
                            {
                                timer.Enabled = false;
                                Close();
                            }
                            else
                            {
                                Thickness thick = new Thickness(0, 0, 0, bottom);



                                border.Margin = thick;

                                if (bottom == 850 * rescaleFactor)

                                {

                                    border.Visibility = Visibility.Visible;

                                    textBox.FontSize = 26 * rescaleFactor;

                                }

                                if (bottom >= 1050 * rescaleFactor)

                                {

                                    border.Visibility = Visibility.Hidden;

                                    textBox.FontSize = 25 * rescaleFactor;

                                    bottom = 500 * rescaleFactor;

                                }



                                bottom += 10;
                            }
                        }
                        catch (Exception ce) { }
                    }));
                }
            }
            catch (Exception ce) { }

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)

        {

            m_start = e.GetPosition(mainWindow);

            if (!grid.IsMouseCaptured)

            {

                grid.CaptureMouse();

            }

        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)

        {

            if (grid.IsMouseCaptured)

            {

                Vector offset = Point.Subtract(e.GetPosition(mainWindow), m_start);

                if (offset.Y >= 100 || offset.Y <= -100)

                {

                    grid.ReleaseMouseCapture();

                    timer.Enabled = false;
                    MessageBox mb = new MessageBox();
                    mb.ButtonClickedEvent += Mb_ButtonClickedEvent;
                    mb.textBox.Text = "Would you like to close the current Activity?";
                    mb.Show();

                }

            }

        }
        private void Mb_ButtonClickedEvent(object sender, UserChoice e)
        {
            if (e.Approved)
            {
                this.Close();
            }
            else
            {
                timer.Enabled = true;
            }
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)

        {

            if (grid.IsMouseCaptured)

            {

                grid.ReleaseMouseCapture();

            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grid.Width = grid.Width * rescaleFactor;
            border.Height = border.Height * rescaleFactor;
            border.Width = border.Width * rescaleFactor;
            textBox.FontSize = textBox.FontSize * rescaleFactor;
            Swipe.Width = Swipe.Width * rescaleFactor;
        }

    }
}

