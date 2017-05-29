using System;
using System.Collections.Generic;
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
using Prototype1.Activity_Classes;
using System.Threading;
using System.ComponentModel;

namespace Prototype1.Activity_Classes
{
    /// <summary>
    /// Interaction logic for ActivityView.xaml
    /// </summary>
    public partial class ActivityView : UserControl
    {
        BackgroundWorker bw;
        private bool reset = true;
        private static int rescaleFactor = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 1080; //Default rescale factor is 1
        public ActivityView()
        {
            InitializeComponent();
            this.Height = this.Height * rescaleFactor;
            grid.Width = grid.Width * rescaleFactor;
            ActivityIcon.Width = ActivityIcon.Width * rescaleFactor;
            ActivityIcon.Height = ActivityIcon.Height * rescaleFactor;
            descriptionTextBox.Width = descriptionTextBox.Width * rescaleFactor;
            descriptionTextBox.Height = descriptionTextBox.Height * rescaleFactor;
            descriptionTextBox.MaxHeight = descriptionTextBox.MaxHeight * rescaleFactor;
            descriptionTextBox.FontSize = descriptionTextBox.FontSize * rescaleFactor;
            descriptionTextBox.Margin = new Thickness(descriptionTextBox.Margin.Left * rescaleFactor, descriptionTextBox.Margin.Top * rescaleFactor, descriptionTextBox.Margin.Right * rescaleFactor, descriptionTextBox.Margin.Bottom * rescaleFactor);
            ActivityIcon.Margin = new Thickness(ActivityIcon.Margin.Left * rescaleFactor, ActivityIcon.Margin.Top * rescaleFactor, ActivityIcon.Margin.Right * rescaleFactor, ActivityIcon.Margin.Bottom * rescaleFactor);
            runButton.Width = runButton.Width * rescaleFactor;
        }

        /// <summary>
        /// Routed event to let Active Interaction know when user has selected this Activity
        /// </summary>
        public static readonly RoutedEvent ActivityActivatedEvent =
        EventManager.RegisterRoutedEvent("ActivityActivated",
                                         RoutingStrategy.Tunnel,
                                         typeof(RoutedEventHandler),
                                         typeof(ActivityView));


        public event RoutedEventHandler ActivityActivated
        {
            add { AddHandler(ActivityActivatedEvent, value); }
            remove { RemoveHandler(ActivityActivatedEvent, value); }
        }

        private void UserControl_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            runButton.Focus();
            ActiveAppSpace.resetTimer = true;
            if (reset)
            {
                this.reset = false;
                runButton.IsEnabled = false;
                Console.Out.WriteLine("Run Clicked");
                RoutedEventArgs args = new RoutedEventArgs(ActivityActivatedEvent);
                RaiseEvent(args);
                this.bw = new BackgroundWorker();
                bw.DoWork += Bw_DoWork;
                bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
                bw.RunWorkerAsync();

            }
            e.Handled = true;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bw.Dispose();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            mouseUp();
        }

        private void button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            runButton.Height += 10;
            runButton.Width += 10;
        }

        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            runButton.Height -= 10;
            runButton.Width -= 10;

        }

        //This Prevents DoubleClick
        private void mouseUp()
        {
            Thread.Sleep(500);
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
            {
                //remove the current coaxer
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    runButton.IsEnabled = true;
                }));
            }
            this.reset = true;
        }

        private void descriptionTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
        }
    }
}
