using InteractionCoaxer.PoiCoaxer;
using InteractionCoaxer;
using Microsoft.Kinect;
using System;
using System.Timers;
using System.Windows.Controls;
using System.Windows;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using Prototype1;
using NewMaster;
using NewMaster.Packets;

namespace InteractionCoaxer
{
    /// <summary>
    /// Controls the and changes the current interaction coaxer in the system
    /// </summary>
    public class InteractionCoaxerController
    {
        /// <summary>
        /// The canvas the Coaxers will work on
        /// </summary>
        private Canvas workingCanvas;

        /// <summary>
        /// Current view for coaxer
        /// </summary>
        private CoaxerViewBase view;

        private Socket icSocket;

        private IPEndPoint endPointB;

        private ICoaxerModel model;

        public InteractionCoaxerController(Canvas canvas)
        {
            workingCanvas = canvas;

            icSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            icSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            icSocket.Bind(new IPEndPoint(
                (from p in hostEntry.AddressList where p.AddressFamily == AddressFamily.InterNetwork select p).First()  //get the IPv4 local host address
                , App.portB));

            IPAddress ipAddress = IPAddress.Parse(App.masterIP);
            endPointB = new IPEndPoint(ipAddress, App.portB);

            beginReceiveFromMaster();

        }

        private void beginReceiveFromMaster()
        {
            SocketBuffer sb = new SocketBuffer(icSocket);
            EndPoint ep = endPointB;
            icSocket.BeginReceiveFrom(sb.Buffer, 0, sb.Buffer.Length, SocketFlags.None, ref ep, new AsyncCallback(OnReceive), sb);
            //Console.Out.WriteLine("Waiting...");
        }

        private void OnReceive(IAsyncResult asyn)
        {
            SocketBuffer sb = (SocketBuffer)asyn.AsyncState;
            EndPoint ep = endPointB;

            int dataReceivedSize = sb.Socket.EndReceiveFrom(asyn, ref ep);

            // Copy data received into new buffer with size matching contents.
            byte[] buffer = new byte[dataReceivedSize];
            Array.Copy(sb.Buffer, buffer, dataReceivedSize);
            //Console.Out.WriteLine("Decoding SIC");
            if (PacketErrorChecker.validateSIC(buffer))
            {
                //System.Diagnostics.Debug.WriteLine("Got SIC");
                SIC_Packet packet = SIC_Packet.decode(buffer);

                //TODO:devize better way to specify views??
                switch (packet.Data.ToUpper())
                {
                    case "BIRD":
                        Application.Current.Dispatcher.Invoke(new Action(() => view = new BirdFollowingPoiCoaxerView(App.monitorCount, App.monitorPosition)));
                        break;
                    default:
                        ERR_Packet ERR = new ERR_Packet(0x03, "Invalid view name");
                        sb.Socket.SendTo(ERR.encode(), ep);
                        return;
                }

                model = view.getModel();
                model.AddObserver(view);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (workingCanvas.Children.Count != 0) { workingCanvas.Children.Clear(); }
                    workingCanvas.Children.Add(view);
                }));

                ACK_Packet ACK = new ACK_Packet(packet.Opcode);
                sb.Socket.SendTo(ACK.encode(), ep);

            }
            else if (PacketErrorChecker.validateUIC(buffer))
            {
                if (model == null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => view = new BirdFollowingPoiCoaxerView(App.monitorCount, App.monitorPosition)));
                    model = view.getModel();
                    model.AddObserver(view);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (workingCanvas.Children.Count != 0) { workingCanvas.Children.Clear(); }
                        workingCanvas.Children.Add(view);
                    }));
                }
                //System.Diagnostics.Debug.WriteLine("Got UIC");
                UIC_Packet packet = UIC_Packet.decode(buffer);
                if (model != null)
                {
                    model.decodeAndUpdate(packet.Data);
                }
            }
            else
            {
                ERR_Packet ERR = new ERR_Packet(0x03, "Invalid packet type for this port");
                sb.Socket.SendTo(ERR.encode(), ep);
            }

            beginReceiveFromMaster();

        }


    }
}
