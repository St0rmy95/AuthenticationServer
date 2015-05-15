using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using AuthenticationServer.Protocol;

namespace AuthServer
{
    public partial class Form1
    {
        public struct Connection
        {
            public NetworkStream Stream;
            public StreamWriter Streamwriter;
            public StreamReader Streamreader;
        }

        private static TcpListener m_tcpListener;
        private static List<Connection> m_lConnections;
        private static List<string> m_lsConnections;

        void Send(Connection con, int packet)
        {
            Send(con, packet.ToString());
        }

        void Send(Connection con, string value)
        {
            con.Streamwriter.WriteLine(Crypto.Encrypt(value, PACKET_KEY));
            con.Streamwriter.Flush();
        }

        void HandlePackets(Connection con, string adress)
        {
            try
            {
                while (con.Stream.CanRead)
                {
                    var readLine = Crypto.Decrypt(con.Streamreader.ReadLine(), PACKET_KEY);
                    if (m_bDebugMode == true)
                    {
                        this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Recv Protocol => " + readLine); });
                    }
                    string[] packet = readLine.Split(new Char[] { ' ' });
                    int type;
                    try
                    {
                        type = int.Parse(packet[0]);
                    }
                    catch
                    {
                        this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Invalid Header Type!"); });
                        type = (int)Opcodes.T_INVALID_PROTOCOL;
                    }

#if DEBUG
                    try
                    {
                        this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Packet Type => " + g_Protocol.GetPacketString(int.Parse(packet[0]))); });
                    }
                    catch
                    { }
#endif
                    switch (type)
                    {
                        case (int)Opcodes.T_AC_HANDSHAKE:
                            {
                                OnHandshake(con, packet, adress);
                                break;
                            }
                        case (int)Opcodes.T_AC_SEND_CLIENT_ID:
                            {
                                OnSendClient(con, packet, adress);
                                break;
                            }
                        case (int)Opcodes.T_AC_REQUEST_CLIENT_ID:
                            {
                                OnRequestClientID(con, packet, adress);
                                break;
                            }
                        case (int)Opcodes.T_AC_REQUEST_LAUNCHER_VERSION:
                            {
                                OnRequestLauncherVersion(con, packet, adress);
                                break;
                            }
                        case (int)Opcodes.T_AC_REQUEST_LAUNCHER_UPDATE_DATA:
                            {
                                OnRequestUpdateData(con, packet, adress);
                                break;
                            }
                        case (int)Opcodes.T_AC_REQUEST_SERVER_DATA:
                            {
                                OnRequestServerData(con, packet, adress);
                                break;
                            }
                        default:
                            {
                                //Connection close + packet dass invalid packet gesendet wurde
                                Send(con, (int)Opcodes.T_AC_SOCKET_CLOSE);
                                SocketClose(con, adress, "InvalidPacketType [" + type + "]");
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                //Closing connection (Security, UNKNOWN ERROR)
                try
                {
                    Send(con, (int)Opcodes.T_AC_SOCKET_CLOSE);
                    SocketClose(con, adress, ex.Message);
                }
                catch
                { }
                this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Socket disconnected [" + adress + "]"); });
                this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket disconnected (" + adress + "), (" + ex.Message + ")", m_sFilepath); });
                try
                {
                    m_lsConnections.Remove(adress);
                }
                catch
                {

                }
            }
            if(m_bDebugMode)
                this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Closing thread [" + adress + "]"); });
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Closing thread (" + adress + ")", m_sFilepath); });
        }


        void ConnectThread()
        {
            this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Opening Socket on: [" + g_Config.IP + ":" + g_Config.Port + "]"); });
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket open [" + g_Config.IP + ":" + g_Config.Port + "]", m_sFilepath); });
            try
            {
                m_tcpListener = new TcpListener(IPAddress.Any, g_Config.Port);
                m_tcpListener.Start();
                m_lConnections = new List<Connection>();
                m_lsConnections = new List<string>();
                while (true)
                {
                    var client = m_tcpListener.AcceptTcpClient();
                    this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Socket connected: " + client.Client.RemoteEndPoint); });
                    this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket connected(" + client.Client.RemoteEndPoint + ")", m_sFilepath); });
                    var c = new Connection { Stream = client.GetStream() };
                    c.Streamreader = new StreamReader(c.Stream);
                    c.Streamwriter = new StreamWriter(c.Stream);
                    m_lConnections.Add(c);
                    m_lsConnections.Add(client.Client.RemoteEndPoint.ToString());
                    var t = new Thread(() => HandlePackets(c, client.Client.RemoteEndPoint.ToString()));
                    t.Start();
                }
            }
            catch
            {
                this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Error! Connection error!"); });
            }
        }

        void SocketClose(Connection con, string adress, string error)
        {
            this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Socket closed: " + adress + " ==> " + error); });
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket closed (" + adress + ") ==> " + error, m_sFilepath); });
            m_lsConnections.Remove(adress);
            con.Stream.Close();
            con.Streamreader.Close();
            con.Streamwriter.Close();
        }
    }
}
