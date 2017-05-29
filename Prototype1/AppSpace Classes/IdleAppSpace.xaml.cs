using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Sockets;
using System.Net;
using NewMaster.Packets;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for IdleAppSpace.xaml
    /// </summary>
    public partial class IdleAppSpace : UserControl, IActiveInterfaceRequestEventProvider
    {
        private FrameworkElement currentMedia;

        private Uri nextMediaFile;

        private byte size;

        private byte position;

        //TODO:Centralize the location of available image and video extensions
        private static List<string> imageExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
        private static List<string> videoExtensions = new List<string> { ".mpeg", ".wmv", ".mp4" };
        private string nextMediaName;
        private string currentMediaName;
        private TcpListener updateListener;

        public IdleAppSpace(double width, double height)
        {
            InitializeComponent();

            Width = width;
            Height = height;

            updateListener = new TcpListener(IPAddress.Any, App.portC);
            Console.WriteLine("Listening...");
            updateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            updateListener.Start();
            updateListener.BeginAcceptSocket(new AsyncCallback(OnReceive), updateListener);

        }

        private void OnReceive(IAsyncResult asyn)
        {
            try
            {
                TcpListener listener = (TcpListener)asyn.AsyncState;
                byte[] buffer = new byte[500];
                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();
                replyStream.Read(buffer, 0, buffer.Length);
                replyStream.Dispose();
                client.Close();

                UCM_Packet packet;

                try
                {
                    packet = UCM_Packet.decode(buffer, (byte)App.monitorCount);
                    System.Diagnostics.Debug.WriteLine("got UCM");
                }
                catch (ArgumentException)
                {
                    ERR_Packet ERR = new ERR_Packet(0x04, "Invalid packet format");
                    updateListener.BeginAcceptSocket(new AsyncCallback(OnReceive), updateListener);
                    return;
                }

                beginTransition(packet);
                updateListener.BeginAcceptSocket(new AsyncCallback(OnReceive), updateListener);
            }
            catch(Exception e) { }
        }

        private void beginTransition(UCM_Packet packet)
        {
            try
            {
                size = packet.Size;
                position = packet.Position;
                string[] temp = packet.Filename.Split('\t');
                nextMediaName = temp[0];
                nextMediaFile = new Uri(AppDomain.CurrentDomain.BaseDirectory + "Resources\\IdleMedia\\Size" + size + "\\" + nextMediaName);

                currentMediaName = nextMediaName;
                if (currentMedia == null)
                {
                    FadeOut_Completed(this, null);
                }
                else
                {
                    if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this on the UI thread
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() => (FindResource("FadeOut") as Storyboard).Begin(currentMedia)));
                    }
                }

                if (currentMedia is MediaElement)
                {
                    mediaElement.Dispatcher.Invoke(new Action(() => mediaElement.Stop()));
                    mediaElement.Dispatcher.Invoke(new Action(() => mediaElement.Close()));

                    currentMedia = null;
                }
            }
            catch (Exception) { }
        }

        private void FadeOut_Completed(object sender, EventArgs e)
        {
            try
            {
                if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this on the UI thread
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => FadeOut_Completed(sender, e)));
                    return;
                }
                System.Diagnostics.Debug.WriteLine("FadeOut");
                Storyboard fadeIn = FindResource("FadeIn") as Storyboard;
                if (imageExtensions.Any(a => currentMediaName.ToLower().EndsWith(a)))
                {
                    System.Diagnostics.Debug.WriteLine("Image");
                    image.Visibility = Visibility.Visible;
                    mediaElement.Visibility = Visibility.Hidden;
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + "Resources\\IdleMedia\\Size" + size + "\\" + currentMediaName);

                    int Height = bmp.Height;
                    int Width = bmp.Width;
                    int bitmapFraction = Width / size;
                    int startingPoint = ((position - 1) * (Width / size));

                    //width of image that each screen will display
                    System.Drawing.Bitmap bmp1 = CropBitmap(bmp, startingPoint, 0, bitmapFraction, Height);

                    image.Source = CreateBitmapSourceFromGdiBitmap(bmp1);
                    currentMedia = image;
                }
                else if (videoExtensions.Any(a => currentMediaName.ToLower().EndsWith(a)))
                {
                    System.Diagnostics.Debug.WriteLine("Video");
                    image.Visibility = Visibility.Hidden;
                    mediaElement.Visibility = Visibility.Visible;
                    mediaElement.Source = nextMediaFile;
                    mediaElement.Pause();//need to call a function on the element in order to load the video

                    //need to wait until width/height is available in order to crop properly
                    while (mediaElement.NaturalVideoWidth == 0 || mediaElement.NaturalVideoHeight == 0) { }
                    int horizontalChunk = mediaElement.NaturalVideoWidth / size;

                    mediaElement.Clip = new RectangleGeometry(new Rect(horizontalChunk * (position - 1),
                                                                        0,
                                                                        horizontalChunk,
                                                                        mediaElement.NaturalVideoHeight));
                    mediaElement.Play();
                    currentMedia = mediaElement;
                }
                fadeIn.Begin(currentMedia);
            }
            catch (Exception) { }

        }

        public System.Drawing.Bitmap CropBitmap(System.Drawing.Bitmap bitmap,
                        int cropX, int cropY,
                        int cropWidth, int cropHeight)
        {
            try
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(cropX, cropY, cropWidth, cropHeight);
                return bitmap.Clone(rect, bitmap.PixelFormat);
            }
            catch (Exception) { return null; }
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(System.Drawing.Bitmap bitmap)
        {
            try
            {
                if (bitmap == null)
                    return null;

                var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

                var bitmapData = bitmap.LockBits(
                    rect,
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
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
            catch (Exception)
            {
                return null;
            }

        }

        private void FadeIn_Completed(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("FadeIn");
                if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this on the UI thread
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => FadeIn_Completed(sender, e)));
                    return;
                }
            }
            catch (Exception) { }
        }

        public void mouseClick()
        {
            try
            {
                if (!Application.Current.Dispatcher.CheckAccess())  //Must execute this on the UI thread
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => mouseClick()));
                    return;
                }
                System.Diagnostics.Debug.WriteLine("Mseclk");
                OnMouseDown(new MouseButtonEventArgs(Mouse.PrimaryDevice, new TimeSpan(DateTime.Now.Ticks).Milliseconds, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                    Source = Container
                });
            }
            catch (Exception) { }
        }

        /// <summary>
        /// If a user causes an event within the Interaction (ei. touch) then 
        /// request an active interaction to replace this idle interaction
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Msedwn");
                base.OnMouseDown(e);
                Point mousePoint = e.GetPosition(Application.Current.MainWindow);   //get position of mouse click in "global" reference
                ActiveInterfaceRequestEventArgs eventArgs = new ActiveInterfaceRequestEventArgs(mousePoint);
                close();
                if (RequestActiveInterface != null)
                {
                    RequestActiveInterface.BeginInvoke(this, eventArgs, null, null);
                }
                e.Handled = true;
            }
            catch (Exception) { }
        }

        public event EventHandler<ActiveInterfaceRequestEventArgs> RequestActiveInterface;


        public void close()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Close AA");
                mediaElement.Stop();
                mediaElement.Source = null;
                mediaElement.Close();
                currentMediaName = null;
                updateListener.Stop();
                updateListener.Server.Dispose();
            }
            catch (Exception) { }
        }
    }
}
