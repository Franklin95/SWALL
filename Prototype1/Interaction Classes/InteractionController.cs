using NewMaster;
using NewMaster.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;

namespace Prototype1
{
    /// <summary>
    /// Top level controller in the system to control the placement of interactions
    /// TODO: Need to make some of the methods critical sections to ensure Interactions are being created/destroyed 1 at a time
    /// </summary>
    class InteractionController
    {
        //Use 2 canvas, one for idle interactions and one for active interactions to have "3-layer" system
        //Use the _interactions list to simulate all interactions being side by side, but have seperate canvases
        //to ensure that Interaction Coaxer can be in between
        private Canvas _idleInteractionSpace;
        private Canvas _activeInteractionSpace;

        private bool _isActive = false;

        private bool _isIdle = false;
        private Client clientWindow;
        private IdleAppSpace idleInteraction;

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0010;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private Image backImage;
        private BackgroundWorker bw;
        private BackgroundWorker bw2;
        private TcpListener updateListener;

        private bool isActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        private bool isIdle
        {
            get { return _isIdle; }
            set { _isIdle = value; }
        }

        public InteractionController(Canvas idleInteractionSpace, Canvas activeInteractionSpace)
        {
            _idleInteractionSpace = idleInteractionSpace;
            _activeInteractionSpace = activeInteractionSpace;
            updateListener = new TcpListener(IPAddress.Any, App.portA);
            Console.WriteLine("Listening...");
            updateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            updateListener.Start();
            updateListener.BeginAcceptSocket(new AsyncCallback(OnReceive), updateListener);

        }

        private void OnReceive(IAsyncResult asyn)
        {
            byte[] buffer = new byte[3];
            try
            {
                TcpListener listener = (TcpListener)asyn.AsyncState;
                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();
                replyStream.Read(buffer, 0, buffer.Length);
                replyStream.Dispose();
                client.Close();
            }
            catch (Exception)
            {
                return;
            }


            CDI_Packet packet;

            try
            {
                packet = CDI_Packet.decode(buffer);
                System.Diagnostics.Debug.WriteLine("Got CDI");
            }
            catch (ArgumentException)
            {
                System.Diagnostics.Debug.WriteLine("Wrong CDI");
                // Didn't receive proper CDI, ignore packet
                updateListener.BeginAcceptSocket(new AsyncCallback(OnReceive), updateListener);
                return;
            }

            /// isActive    |   isIdle  |   Type    |   Mode    |   Result
            /// ----------------------------------------------------------------------------------------
            ///     0       |       0   |   Active  |   Create  |   Active interaction created
            ///     0       |       0   |   Active  |   Destroy |   Nothing, respond with ACK
            ///     0       |       0   |   Idle    |   Create  |   Idle Interaction Created
            ///     0       |       0   |   Idle    |   Destroy |   Nothing, respond with ACK
            ///     0       |       1   |   Active  |   Create  |   Active interaction created
            ///     0       |       1   |   Active  |   Destroy |   Error
            ///     0       |       1   |   Idle    |   Create  |   Nothing, respond with ACK
            ///     0       |       1   |   Idle    |   Destroy |   Idle Interaction Destroyed
            ///     1       |       0   |   Active  |   Create  |   Nothing, respond with ACK
            ///     1       |       0   |   Active  |   Destroy |   Active Interaction Destroyed
            ///     1       |       0   |   Idle    |   Create  |   Idle Interaction Created
            ///     1       |       0   |   Idle    |   Destroy |   Error



            //Cases where interaction is created
            if (!isActive && !isIdle &&
                packet.Type == CDI_Packet.CDI_Type.Active &&
                packet.Mode == CDI_Packet.CDI_Mode.Create)
            {
                createActiveInteraction();
            }
            else if (!isActive && !isIdle &&
                     packet.Type == CDI_Packet.CDI_Type.Idle &&
                     packet.Mode == CDI_Packet.CDI_Mode.Create)
            {
                createIdleInteraction();
            }
            else if (isActive &&
                packet.Type == CDI_Packet.CDI_Type.Idle &&
                packet.Mode == CDI_Packet.CDI_Mode.Create)
            {
                destroyCurrentInteraction();
                createIdleInteraction();
            }
            else if (isIdle &&
                     packet.Type == CDI_Packet.CDI_Type.Active &&
                     packet.Mode == CDI_Packet.CDI_Mode.Create)
            {
                destroyCurrentInteraction();
                createActiveInteraction();
            }

            //Cases where interaction is destroyed
            else if (isActive &&
                packet.Type == CDI_Packet.CDI_Type.Active &&
                packet.Mode == CDI_Packet.CDI_Mode.Destroy)
            {
                destroyCurrentInteraction();
            }
            else if (isIdle &&
                     packet.Type == CDI_Packet.CDI_Type.Idle &&
                     packet.Mode == CDI_Packet.CDI_Mode.Destroy)
            {
                destroyCurrentInteraction();
            }
            updateListener.BeginAcceptSocket(new AsyncCallback(OnReceive), updateListener);

        }

