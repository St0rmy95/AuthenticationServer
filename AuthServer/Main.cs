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

//External Classes
using IniReaderLib;
using AuthenticationServer.Config;
using AuthenticationServer.Database;
using AuthenticationServer.Log;
using AuthenticationServer.Protocol;

namespace AuthServer
{
    public partial class Form1 : Form
    {
        bool DebugMode;
        // Get DateTime for Log file name
        public static string filepath = "..\\..\\Logs\\AuthSystem_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".log";
        
        private static TcpListener _server;
        private static List<Connection> _list;
        private static List<string> _connections;

        private struct Connection
        {
            public NetworkStream Stream;
            public StreamWriter Streamw;
            public StreamReader Streamr;
        }

        //Importing external Classes
        Config g_Config = new Config();
        SQL g_Sql = new SQL();
        Log g_Log = new Log();
        Protocol g_Protocol = new Protocol();

        private void ConsoleWriteLine(string text) 
        {
            try
            {
                richTextBox1.AppendText(text + "\n");
                richTextBox1.ScrollToCaret();
            }
            catch
            {

            }
        }
        public Form1()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void main(object sender, EventArgs e)
        {
            if (timer1.Enabled == true)
            {
                timer1.Stop();
            }

#if DEBUG
            ConsoleWriteLine("############################ Combat-Rivals AuthenticationServer #################################");
            this.Text = "AuthenticationServer (Debug x86)";
            DebugMode = true;
#else
			ConsoleWriteLine("############################ Combat-Rivals AuthenticationServer #################################");
            this.Text = "AuthenticationServer (Release x86)";
            DebugMode = false;
#endif
			ConsoleWriteLine("Using Logfile \"" + filepath + "\"");
            ConsoleWriteLine("Reading Configuration...");
            g_Log.WriteLog("Loading Configuration", filepath);

            try
            {
                g_Config.ReadConfig();
            }
            catch
            {
                ConsoleWriteLine("Configuration Error!");
                g_Log.WriteLog("Config Error (0)", filepath);
                Environment.Exit(0);
            }

            try
            {
                WebClient versionread = new WebClient();
                versionread.Proxy = null;
                g_Config.LVersion = versionread.DownloadString(g_Config.LVersionURL);
            }
            catch
            {
                ConsoleWriteLine("LauncherVerison not found, setting Version to: 0.0.0.0");
                g_Config.LVersion = "0.0.0.0";
            }

            ConsoleWriteLine("Connecting to Database...");
            g_Log.WriteLog("Connecting to DB", filepath);
            try
            {
                g_Sql.SQLConnect(g_Config.sqlIp, g_Config.sqlLogin, g_Config.sqlPwd);
            }
            catch
            {
                ConsoleWriteLine("Database connection failed");
                g_Log.WriteLog("Database connection failed", filepath);
                Environment.Exit(0);
            }

            Thread thread = new Thread(ConnectThread);
            thread.Start();
        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string input;
                {
                    input = richTextBox2.Text;
                    richTextBox2.Text = "";
                    richTextBox2.Focus();
                    SendKeys.Send("{BACKSPACE}");
                    if (input != "")
                    {
                        ConsoleWriteLine(">> " + input);
                        string[] args = input.Split(new Char[] { '.', ' ' });
                        //Command parser
                        if (args[0].ToLower() == "server")
                        {
                            if (args[1].ToLower() == "stop")
                            {
                                StopServer();
                            }
                            else
                            {
                                ConsoleWriteLine("No such subcommand!");
                            }
                        }
                        else if (args[0].ToLower() == "client")
                        {
                            if (args[1].ToLower() == "block")
                            {
								if (g_Sql.HasRows("SELECT ClientID FROM atum2_db_account.dbo.td_AuthBan WHERE ClientID = '" + args[2] + "'"))
								{
									g_Sql.Execute("UPDATE atum2_db_account.dbo.td_AuthBan SET Banned = 1, BanDate = GETDATE() WHERE ClientID = '" + args[2] + "'");
								}
								else
								{
									g_Sql.Execute("INSERT INTO atum2_db_account.dbo.td_AuthBan (ClientID, Banned, BanDate) VALUES ('" + args[2] + "', 1, GETDATE())");
								}
								ConsoleWriteLine("Client banned: " + args[2]);
								g_Log.WriteLog("Client banned (" + args[2] + ")", filepath);
                            }
							else if (args[1].ToLower() == "unblock")
							{
								if (g_Sql.HasRows("SELECT ClientID FROM atum2_db_account.dbo.td_AuthBan  WHERE ClientID = '" + args[2] + "'"))
								{
									g_Sql.Execute("UPDATE atum2_db_account.dbo.td_AuthBan SET Banned = 0 WHERE ClientID = '" + args[2] + "'");
									ConsoleWriteLine("Client unbanned: " + args[2]);
									g_Log.WriteLog("Client unbanned (" + args[2] + ")", filepath);
								}
								else
								{
									ConsoleWriteLine("Client \"" + args[2] + "\" is not blocked");
								}
							}
							else
							{
								ConsoleWriteLine("No such subcommand!");
							}
                        }
                        else if (args[0].ToLower() == "help" || args[0] == "?")
                        {
                            Help();
                        }
                        else if (args[0].ToLower() == "list")
                        {
                            int i = 0;
                            ConsoleWriteLine("All active Connections:");
                            foreach (var connection in _connections)
                            {
                                ConsoleWriteLine("-" + connection.ToString());
                                i++;
                            }
                            if (i == 0)
                            {
                                ConsoleWriteLine("No active Connections!");
                            }
                        }
                        else
                        {
                            ConsoleWriteLine("No such command!");
                        }
                    }
                }
            }
        }

