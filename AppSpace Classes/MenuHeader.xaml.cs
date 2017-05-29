using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for MenuHeader.xaml
    /// </summary>
    public partial class MenuHeader : UserControl, INotifyPropertyChanged
    {
        //***May not be needed, windows may start the OSK if it registers the touch input on the textbox..
        private Process onScreenKeyboardProcess;
        private string onScrteenKeyboardProcessLocation = Environment.GetFolderPath(Environment.SpecialFolder.System) + "/osk.exe";
        private static double rescaleFactor = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 1080;
        public MenuHeader()
        {
            InitializeComponent();
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            Rect.Width = Rect.Width * rescaleFactor;
            searchTextBox.Width = searchTextBox.Width * rescaleFactor;
            searchTextBox.FontSize = searchTextBox.FontSize * rescaleFactor;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The tag that the user has entered to search the activity set for
        /// </summary>
        public string SearchTag
        {
            get
            {
                return GetValue(SearchTagProperty) as string;
            }
            set
            {
                SetValue(SearchTagProperty, value);
                NotifyPropertyChanged("SearchTag"); ;
            }
        }

        public static readonly DependencyProperty SearchTagProperty =
                DependencyProperty.Register("SearchTag",
                                            typeof(string),
                                            typeof(MenuHeader),
                                            new PropertyMetadata(new PropertyChangedCallback(OnSearchTagPropertyChanged)));
        private VirtualKeyboard customKeyboard;

        public static void OnSearchTagPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var sl = sender as MenuHeader;
            sl.NotifyPropertyChanged("SearchTag");
        }


        //***May not be needed, windows may start the OSK if it registers the touch input on the textbox..
        //***Only works if the Solutions Platform is x64!!!!
        private void searchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
            /*if (onScreenKeyboardProcess == null)
            {
                ProcessStartInfo psi = new ProcessStartInfo(onScrteenKeyboardProcessLocation);
                onScreenKeyboardProcess = Process.Start(psi);
                onScreenKeyboardProcess.EnableRaisingEvents = true;
                onScreenKeyboardProcess.Exited += OnScreenKeyboardProcess_Exited;
            }*/
            if (customKeyboard == null)
            {
                customKeyboard = new VirtualKeyboard(this);
                customKeyboard.Closed += CustomKeyboard_Closed;
                customKeyboard.Show();
            }
            searchTextBox.SelectAll();

        }

        private void CustomKeyboard_Closed(object sender, EventArgs e)
        {
            customKeyboard.Closed -= CustomKeyboard_Closed;
            customKeyboard = null;
            CULogo.Focus();
        }

        //***May not be needed, windows may start the OSK if it registers the touch input on the textbox..
        private void OnScreenKeyboardProcess_Exited(object sender, EventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
            if (onScreenKeyboardProcess != null)
            {
                onScreenKeyboardProcess.Exited -= OnScreenKeyboardProcess_Exited;
                onScreenKeyboardProcess = null;
            }
        }

        //***May not be needed, windows may start the OSK if it registers the touch input on the textbox..
        private void searchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
            /*if (onScreenKeyboardProcess != null)
            {
                onScreenKeyboardProcess.Kill();
                onScreenKeyboardProcess = null;
            }*/
            if (customKeyboard != null)
            {
                customKeyboard.Close();
                customKeyboard = null;
            }
        }
        private void CULogo_Loaded(object sender, RoutedEventArgs e)
        {
            Rect.Height = CULogo.ActualHeight * 25 / 96;
            searchTextBox.Height = CULogo.ActualHeight * 25 / 96;
            e.Handled = true;
        }

        private void CULogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
            (sender as Image).Focus();
        }
        
        public void Close()
        {
            searchTextBox_LostFocus(null, null);
        }
    }
}