        private void destroyCurrentInteraction()
        {
            try
            {
                if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this on the UI thread
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => destroyCurrentInteraction()));
                    return;
                }
                if (isActive)
                {
                    ((ActiveAppSpace)_activeInteractionSpace.Children[1]).Close();
                    _activeInteractionSpace.Children.Clear();
                }
                else
                {
                    /*Start comment here if you want to get rid of the Interaction Coaxer*/
                    this.bw = new BackgroundWorker();
                    bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                    bw.RunWorkerAsync();
                    Console.Out.WriteLine("IdleSpace Destroyed: " + DateTime.Now);
                    /* Stop comment here*/
                    //((IdleAppSpace)_idleInteractionSpace.Children[0]).close();
                    _idleInteractionSpace.Children.Clear();
                    //beginReceiveFromMaster();
                }
            }
            catch (Exception) { }
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
        }
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (clientWindow != null)
                        {
                            clientWindow.Hide();
                            //clientWindow = null;
                        }
                    }));
                }
            }
            catch (Exception te) { }

        }
        void bw2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
        }
        void bw2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (clientWindow == null)
                        {
                            clientWindow = new Client(true);
                            IntPtr hWnd = new WindowInteropHelper(clientWindow).Handle;
                            clientWindow.Hiding += ClientWindow_Hidden;
                            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                            //clientWindow.Closed += ClientWindow_Closed;

                        }
                        clientWindow.Visibility = Visibility.Visible;
                    }));
                }
            }
            catch (Exception) { }
        }

        private void ClientWindow_Hidden(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Client Closed");
                //(sender as Client).Closed -= ClientWindow_Closed;
                idleInteraction.mouseClick();
            }
            catch (Exception) { }
        }

        public void closeClientWindow()
        {
            try
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => closeClientWindow()));
                }
                if (clientWindow != null)
                {
                    clientWindow.Close();
                    clientWindow = null;
                }
            }
            catch(Exception e) { }
        }

        private void createIdleInteraction()
        {
            try
            {
                if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this on the UI thread
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => createIdleInteraction()));
                    return;
                }
                Console.Out.WriteLine("IdleSpace Created: " + DateTime.Now);
                this.bw2 = new BackgroundWorker();
                bw2.DoWork += new DoWorkEventHandler(bw2_DoWork);
                bw2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw2_RunWorkerCompleted);
                bw2.RunWorkerAsync();
                //create the idle interaction and listen to the event of it requesting an Active Interaction
                idleInteraction = new IdleAppSpace(App.monitorWidth, App.monitorHeight);
                idleInteraction.RequestActiveInterface += RequestActiveInterface_Handler;
                _idleInteractionSpace.Children.Add(idleInteraction);
                /*Start comment here if you want to get rid of the Interaction Coaxer*/
                /*Start comment here if you want to get rid of the Interaction Coaxer*/

                /* Stop comment here*/
                _isIdle = true;
                _isActive = false;
            }
            catch (Exception ce) { }
        }

        private void createActiveInteraction()
        {
            try
            {
                if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this on the UI thread
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => createActiveInteraction()));
                    return;
                }
                Console.Out.WriteLine("ActiveSpace Created: " + DateTime.Now);
                backImage = new Image();
                backImage.BeginInit();
                backImage.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/MenuBackgroundDraft_v4.jpg"));
                backImage.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                backImage.Stretch = Stretch.Fill;
                backImage.EndInit();
                ActiveAppSpace activeInteraction = new ActiveAppSpace(App.monitorWidth, App.monitorHeight);
                activeInteraction.Closing += ActiveInteraction_Closing;
                _activeInteractionSpace.Children.Add(backImage);
                _activeInteractionSpace.Children.Add(activeInteraction);
                _isIdle = false;
                _isActive = true;
            }
            catch (Exception ce) { }
        }

        /// <summary>
        /// If user closes Active interaction, remove it and re-do interaction placement now that the monitor is no longer taken up
        /// by an Active Interaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActiveInteraction_Closing(object sender, EventArgs e)
        {

            try
            {
                //TODO: Tell master
                CDI_Packet packet = new CDI_Packet(CDI_Packet.CDI_Type.Idle, CDI_Packet.CDI_Mode.Create);

                TcpClient masterUpdateSocket = new TcpClient(App.masterIP, App.portA);
                masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                NetworkStream masterUpdate = masterUpdateSocket.GetStream();
                byte[] cdiData = packet.encode();
                byte[] data = new byte[1 + cdiData.Length];
                data[0] = App.monitorPosition;
                Array.Copy(cdiData, 0, data, 1, cdiData.Length);
                masterUpdate.Write(data, 0, data.Length);
                masterUpdate.Dispose();
                masterUpdateSocket.Close();
            }
            catch (Exception)
            {

            }

        }

        private void RequestActiveInterface_Handler(object sender, ActiveInterfaceRequestEventArgs e)
        {
            Console.Out.WriteLine(" Requesting ActiveSpace");
            //closeClientWindow();
            CDI_Packet packet = new CDI_Packet(CDI_Packet.CDI_Type.Active, CDI_Packet.CDI_Mode.Create);

            TcpClient masterUpdateSocket = new TcpClient(App.masterIP, App.portA);
            masterUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            NetworkStream masterUpdate = masterUpdateSocket.GetStream();
            byte[] cdiData = packet.encode();
            byte[] data = new byte[1 + cdiData.Length];
            data[0] = App.monitorPosition;
            Array.Copy(cdiData, 0, data, 1, cdiData.Length);
            masterUpdate.Write(data, 0, data.Length);
            masterUpdate.Dispose();
            masterUpdateSocket.Close();

        }
        public void Close()
        {
            destroyCurrentInteraction();
            updateListener.Stop();
            updateListener.Server.Dispose();
        }

    }
}