        public void Help()
        {
            ConsoleWriteLine("");
            ConsoleWriteLine("-? or help = Shows all commands");
            ConsoleWriteLine("-server.stop = Stops the Server");
            ConsoleWriteLine("-client.block <ClientID> = Blocks the specific ClientID");
            ConsoleWriteLine("-client.unblock <ClientID>= Unblocks the specific ClientID");
            ConsoleWriteLine("-list = Lists all active connections");
            ConsoleWriteLine("");
        }

        public void StopServer()
        {
            ConsoleWriteLine("Closing Server...");
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Console: Shutdown", filepath); });
            ConsoleWriteLine("Server closed!");
            g_Sql.SQLDisconnect();
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Closing DBCon", filepath); });
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Shutting down Server", filepath); });
            Environment.Exit(0);
        }


        public void Version()
        {
            ConsoleWriteLine("Combat-Rivals Authentication Server\nCopyright by St0rmy");
        }

        void HandlePackets(Connection con, string adress)
        {
            try
            {
                while (true)
                {
                    var readLine = con.Streamr.ReadLine();
                    if (DebugMode == true)
                    {
                        this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Recv Protocol => " + readLine); });
                    }
                    string[] split = readLine.Split(new Char[] {' '});

#if DEBUG
                    try
                    {
                        this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Packet Type => " + g_Protocol.GetPacketString(int.Parse(split[0]))); });
                    }
                    catch
                    { }
#endif

                    if(split[0].ToString() == ((int)Opcodes.T_AC_HANDSHAKE).ToString())
                    {
                        
                        //Accepting fertig
                        this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Client connected (" + adress + ")", filepath); });
                    }
                    else if (split[0].ToString() == ((int)Opcodes.T_AC_SEND_CLIENT_ID).ToString())
                    {
                        bool ClientExist = true;
                        bool ClientBanned = false;
                        //Check ob ClientID Valid (wenn nicht Error oder Banned)
                        ClientBanned = g_Sql.HasRows("SELECT * FROM atum2_db_account.dbo.td_AuthBan WHERE ClientID = '" + split[1] + "' AND Banned = 1");
                        if (ClientBanned)
                        {
                            if (DebugMode)
                            {
                                this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Client is banned: " + split[1]); });
                            }
                            con.Streamw.WriteLine((int)Opcodes.T_AC_ERR_CLIENT_BANNED);
                            con.Streamw.WriteLine((int)Opcodes.T_AC_SOCKET_CLOSE);
                            con.Streamw.Flush();
                            SocketClose(con, adress, "AccountBlocked");
                            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Client is blocked (" + split[1] + ") (" + adress + ")", filepath); });
							break;
                        }
                        else
                        {
                            if (DebugMode)
                            {
                                this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Client login: " + split[1]); });
                            }
                            ClientExist = g_Sql.HasRows("SELECT * FROM atum2_db_account.dbo.td_Auth WHERE ClientID = '" + split[1] + "'");
                            if (ClientExist == false)
                            {
                                con.Streamw.WriteLine(((int)Opcodes.T_AC_ERR_CLIENT_NOT_EXISTS).ToString());
                                con.Streamw.Flush();
                            }
                            else
                            {
                                g_Sql.Execute("UPDATE atum2_db_account.dbo.td_Auth SET LastLoginIP = '" + adress + "', LastLoginTime = GETDATE() WHERE ClientID = '" + split[1] + "'");
                                con.Streamw.WriteLine((int)Opcodes.T_AC_CLIENT_VALID_OK);
                                con.Streamw.Flush();
                                
                            }
                        }
                    }
                    else if (split[0].ToString() == ((int)Opcodes.T_AC_REQUEST_CLIENT_ID).ToString())
                    {
                        try
                        {
                            //Sende neue Client ID
                            g_Sql.Execute("INSERT INTO atum2_db_account.dbo.td_Auth VALUES ('" + g_Config.NextClientID + "', '" + adress + "', GETDATE())");
                            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("New Client registered (" + g_Config.NextClientID.ToString() + ") (" + adress + ")", filepath); });
                            g_Config.RiseNextClientID();
                            if (DebugMode)
                            {
                                this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Set NextClientID to : " + g_Config.NextClientID); });
                            }
                            con.Streamw.WriteLine((int)Opcodes.T_AC_SEND_NEW_CLIENT_ID + " " + (g_Config.NextClientID - 1).ToString());
                            con.Streamw.Flush();
                        }
                        catch (Exception ex)
                        {
							ConsoleWriteLine(ex.Message);
                            con.Streamw.WriteLine((int)Opcodes.T_AC_ERR_UNKNOWN);
                            con.Streamw.Flush();
                        }
                    }
                    else if (split[0].ToString() == ((int)Opcodes.T_AC_REQUEST_LAUNCHER_VERSION).ToString())
                    {
                        //Sende Launcher Version (vom Webserver übernommen)
                        con.Streamw.Write((int)Opcodes.T_AC_SEND_LAUNCHER_VERSION + " " + g_Config.LVersion);
                        con.Streamw.Flush();
                    }
                    else if (split[0].ToString() == ((int)Opcodes.T_AC_REQUEST_LAUNCHER_UPDATE_DATA).ToString())
                    {
                        con.Streamw.WriteLine((int)Opcodes.T_AC_REQUEST_LAUNCHER_UPDATE_DATA + " " + g_Config.LDownloadURL);
                        con.Streamw.Flush();
                    }
                    else if (split[0].ToString() == ((int)Opcodes.T_AC_REQUEST_SERVER_DATA).ToString())
                    {
                        con.Streamw.WriteLine((int)Opcodes.T_AC_SEND_SERVER_DATA + " " + g_Config.IP);
                        con.Streamw.Flush();
                    }
                    else 
                    {
                        //Connection close + packet dass invalid packet gesendet wurde
                        con.Streamw.WriteLine((int)Opcodes.T_AC_SOCKET_CLOSE);
                        con.Streamw.Flush();
                        SocketClose(con, adress, "InvalidPacketType [" + split[0] + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                //Closing connection (Security, UNKNOWN ERROR)
				try
				{
                    con.Streamw.WriteLine((int)Opcodes.T_AC_SOCKET_CLOSE);
					con.Streamw.Flush();
					SocketClose(con, adress, ex.Message);
				}
				catch
				{ }
                this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Socket disconnected [" + adress + "]"); });
                this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket closed (" + adress + "), (" + ex.Message + ")", filepath); });
                try
                {
                    _connections.Remove(adress);
                }
                catch
                {

                }
            }
        }

        
        void ConnectThread()
        {
            this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Opening Socket on: [" + g_Config.IP + ":" + g_Config.Port + "]"); });
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket open [" + g_Config.IP + ":" + g_Config.Port + "]", filepath); });
            try
            {
                _server = new TcpListener(IPAddress.Any, g_Config.Port);
                _server.Start();
                _list = new List<Connection>();
                _connections = new List<string>();
                while (true)
                {
                    var client = _server.AcceptTcpClient();
                    this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Socket connected: " + client.Client.RemoteEndPoint); });
                    this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket connected(" + client.Client.RemoteEndPoint + ")", filepath); });
                    var c = new Connection { Stream = client.GetStream() };
                    c.Streamr = new StreamReader(c.Stream);
                    c.Streamw = new StreamWriter(c.Stream);
                    _list.Add(c);
                    _connections.Add(client.Client.RemoteEndPoint.ToString());
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
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Socket closed (" + adress + ") ==> " + error, filepath); });
            _connections.Remove(adress);
            con.Stream.Close();
            con.Streamr.Close();
            con.Streamw.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Closing Server [0]", filepath); });
            Environment.Exit(0);
        }
    }
}
