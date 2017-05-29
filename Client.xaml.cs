using NewMaster.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZeroMQ;


namespace Prototype1
{
    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class Client : Window
    {
        private string masterIP = App.masterIP;
        private string slave1IP = App.slave1IP;
        private string slave2IP = App.slave2IP;
        private string slave3IP = App.slave3IP;
        private string slave4IP = App.slave4IP;
        private const double timerInterval = ((10.00 * 10.00) * 1) * 1; 
        private const double timer2Interval = ((10.00 * 10.00) * 1) * 1; 
        private System.Timers.Timer timer;
        private System.Timers.Timer timer2;
        //private Thread worker;
        private BackgroundWorker bw;
        private BackgroundWorker bw1;
        private BackgroundWorker bw2;
        private Byte[] colorPixels;
        private int colorWidth;
        private int colorHeight;
        private BitmapSource source;
        private BitmapSource source1;
        private ZContext slave1context;
        private ZContext mastercontext;
        private ZSocket slave1socket;
        private ZSocket mastersocket;
        private NetworkPublisher colorPublisher;
        private Socket statusSocket;
        private Boolean newData = false;
        private IPEndPoint ip;
        //private TcpListener listener;
        private bool closeRequested = false;
        private byte[] statusData;
        private bool first = false;
        private bool connectedToMaster = false;
        private bool connectedToSlave1 = false;
        private bool connectedToSlave2;
        private bool connectedToSlave3;
        private bool slave1Closed = true;
        private bool slave2Closed = true;
        private bool slave3Closed = true;
        private bool slave4Closed = true;
        private string connect_to_master;
        private string connect_to_slave3;
        private ZContext slave3context;
        private ZSocket slave3socket;
        private ZContext slave2context;
        private ZSocket slave2socket;
        private string connect_to_slave1;
        private string connect_to_slave2;
        private TimeSpan receiveTimeout;
        private int slaveNumber;
        private int slaveCount;
        private bool reconnection = false;
        private byte[] status;
        private TcpListener requestListener;
        private byte[] requestData;
        private Socket UpdateSocket;
        private IPEndPoint UpdateEndpoint;
        private int UpdatePort = 4000;
        private bool IsListening;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private string connect_to_slave4;
        private ZError error;

        //private ZFrame frame;
        //private Boolean first = true;
        public Client(bool first)
        {
            UpdateSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //UpdateSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            UpdateSocket.Bind(new IPEndPoint(
                (from p in hostEntry.AddressList where p.AddressFamily == AddressFamily.InterNetwork select p).First()  //get the IPv4 local host address
                , UpdatePort));
            UpdateEndpoint = new IPEndPoint(IPAddress.Any, UpdatePort);
            UpdateSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.first = first;
            this.requestData = new byte[4];
            this.colorPublisher = new NetworkPublisher();
            this.colorPublisher.SetConflate();
            this.colorPublisher.Bind("33408");
            this.status = new byte[2];
            this.slaveNumber = App.monitorPosition;
            this.slaveCount = App.monitorCount;
            this.receiveTimeout = new TimeSpan(5000000);
            this.statusData = new byte[2];
            this.colorWidth = Screen.PrimaryScreen.WorkingArea.Width;
            this.colorHeight = Screen.PrimaryScreen.WorkingArea.Height;
            connect_to_slave4 = "tcp://" + slave4IP + ":33408";
            connect_to_slave3 = "tcp://" + slave3IP + ":33408";
            connect_to_slave2 = "tcp://" + slave2IP + ":33408";
            connect_to_slave1 = "tcp://" + slave1IP + ":33408";
            connect_to_master = "tcp://" + masterIP + ":33408";
            //this.cm = new ClientModel();
            this.bw = new BackgroundWorker();
            //this.bw1 = new BackgroundWorker();
            this.bw2 = new BackgroundWorker();
            this.source = null;
            requestListener = new TcpListener(IPAddress.Any, 4001);
            Console.WriteLine("Listening...");
            requestListener.Start();
            Request();
            receiveMasterAck();
            //beginReceiveFromMaster();
            StartTransmission();

