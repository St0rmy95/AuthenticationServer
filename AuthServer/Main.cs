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
        bool m_bDebugMode;
        // Get DateTime for Log file name
        public static string m_sFilepath = "..\\..\\Logs\\AuthSystem_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".log";

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
            m_bDebugMode = true;
#else
			ConsoleWriteLine("############################ Combat-Rivals AuthenticationServer #################################");
            this.Text = "AuthenticationServer (Release x86)";
            DebugMode = false;
#endif
			ConsoleWriteLine("Using Logfile \"" + m_sFilepath + "\"");
            ConsoleWriteLine("Reading Configuration...");
            g_Log.WriteLog("Loading Configuration", m_sFilepath);

            try
            {
                g_Config.ReadConfig();
            }
            catch
            {
                ConsoleWriteLine("Configuration Error!");
                g_Log.WriteLog("Config Error (0)", m_sFilepath);
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
            g_Log.WriteLog("Connecting to DB", m_sFilepath);
            try
            {
                g_Sql.SQLConnect(g_Config.sqlIp, g_Config.sqlLogin, g_Config.sqlPwd);
            }
            catch
            {
                ConsoleWriteLine("Database connection failed");
                g_Log.WriteLog("Database connection failed", m_sFilepath);
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
                        OnInput(args);
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
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Console: Shutdown", m_sFilepath); });
            ConsoleWriteLine("Server closed!");
            g_Sql.SQLDisconnect();
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Closing Database Connection", m_sFilepath); });
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Shutting down Server", m_sFilepath); });
            Environment.Exit(0);
        }


        public void Version()
        {
            ConsoleWriteLine("Authentication Server\nCopyright by St0rmy");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Closing Server [0]", m_sFilepath); });
            Environment.Exit(0);
        }
    }
}
