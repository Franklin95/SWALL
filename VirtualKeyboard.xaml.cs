using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class VirtualKeyboard : Window
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0010;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        String initiatingApp = "vshost32";
        private bool mouseup = true;
        private Thickness lastMargin = new Thickness(0);
        private System.Windows.Point m_start;
        private double GpiSafeHeight;
        private bool closeInitiated = false;
        private UIElement control;
        private static double rescaleFactor = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 1080;
        public VirtualKeyboard(UIElement control)
        {
            Console.Out.WriteLine("Virtual Keyboard Created");
            this.control = control;
            InitializeComponent();
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (control != null)
            {
                Keyboard.Focus((control as MenuHeader).searchTextBox);
                //control.Focus();
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }
        void ActivateApp(string processName)
        {
            Process[] p = Process.GetProcessesByName(processName);

            // Activate the first application we find with this name
            if (p.Count() > 0)
                SetForegroundWindow(p[0].MainWindowHandle);

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...

            return IntPtr.Zero;
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }


        private void button_Copy28_Click(object sender, RoutedEventArgs e)
        {
            /*IntPtr hWnd = new WindowInteropHelper(this).Handle;
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);*/
            //Keyboard.Focus((control as MenuHeader).searchTextBox);
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            //control.Focus();
            //this.Focus();
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("Q");
            }
            else
            {
                SendKeys.SendWait("q");
            }
            e.Handled = true;
        }

        private void button_Copy29_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("W");
            }
            else
            {
                SendKeys.SendWait("w");
            }
        }

        private void button_Copy30_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("E");
            }
            else
            {
                SendKeys.SendWait("e");
            }
        }

        private void button_Copy31_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("R");
            }
            else
            {
                SendKeys.SendWait("r");
            }
        }

        private void button_Copy32_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("T");
            }
            else
            {
                SendKeys.SendWait("t");
            }
        }

        private void button_Copy33_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("Y");
            }
            else
            {
                SendKeys.SendWait("y");
            }
        }

        private void button_Copy34_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("U");
            }
            else
            {
                SendKeys.SendWait("u");
            }
        }

        private void button_Copy35_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("I");
            }
            else
            {
                SendKeys.SendWait("i");
            }
        }

        private void button_Copy36_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("O");
            }
            else
            {
                SendKeys.SendWait("o");
            }
        }

        private void button_Copy37_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("P");
            }
            else
            {
                SendKeys.SendWait("p");
            }
        }

        private void button_Copy27_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{TAB}");
        }

        private void button_Copy38_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{{}");
            }
            else
            {
                SendKeys.SendWait("[");
            }
        }

        private void button_Copy39_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{}}");
            }
            else
            {
                SendKeys.SendWait("]");
            }
        }

        private void button_Copy40_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("|");
            }
            else
            {
                SendKeys.SendWait("\\");
            }
        }

        private void button_Copy42_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("A");
            }
            else
            {
                SendKeys.SendWait("a");
            }
        }

        private void button_Copy43_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("S");
            }
            else
            {
                SendKeys.SendWait("s");
            }
        }

        private void button_Copy44_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("D");
            }
            else
            {
                SendKeys.SendWait("d");
            }
        }

        private void button_Copy45_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("F");
            }
            else
            {
                SendKeys.SendWait("f");
            }
        }

        private void button_Copy46_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("G");
            }
            else
            {
                SendKeys.SendWait("g");
            }
        }

        private void button_Copy47_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("H");
            }
            else
            {
                SendKeys.SendWait("h");
            }
        }

        private void button_Copy48_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("J");
            }
            else
            {
                SendKeys.SendWait("j");
            }
        }

        private void button_Copy49_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("K");
            }
            else
            {
                SendKeys.SendWait("k");
            }
        }

        private void button_Copy50_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("L");
            }
            else
            {
                SendKeys.SendWait("l");
            }
        }

        private void button_Copy51_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait(":");
            }
            else
            {
                SendKeys.SendWait(";");
            }
        }

        private void button_Copy52_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("\"");
            }
            else
            {
                SendKeys.SendWait("'");
            }
        }

        private void button_Copy53_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{ENTER}");
        }

        private void button_Copy55_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("Z");
            }
            else
            {
                SendKeys.SendWait("z");
            }
        }

        private void button_Copy56_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("X");
            }
            else
            {
                SendKeys.SendWait("x");
            }
        }

        private void button_Copy57_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("C");
            }
            else
            {
                SendKeys.SendWait("c");
            }
        }

        private void button_Copy58_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("V");
            }
            else
            {
                SendKeys.SendWait("v");
            }
        }

        private void button_Copy59_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("B");
            }
            else
            {
                SendKeys.SendWait("b");
            }
        }

        private void button_Copy60_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("N");
            }
            else
            {
                SendKeys.SendWait("n");
            }
        }

        private void button_Copy61_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (CapsLock.IsChecked == true || Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("M");
            }
            else
            {
                SendKeys.SendWait("m");
            }
        }

        private void button_Copy62_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("<");
            }
            else
            {
                SendKeys.SendWait(",");
            }
        }

        private void button_Copy63_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait(">");
            }
            else
            {
                SendKeys.SendWait(".");
            }
        }

        private void button_Copy64_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("?");
            }
            else
            {
                SendKeys.SendWait("/");
            }
        }

        private void button_Copy68_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("^{ESC}");
        }

        private void button_Copy70_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait(" ");
        }

        private void button_Copy72_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("^{ESC}");
        }

        private void button_Copy13_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{~}");
            }
            else
            {
                SendKeys.SendWait("`");
            }
        }

        private void button_Copy14_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("!");
            }
            else
            {
                SendKeys.SendWait("1");
            }
        }

        private void button_Copy15_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("@");
            }
            else
            {
                SendKeys.SendWait("2");
            }
        }

        private void button_Copy16_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("#");
            }
            else
            {
                SendKeys.SendWait("3");
            }
        }

        private void button_Copy17_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("$");
            }
            else
            {
                SendKeys.SendWait("4");
            }
        }

        private void button_Copy18_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{%}");
            }
            else
            {
                SendKeys.SendWait("5");
            }
        }

        private void button_Copy19_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{^}");
            }
            else
            {
                SendKeys.SendWait("6");
            }
        }

        private void button_Copy20_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("&");
            }
            else
            {
                SendKeys.SendWait("7");
            }
        }

        private void button_Copy21_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("*");
            }
            else
            {
                SendKeys.SendWait("8");
            }
        }

        private void button_Copy22_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{(}");
            }
            else
            {
                SendKeys.SendWait("9");
            }
        }

        private void button_Copy23_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{)}");
            }
            else
            {
                SendKeys.SendWait("0");
            }
        }

        private void button_Copy24_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("_");
            }
            else
            {
                SendKeys.SendWait("-");
            }
        }

        private void button_Copy25_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (Shift1.IsChecked == true || Shift2.IsChecked == true)
            {
                SendKeys.SendWait("{+}");
            }
            else
            {
                SendKeys.SendWait("=");
            }
        }

        private void button_Copy26_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{BACKSPACE}");
        }

        private void button_Copy82_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{UP}");
        }

        private void button_Copy84_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{LEFT}");
        }

        private void button_Copy85_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{DOWN}");
        }

        private void button_Copy86_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{RIGHT}");
        }

        private void button_Copy75_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{INSERT}");
        }

        private void button_Copy76_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{HOME}");
        }

        private void button_Copy77_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{PGUP}");
        }

        private void button_Copy78_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{DELETE}");
        }

        private void button_Copy79_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{END}");
        }

        private void button_Copy80_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{PGDN}");
        }

        private void PrtScn_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{PRTSC}");
        }

        private void ScrLk_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{SCROLLLOCK}");
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{PAUSE}");
        }

        private void button_Copy91_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("7");
            }
            else
            {
                SendKeys.SendWait("{HOME}");
            }
        }

        private void button_Copy88_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{DIVIDE}");
        }

        private void button_Copy89_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{MULTIPLY}");
        }

        private void button_Copy90_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{SUBTRACT}");
        }

        private void button_Copy92_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("{8}");
            }
            else
            {
                SendKeys.SendWait("{UP}");
            }
        }

        private void button_Copy93_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("9");
            }
            else
            {
                SendKeys.SendWait("{PGUP}");
            }
        }

        private void button_Copy94_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{ADD}");
        }

        private void button_Copy95_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("4");
            }
            else
            {
                SendKeys.SendWait("{LEFT}");
            }
        }

        private void button_Copy96_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("5");
            }
        }

        private void button_Copy97_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("6");
            }
            else
            {
                SendKeys.SendWait("{RIGHT}");
            }
        }

        private void button_Copy98_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("1");
            }
            else
            {
                SendKeys.SendWait("{END}");
            }
        }

        private void button_Copy99_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("2");
            }
            else
            {
                SendKeys.SendWait("{DOWN}");
            }
        }

        private void button_Copy100_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("3");
            }
            else
            {
                SendKeys.SendWait("{PGDN}");
            }
        }

        private void button_Copy101_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{ENTER}");
        }

        private void button_Copy102_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait("0");
            }
            else
            {
                SendKeys.SendWait("{INS}");
            }
        }

        private void button_Copy104_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            if (NumLock.IsChecked == true)
            {
                SendKeys.SendWait(".");
            }
            else
            {
                SendKeys.SendWait("{DEL}");
            }
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{ESC}");
        }

        private void button_Copy1_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F1}");
        }

        private void button_Copy2_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F2}");
        }
        private void button_Copy3_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F3}");
        }

        private void button_Copy4_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F4}");
        }

        private void button_Copy5_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F5}");
        }

        private void button_Copy6_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F6}");
        }

        private void button_Copy7_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F7}");
        }

        private void button_Copy8_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F8}");
        }

        private void button_Copy9_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F9}");
        }

        private void button_Copy10_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F10}");
        }

        private void button_Copy11_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F11}");
        }

        private void button_Copy12_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((control as MenuHeader).searchTextBox);
            SendKeys.SendWait("{F12}");
            e.Handled = true;
        }

        private void closeButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            closeButton.Width += 5;
        }

        private void closeButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            closeButton.Width -= 5;
        }

        private void closeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            closeInitiated = true;
            this.Close();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!closeInitiated)
            {
                mouseup = false;
                grid.Margin = lastMargin;
                m_start = e.GetPosition(this);
                GpiSafeHeight = GetDpiSafeResolution().Width;
                if (!grid.IsMouseCaptured)
                {
                    grid.CaptureMouse();
                }
            }
            e.Handled = true;
        }
        private System.Windows.Size GetDpiSafeResolution()
        {
            PresentationSource _presentationSource = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);
            Matrix matix = _presentationSource.CompositionTarget.TransformToDevice;
            return new System.Windows.Size(SystemParameters.PrimaryScreenHeight * matix.M11, SystemParameters.PrimaryScreenWidth * matix.M22);
        }
        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!mouseup && grid.IsMouseCaptured)
            {
                Vector offset = System.Windows.Point.Subtract(e.GetPosition(this), m_start);
                if ((grid.PointToScreen(new System.Windows.Point(0, 0)).Y >= 0 || -offset.Y <= grid.Margin.Bottom - lastMargin.Bottom || offset.Y >= 0) && (grid.Margin.Bottom >= 0 || grid.Margin.Bottom - offset.Y >= 0 || offset.Y <= lastMargin.Bottom || offset.Y <= 0))
                {
                    grid.Margin = new Thickness(0, 0, 0, lastMargin.Bottom - offset.Y);
                }
            }
            e.Handled = true;
        }

        private void Grid_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.mouseup = true;
            lastMargin = grid.Margin;
            grid.ReleaseMouseCapture();
            e.Handled = true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Controls.Button tb in FindVisualChildren<System.Windows.Controls.Button>(this))
            {
                tb.Width = tb.Width * rescaleFactor;
                tb.Height = tb.Height * rescaleFactor;
                tb.FontSize = tb.FontSize * rescaleFactor;
            }
            foreach (System.Windows.Controls.Primitives.ToggleButton tb in FindVisualChildren<System.Windows.Controls.Primitives.ToggleButton>(this))
            {
                tb.Width = tb.Width * rescaleFactor;
                tb.Height = tb.Height * rescaleFactor;
                tb.FontSize = tb.FontSize * rescaleFactor;
            }
            foreach (Image tb in FindVisualChildren<Image>(this))
            {
                tb.Width = tb.Width * rescaleFactor;
            }
        }
    }
}