            if (slaveNumber == 4 && !slave3Closed)
            {
                this.slave3context = new ZContext();
                this.slave3socket = new ZSocket(this.slave3context, ZSocketType.SUB);
                this.slave3socket.SetOption(ZSocketOption.CONFLATE, 1);
                this.slave3socket.SetOption(ZSocketOption.SUBSCRIBE, "");
                this.slave3socket.ReceiveTimeout = receiveTimeout;
                Console.WriteLine("I: Connecting to {0}…", connect_to_slave3);
                slave3socket.Connect(connect_to_slave3);
                Console.WriteLine("Connected");
                Console.WriteLine("Connected1");
                slave3socket.Subscribe("");
                Console.WriteLine("Subscribed");
                this.connectedToSlave3 = true;
            }
            else if (slaveNumber == 3 && !slave2Closed)
            {


                this.slave2context = new ZContext();
                this.slave2socket = new ZSocket(this.slave2context, ZSocketType.SUB);
                this.slave2socket.SetOption(ZSocketOption.CONFLATE, 1);
                this.slave2socket.SetOption(ZSocketOption.SUBSCRIBE, "");
                this.slave2socket.ReceiveTimeout = receiveTimeout;
                Console.WriteLine("I: Connecting to {0}…", connect_to_slave2);
                slave2socket.Connect(connect_to_slave2);
                Console.WriteLine("Connected To Slave 2");
                slave2socket.Subscribe("");
                Console.WriteLine("Subscribed To Slave 2");
                this.connectedToSlave3 = false;
                this.connectedToMaster = false;
                this.connectedToSlave1 = false;
                this.connectedToSlave2 = true;
            }
            else if (slaveNumber == 2 && !slave1Closed)
            {


                this.slave1context = new ZContext();
                this.slave1socket = new ZSocket(this.slave1context, ZSocketType.SUB);
                this.slave1socket.SetOption(ZSocketOption.CONFLATE, 1);
                this.slave1socket.SetOption(ZSocketOption.SUBSCRIBE, "");
                this.slave1socket.ReceiveTimeout = receiveTimeout;
                Console.WriteLine("I: Connecting to {0}…", connect_to_slave1);
                slave1socket.Connect(connect_to_slave1);
                Console.WriteLine("Connected To Slave 1");
                slave1socket.Subscribe("");
                Console.WriteLine("Subscribed To Slave 1");
                this.connectedToSlave2 = false;
                this.connectedToSlave3 = false;
                this.connectedToMaster = false;
                this.connectedToSlave1 = true;
            }

            else if (slaveNumber == 1)
            {
                this.connectedToSlave1 = false;
                this.connectedToSlave2 = false;
                this.connectedToSlave3 = false;
                this.mastercontext = new ZContext();
                this.mastersocket = new ZSocket(this.mastercontext, ZSocketType.SUB);
                this.mastersocket.SetOption(ZSocketOption.CONFLATE, 1);
                this.mastersocket.SetOption(ZSocketOption.RCVTIMEO, 1000);
                this.mastersocket.SetOption(ZSocketOption.SUBSCRIBE, "");
                this.mastersocket.ReceiveTimeout = receiveTimeout;
                Console.WriteLine("I: Connecting to {0}…", connect_to_master);
                mastersocket.Connect(connect_to_master);
                Console.WriteLine("Connected To Master");
                mastersocket.Subscribe("");
                Console.WriteLine("Subscribed To Master");
                this.connectedToMaster = true;
            }
            InitializeComponent();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();

            /*bw1 = new BackgroundWorker();
            bw1.DoWork += new DoWorkEventHandler(bw1_DoWork);
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            //bw.WorkerReportsProgress = true;
            bw1.WorkerSupportsCancellation = true;
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw1_RunWorkerCompleted);
            bw1.RunWorkerAsync();*/


            timer = new System.Timers.Timer();
            timer.Interval = timerInterval;
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            //Timer_Elapsed(null, null);
            /*bw1.DoWork += new DoWorkEventHandler(bw1_DoWork);
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw1_RunWorkerCompleted);
            bw1.RunWorkerAsync();*/

