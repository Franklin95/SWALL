using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.ComponentModel;
using InteractionCoaxer;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Net.Sockets;
using System.Net;
using NewMaster.Packets;
using System.Linq;
using System.Diagnostics;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        /// <summary>
        /// Amount of monitors
        /// </summary>
        public static int monitorCount;

        public static byte monitorPosition;

        public static string masterIP;
        public static string slave1IP;
        public static string slave2IP;
        public static string slave3IP;
        public static string slave4IP;

        public static int portA = 69;
        public static int portB = 70;
        public static int portC = 71;
        public static int portD = 72;

        /// <summary>
        /// In pixels
        /// This Assumes that all monitors have the same size/resolution
        /// </summary>
        public static double monitorHeight;

        /// <summary>
        /// In pixels
        /// This Assumes that all monitors have the same size/resolution
        /// </summary>
        public static double monitorWidth;

        private InteractionCoaxerController interactionCoaxerController;
        private BackgroundWorker worker;

        private InteractionController interactionController;

        private Canvas idleInteractionSpace;
        private Canvas activeInteractionSpace;
        private Canvas InteractionCoaxerCanvas;


        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0010;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);


        protected override void OnStartup(StartupEventArgs e)
        {
            //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            base.OnStartup(e);

            InitializeApp();
            this.Exit += App_Exit;

            MainWindow mainWindow = new MainWindow();
            //Client clientWindow = new Client();

            mainWindow.Width = monitorWidth;
            mainWindow.Height = monitorHeight;
            MainWindow = mainWindow;

            idleInteractionSpace = new Canvas();
            activeInteractionSpace = new Canvas();
            InteractionCoaxerCanvas = new Canvas();

            //Add all 3 layers to main window
            mainWindow.WorkingArea.Children.Add(idleInteractionSpace);
            mainWindow.WorkingArea.Children.Add(InteractionCoaxerCanvas);
            mainWindow.WorkingArea.Children.Add(activeInteractionSpace);

            ////////////////////FOR DEBUG////////////////////
            Button closeButton = new Button() { Content = "Close" };
            closeButton.Click += CloseButton_Click; ;
            mainWindow.WorkingArea.Children.Add(closeButton);
            Canvas.SetTop(closeButton, mainWindow.Height - 20);
            ////////////////////FOR DEBUG////////////////////

            //Set window to "always on bottom"
            //Warning: Other programs must still set themselves to "Always be on top"
            IntPtr hWnd = new WindowInteropHelper(mainWindow).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            //IntPtr hWnd1 = new WindowInteropHelper(clientWindow).Handle;
            //SetWindowPos(hWnd1, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            //Display the main window
            mainWindow.Show();
            //clientWindow.Show();
            interactionController = new InteractionController(idleInteractionSpace, activeInteractionSpace);
            registerWithMaster();

            //Run the Interaction Coaxer Controller on a different, background thread
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();

        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Console.Out.WriteLine("Exit Called");
            if (interactionController != null)
            {
                interactionController.closeClientWindow();
                interactionController.Close();
                interactionController = null;
            }
            /*if (MainWindow != null)
            {
                MainWindow.Close();
            }*/
            Dispose();
        }

        ////////////////////FOR DEBUG////////////////////
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Shutdown();
        }
        ////////////////////FOR DEBUG////////////////////

        public void Dispose()
        {
            worker.Dispose();
            GC.SuppressFinalize(this);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            interactionCoaxerController = new InteractionCoaxerController(InteractionCoaxerCanvas);
        }

        private void InitializeApp()
        {
            //Get the number of monitors hooked up in the system according to the App.config
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("MonitorCount"), out monitorCount)
                || monitorCount < 0)
            {
                throw new ConfigurationErrorsException("Invalid monitor count value set in App config");
            }

            if (!byte.TryParse(ConfigurationManager.AppSettings.Get("MonitorPosition"), out monitorPosition)
                || monitorPosition < 1 || monitorPosition > monitorCount)
            {
                throw new ConfigurationErrorsException("Invalid monitor position value set in App config");
            }

            //if (!long.TryParse(ConfigurationManager.AppSettings.Get("MasterIP"), out masterIP))
            //{
            //    throw new ConfigurationErrorsException("Invalid master IP value set in App config");
            //}
            masterIP = ConfigurationManager.AppSettings.Get("MasterIP");
            slave1IP = ConfigurationManager.AppSettings.Get("Slave1IP");
            slave2IP = ConfigurationManager.AppSettings.Get("Slave2IP");
            slave3IP = ConfigurationManager.AppSettings.Get("Slave3IP");
            slave4IP = ConfigurationManager.AppSettings.Get("Slave4IP");
            //Set the size of width and height for a single app space.
            //This is based on the size of the primary monitor size.
            //Therefore this assumes all monitors are the same size
            monitorWidth = SystemParameters.PrimaryScreenWidth;
            monitorHeight = SystemParameters.PrimaryScreenHeight;
        }

        /// <summary>
        /// Register this slave with the master. This function must complete before the program continues.
        /// </summary>
        private void registerWithMaster()
        {
            RSV_Packet packet = new RSV_Packet(monitorPosition);
            TcpClient masterUpdateSocket = new TcpClient(App.masterIP, App.portD);
            masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            NetworkStream masterUpdate = masterUpdateSocket.GetStream();
            masterUpdate.Write(packet.encode(), 0, packet.encode().Length);
            masterUpdate.Dispose();
            masterUpdateSocket.Close();
        }

    }
}