            bw2.DoWork += new DoWorkEventHandler(bw2_DoWork);
            bw2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw2_RunWorkerCompleted);
            bw2.RunWorkerAsync();
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bw1 = new BackgroundWorker();
            bw1.DoWork += new DoWorkEventHandler(bw1_DoWork);
            bw1.WorkerSupportsCancellation = true;
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw1_RunWorkerCompleted);
            bw1.RunWorkerAsync();


        }

        public void Receive()
        {
            ZFrame frame = new ZFrame();
            if (!closeRequested)
            {
                System.Windows.Media.PixelFormat format = PixelFormats.Bgr32;
                try
                {
                    if (!slave3Closed && connectedToSlave3 && slaveNumber == 4)
                    {
                        this.connectedToSlave1 = false;
                        this.connectedToMaster = false;
                        this.connectedToSlave2 = false;
                        this.connectedToSlave3 = true;
                        Console.WriteLine("Trying To Receive Frame From Slave 3");
                        frame = slave3socket.ReceiveFrame();
                        Console.WriteLine("Received Frame From Slave 3");
                    }
                    else if (!slave2Closed && connectedToSlave2 && slaveNumber == 3)
                    {
                        this.connectedToSlave1 = false;
                        this.connectedToMaster = false;
                        this.connectedToSlave3 = false;
                        this.connectedToSlave2 = true;
                        Console.WriteLine("Trying To Receive Frame From Slave 2");
                        frame = slave2socket.ReceiveFrame();
                        Console.WriteLine("Received Frame From Slave 2");
                    }
                    else if (!slave1Closed && connectedToSlave1 && slaveNumber == 2)
                    {
                        this.connectedToSlave2 = false;
                        this.connectedToMaster = false;
                        this.connectedToSlave3 = false;
                        this.connectedToSlave1 = true;
                        Console.WriteLine("Trying To Receive Frame From Slave 1");
                        frame = slave1socket.ReceiveFrame();
                        Console.WriteLine("Received Frame From Slave 1");
                    }
                    else if (slaveNumber == 1 && connectedToMaster)
                    {
                        this.connectedToSlave2 = false;
                        this.connectedToSlave1 = false;
                        this.connectedToSlave3 = false;
                        this.connectedToMaster = true;
                        //Console.WriteLine("Trying To Receive Frame From Master");
                        frame = mastersocket.ReceiveFrame(out error);
                        if(error == ZError.EAGAIN)
                        {
                            System.Diagnostics.Debug.WriteLine("ERROR");
                            return;
                        }
                        //Console.WriteLine("Received Frame From Master");
                    }
                    else
                    {
                        if (first)
                        {
                            if (!slave3Closed && (slaveNumber - 1) >= 3)
                            {
                                Disconnect();
                                Console.WriteLine("Connecting to Slave 3");
                                this.slave3context = new ZContext();
                                this.slave3socket = new ZSocket(this.slave3context, ZSocketType.SUB);
                                this.slave3socket.SetOption(ZSocketOption.CONFLATE, 1);
                                this.slave3socket.SetOption(ZSocketOption.SUBSCRIBE, "");
                                this.slave3socket.ReceiveTimeout = receiveTimeout;
                                Console.WriteLine("I: Connecting to {0}…", connect_to_slave3);
                                slave3socket.Connect(connect_to_slave3);
                                Console.WriteLine("Connected To Slave 3");
                                slave3socket.Subscribe("");
                                Console.WriteLine("Subscribed To Slave 3");
                                this.connectedToSlave2 = false;
                                this.connectedToSlave1 = false;
                                this.connectedToMaster = false;
                                this.connectedToSlave3 = true;
                            }
                            else if (!slave2Closed && (slaveNumber - 1) >= 2)
                            {
                                Disconnect();
                                Console.WriteLine("Connecting to Slave 2");
                                this.slave2context = new ZContext();
                                this.slave2socket = new ZSocket(this.slave2context, ZSocketType.SUB);
                                this.slave2socket.SetOption(ZSocketOption.CONFLATE, 1);
                                this.slave2socket.SetOption(ZSocketOption.SUBSCRIBE, "");
                                this.slave2socket.ReceiveTimeout = receiveTimeout;
                                Console.WriteLine("I: Connecting to {0}…", connect_to_slave2);
                                slave2socket.Connect(connect_to_slave2);
                                Console.WriteLine("Connected To Slave 2");
                                slave2socket.Subscribe("");
                                Console.WriteLine("Subscribed To Slave 2");
                                this.connectedToSlave3 = false;
                                this.connectedToMaster = false;
                                this.connectedToSlave1 = false;
                                this.connectedToSlave2 = true;
                            }
                            else if (!slave1Closed && (slaveNumber - 1) >= 1)
                            {
                                Disconnect();
                                Console.WriteLine("Connecting to Slave 1");
                                this.slave1context = new ZContext();
                                this.slave1socket = new ZSocket(this.slave1context, ZSocketType.SUB);
                                this.slave1socket.SetOption(ZSocketOption.CONFLATE, 1);
                                this.slave1socket.SetOption(ZSocketOption.SUBSCRIBE, "");
                                this.slave1socket.ReceiveTimeout = receiveTimeout;
                                Console.WriteLine("I: Connecting to {0}…", connect_to_slave1);
                                slave1socket.Connect(connect_to_slave1);
                                Console.WriteLine("Connected To Slave 1");
                                slave1socket.Subscribe("");
                                Console.WriteLine("Subscribed To Slave 1");
                                this.connectedToSlave2 = false;
                                this.connectedToSlave3 = false;
                                this.connectedToMaster = false;
                                this.connectedToSlave1 = true;
                            }

                            else
                            {
                                Disconnect();
                                Console.WriteLine("Connecting to Master");
                                this.connectedToSlave1 = false;
                                this.connectedToSlave2 = false;
                                this.connectedToSlave3 = false;
                                this.mastercontext = new ZContext();
                                this.mastersocket = new ZSocket(this.mastercontext, ZSocketType.SUB);
                                this.mastersocket.SetOption(ZSocketOption.CONFLATE, 1);
                                this.mastersocket.SetOption(ZSocketOption.RCVTIMEO, 1000);
                                this.mastersocket.SetOption(ZSocketOption.SUBSCRIBE, "");
                                this.mastersocket.ReceiveTimeout = receiveTimeout;
                                Console.WriteLine("I: Connecting to {0}…", connect_to_master);
                                mastersocket.Connect(connect_to_master);
                                Console.WriteLine("Connected To Master");
                                mastersocket.Subscribe("");
                                Console.WriteLine("Subscribed To Master");
                                this.connectedToMaster = true;
                            }
                        }
                        first = false;
                        if (connectedToMaster)
                        {
                            //Console.WriteLine("Trying To Receive Frame From Master");
                            frame = mastersocket.ReceiveFrame(out error);
                            if (error == ZError.EAGAIN)
                            {
                                System.Diagnostics.Debug.WriteLine("ERROR: NOTHING");
                                return;
                            }
                        }
                        if (connectedToSlave1)
                        {
                            Console.WriteLine("Trying To Receive Frame From Slave 1");
                            frame = slave1socket.ReceiveFrame();
                            Console.WriteLine("Received Frame From Slave 1");
                        }
                        if (connectedToSlave2)
                        {
                            Console.WriteLine("Trying To Receive Frame From Slave 2");
                            frame = slave2socket.ReceiveFrame();
                            Console.WriteLine("Received Frame From Slave 2");
                        }
                        if (connectedToSlave3)
                        {
                            Console.WriteLine("Trying To Receive Frame From Slave 2");
                            frame = slave3socket.ReceiveFrame();
                            Console.WriteLine("Received Frame From Slave 2");
                        }
                    }
                    if (frame != null)
                    {
                        colorPixels = ReadAllBytes(frame);
                        newData = true;
                        colorPublisher.SendByteArray(colorPixels);
                        GC.Collect();
                        //colorPixels = temp;
                    }
                    else
                    {
                        Console.WriteLine("Null Frame");
                        newData = false;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Error Frame" + e.Message);
                    newData = false;
                    return;
                }

            }
        }
        public static byte[] ReadAllBytes(ZFrame frame)
        {
            const int bufferSize = 4096000;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = frame.Read(buffer, 0, buffer.Length)) != 0)
                {
                    ms.Write(buffer, 0, count);
                }
                //Console.Out.WriteLine("Returning Bytes");
                var data = ms.ToArray();
                ms.Dispose();
                return data;
            }

        }

        public void ReceiveData()
        {

            if (newData && colorPixels.Length != 0)
            {
                byte[] temp = colorPixels;
                newData = false;
                try
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            //Console.Out.WriteLine("Got Bytes" + temp.ToArray());
                            //Console.Out.WriteLine("Byte Size" + temp.Length);
                            Stream imageStreamSource = new MemoryStream(temp);

                            PngBitmapDecoder decoder = new
                            PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                            source1 = decoder.Frames[0];

                            Bitmap bmp = GetBitmap(source1);
                            if (bmp == null) { return; }
                            int Height = bmp.Height;
                            int Width = bmp.Width;
                            int bitmapFraction = Width / slaveCount;
                            int startingPoint = ((slaveNumber - 1) * (Width / slaveCount));
                            if (bmp != null)
                            {
                                Bitmap bmp1 = CropBitmap(bmp, startingPoint, 0, bitmapFraction, Height);
                                source = CreateBitmapSourceFromGdiBitmap(bmp1);

                                camera.Source = source;
                                //newData = false;
                                //doSomethingWithBitmapFast(bmp);
                                //return;
                                //Console.Out.WriteLine("Image Shown");
                                imageStreamSource.Dispose();
                                bmp1.Dispose();
                                bmp.Dispose();
                            }
                            //doSomethingWithBitmapSlow(bmp);
                            //bmp.MakeTransparent(System.Drawing.Color.Black);
                            //Bitmap bmp1 = RemoveBack(bmp);

                        }));
                    }
                    else
                    {
                        //Console.Out.WriteLine("Got Bytes" + temp.ToArray());
                        //Console.Out.WriteLine("Byte Size" + temp.Length);
                        Stream imageStreamSource = new MemoryStream(temp);

                        PngBitmapDecoder decoder = new
                        PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                        source1 = decoder.Frames[0];

                        Bitmap bmp = GetBitmap(source1);
                        if (bmp == null) { return; }
                        int Height = bmp.Height;
                        int Width = bmp.Width;
                        int bitmapFraction = Width / slaveCount;
                        int startingPoint = ((slaveNumber - 1) * (Width / slaveCount));
                        if (bmp != null)
                        {
                            Bitmap bmp1 = CropBitmap(bmp, startingPoint, 0, bitmapFraction, Height);
                            source = CreateBitmapSourceFromGdiBitmap(bmp1);

                            camera.Source = source;
                            //newData = false;
                            //doSomethingWithBitmapFast(bmp);
                            //return;
                            //Console.Out.WriteLine("Image Shown");
                            imageStreamSource.Dispose();
                            bmp1.Dispose();
                            bmp.Dispose();
                        }
                    }
                }
                catch (ArgumentException ae)
                {
                    GC.Collect();
                    Console.Error.WriteAsync("Error" + ae.Message);
                    //source = null;
                    //return;
                }
                temp = null;
            }
            else
            {
                GC.Collect();
                //Console.Out.Write("Nothing");
                //source = null;
                return;
            }
            //}
            GC.Collect();
        }

        public Bitmap CropBitmap(Bitmap bitmap,
                         int cropX, int cropY,
                         int cropWidth, int cropHeight)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(cropX, cropY, cropWidth, cropHeight);
            return bitmap.Clone(rect, bitmap.PixelFormat);
        }
        Bitmap GetBitmap(BitmapSource source)
        {
            try
            {
                Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride); bmp.UnlockBits(data);
                return bmp;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
                bitmap.Dispose();
            }
        }


        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((sender as BackgroundWorker).CancellationPending)
            {
                (sender as BackgroundWorker).Dispose();
            }
            else
            {
                (sender as BackgroundWorker).RunWorkerAsync();
            }
        }
        void bw1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
        }
        void bw2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                Receive();
            }
            catch (Exception te) { }

        }

        void bw1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (this.IsVisible)
                {
                    ReceiveData();
                }
            }
            catch (Exception te) { }
        }


        void bw2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                beginReceiveFromMaster();
            }
            catch (Exception te) { }
        }
        private void beginReceiveFromMaster()
        {
            SocketBuffer sb = new SocketBuffer(UpdateSocket);
            EndPoint ep = UpdateEndpoint;
            UpdateSocket.BeginReceiveFrom(sb.Buffer, 0, sb.Buffer.Length, SocketFlags.None, ref ep, new AsyncCallback(OnReceive), sb);

        }
        private void OnReceive(IAsyncResult asyn)
        {
            allDone.Set();
            SocketBuffer sb = (SocketBuffer)asyn.AsyncState;
            EndPoint ep = UpdateEndpoint;

            int dataReceivedSize;

            try
            {
                dataReceivedSize = sb.Socket.EndReceiveFrom(asyn, ref ep);
            }
            catch (ObjectDisposedException)
            {
                return; //RETURN IF CANVAS IS CLOSED    
            }


            // Copy data received into new buffer with size matching contents.
            Array.Copy(sb.Buffer, statusData, dataReceivedSize);
            Console.Out.WriteLine("A Slave has quit");
            if (statusData[0] == (byte)0)
            {
                if (statusData[1] == (byte)1)
                {
                    this.first = true;
                    this.slave1Closed = true;
                    Console.Out.WriteLine("Slave 1 has quit");
                }
                else if (statusData[1] == (byte)2)
                {
                    this.first = true;
                    this.slave2Closed = true;
                    Console.Out.WriteLine("Slave 2 has quit");
                }
                else if (statusData[1] == (byte)3)
                {
                    this.first = true;
                    this.slave3Closed = true;
                    Console.Out.WriteLine("Slave 3 has quit");
                }
                else if (statusData[1] == (byte)4)
                {
                    this.first = true;
                    this.slave4Closed = true;
                    Console.Out.WriteLine("Slave 4 has quit");
                }
            }
            else if (statusData[0] == (byte)1)
            {
                if (statusData[1] == (byte)1)
                {
                    this.first = true;
                    this.slave1Closed = false;
                    Console.Out.WriteLine("Slave 1 has resumed");
                }
                else if (statusData[1] == (byte)2)
                {
                    this.first = true;
                    this.slave2Closed = false;
                    Console.Out.WriteLine("Slave 2 has resumed");
                }
                else if (statusData[1] == (byte)3)
                {
                    this.first = true;
                    this.slave3Closed = false;
                    Console.Out.WriteLine("Slave 3 has resumed");
                }
                else if (statusData[1] == (byte)4)
                {
                    this.first = true;
                    this.slave4Closed = false;
                    Console.Out.WriteLine("Slave 4 has resumed");
                }
            }
        }
        public void Request()
        {
            try
            {
                this.closeRequested = false;
                status[0] = (byte)1;
                status[1] = (byte)slaveNumber;
                TcpClient masterResponseSocket = new TcpClient(masterIP, 4000);
                NetworkStream masterResponse = masterResponseSocket.GetStream();

                //---send the text---
                Console.WriteLine("Sending Reconnection Message to Server");
                masterResponse.Write(status, 0, status.Length);
                masterResponse.Dispose();
                masterResponseSocket.Close();
            }
            catch { }
        }
        private void StartTransmission()
        {
            using (Socket openingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                openingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                if (!slave1Closed && slaveNumber != 1)
                {
                    var slave1ep = new IPEndPoint(IPAddress.Parse(slave1IP), UpdatePort);
                    openingSocket.SendTo(status, slave1ep);
                }
                if (!slave2Closed && slaveNumber != 2)
                {
                    var slave2ep = new IPEndPoint(IPAddress.Parse(slave2IP), UpdatePort);
                    openingSocket.SendTo(status, slave2ep);
                }
                if (!slave3Closed && slaveNumber != 3)
                {
                    var slave3ep = new IPEndPoint(IPAddress.Parse(slave3IP), UpdatePort);
                    openingSocket.SendTo(status, slave3ep);
                }
                if (!slave4Closed && slaveNumber != 4)
                {
                    var slave4ep = new IPEndPoint(IPAddress.Parse(slave4IP), UpdatePort);
                    openingSocket.SendTo(status, slave4ep);
                }
            }
        }

        public void receiveMasterAck()
        {
            Console.WriteLine("Waiting for Master's ACK");
            TcpClient request = requestListener.AcceptTcpClient();
            Console.WriteLine("Got Master's ACK");
            //---get the incoming data through a network stream---
            NetworkStream requestStream = request.GetStream();

            //---read incoming stream---
            int bytesRead = requestStream.Read(requestData, 0, 4);
            if (requestData[0] == (byte)1)
            {
                this.first = true;
                this.slave1Closed = false;
                Console.Out.WriteLine("Slave 1 is Active");
            }
            if (requestData[1] == (byte)1)
            {
                this.first = true;
                this.slave2Closed = false;
                Console.Out.WriteLine("Slave 2 is Active");
            }
            if (requestData[2] == (byte)1)
            {
                this.first = true;
                this.slave3Closed = false;
                Console.Out.WriteLine("Slave 3 is Active");
            }
            if (requestData[3] == (byte)1)
            {
                this.first = true;
                this.slave4Closed = false;
                Console.Out.WriteLine("Slave 4 is Active");
            }
            if (requestData[0] == (byte)0)
            {
                this.first = true;
                this.slave1Closed = true;
                Console.Out.WriteLine("Slave 1 is Inactive");
            }
            if (requestData[1] == (byte)0)
            {
                this.first = true;
                this.slave2Closed = true;
                Console.Out.WriteLine("Slave 2 is Inactive");
            }
            if (requestData[2] == (byte)0)
            {
                this.first = true;
                this.slave3Closed = true;
                Console.Out.WriteLine("Slave 3 is Inactive");
            }
            if (requestData[3] == (byte)0)
            {
                this.first = true;
                this.slave4Closed = true;
                Console.Out.WriteLine("Slave 4 is Inactive");
            }
            requestStream.Dispose();
            request.Close();
            requestListener.Server.Dispose();
        }
        public void Disconnect()
        {
            if (connectedToMaster)
            {
                this.mastersocket.Unsubscribe("");
                this.mastersocket.Disconnect(connect_to_master);
                this.mastersocket.Close();
                this.mastersocket.Dispose();
                //this.mastercontext.Dispose();
                connectedToMaster = false;
            }
            else if (connectedToSlave1)
            {
                this.slave1socket.Unsubscribe("");
                this.slave1socket.Disconnect(connect_to_slave1);
                this.slave1socket.Dispose();
                this.slave1context.Dispose();
                connectedToSlave1 = false;
            }
            else if (connectedToSlave2)
            {
                this.slave2socket.Unsubscribe("");
                this.slave2socket.Disconnect(connect_to_slave2);
                this.slave2socket.Dispose();
                this.slave2context.Dispose();
                connectedToSlave2 = false;
            }
            else if (connectedToSlave3)
            {
                this.slave3socket.Unsubscribe("");
                this.slave3socket.Disconnect(connect_to_slave3);
                this.slave3socket.Dispose();
                this.slave3context.Dispose();
                connectedToSlave3 = false;
            }
        }
        public event EventHandler Hiding;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            //firstProc.Close();

            base.OnMouseDown(e);
            Hiding.BeginInvoke(this, new EventArgs(), null, null);
            this.Hide();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.closeRequested = true;
            /*timer.Enabled = false;
            timer.Dispose();
            timer = null;*/
            status[0] = (byte)0;
            status[1] = (byte)slaveNumber;
            bw.CancelAsync();
            //bw.Dispose();
            using (Socket closingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                Console.WriteLine("Sending Closing Message to Server");
                var masterep = new IPEndPoint(IPAddress.Parse(masterIP), 4001);
                closingSocket.SendTo(status, masterep);
                if (!slave1Closed && slaveNumber != 1)
                {
                    var slave1ep = new IPEndPoint(IPAddress.Parse(slave1IP), UpdatePort);
                    closingSocket.SendTo(status, slave1ep);
                }
                if (!slave2Closed && slaveNumber != 2)
                {
                    var slave2ep = new IPEndPoint(IPAddress.Parse(slave2IP), UpdatePort);
                    closingSocket.SendTo(status, slave2ep);
                }
                if (!slave3Closed && slaveNumber != 3)
                {
                    var slave3ep = new IPEndPoint(IPAddress.Parse(slave3IP), UpdatePort);
                    closingSocket.SendTo(status, slave3ep);
                }
                if (!slave4Closed && slaveNumber != 4)
                {
                    var slave4ep = new IPEndPoint(IPAddress.Parse(slave4IP), UpdatePort);
                    closingSocket.SendTo(status, slave4ep);
                }
                Disconnect();
                Console.WriteLine("Disconnected");
            }
            this.IsListening = false;
            bw1.CancelAsync();
            bw1.Dispose();
            Console.WriteLine("BW1 Disposed");
            //listener.Server.Dispose();
            //Console.WriteLine("Listener Stopped");
            bw2.Dispose();
            colorPublisher.Close();
            Console.WriteLine("BW2 Disposed");
            UpdateSocket.Close();
            UpdateSocket.Dispose();
            UpdateSocket = null;
            colorPixels = null;


        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            GC.Collect();
        }
    }

}
