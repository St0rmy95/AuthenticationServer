//******************************************COPYRIGHT*****************************************
//* Combat-Rivals PreLauncher © St0rmy 2013 - Combat-Gaming Network                          *
//* All rights to the owners and creators of ths Project                                     *
//* Do not copy, change or use this Project without the permission of the owner (St0rmy)     *
//********************************************************************************************


//Defines
//#define LOCAL


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;

namespace AcePreLauncher
{
    public partial class Form1 : Form
    {
		string sLauncherURL = "http://144.76.100.3:80/autoupdate/launcher/launcher.atm"; //launcher.atm URL
		string sVersionURL = "http://144.76.100.3:80/autoupdate/launcher/lversion.ver"; //Launcher version file URL 144.76.100.3
        string WindowName = "Combat-Rivals";

        string STRERR_FILE_MISSING = "File not found.";
        string STRERR_FILE_INVALID = "The .exe file is invalid.";
        string STRERR_DOWNL_UPDATE_FAIL = "Update file cannot be downloaded.";
        string STRERR_UPDATE_CON_FAIL = "Cannot connect to the download server.";
        string STRERR_SERVER_OFFLINE = "Server is not activated.";
        string STRERR_UPDATE_FAIL = "Auto update failed.\r\nPlease reinstall the game.\r\n";

        string sGameIP;
        int iPrePort = 15100; //PreServer Port 
        string sVersionLocal;
		string sVerLocal;
        string sVersionNew;
        int iVersion;
        int iVersionLocal;
        static TcpClient client;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Visible = true;
            pictureBox1.Width = 0;
            this.Text = WindowName;
#if LOCAL
        sGameIP = "127.0.0.1"; //Local IP
#else
			var address = Dns.GetHostAddresses("25.220.51.217")[0];
			sGameIP = address.ToString(); //Main Root IP 25.220.51.217
#endif
            timer1.Start();
        }

        private void Startup()
        {
            try
            {
                client = new TcpClient(sGameIP, iPrePort);
            }
            catch
            {
                MessageBox.Show(STRERR_SERVER_OFFLINE, WindowName);
                Environment.Exit(0);
            }
            client.Close();
            pictureBox1.Width += 46;
            #region RemoteVersion
            try
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = null;
                sVersionNew = webClient.DownloadString(sVersionURL);
            }
            catch
            {
                MessageBox.Show(STRERR_UPDATE_CON_FAIL, WindowName);
                Environment.Exit(0);
            }
            pictureBox1.Width += 46;
            if (File.Exists(Application.StartupPath + "\\launcher.atm") == false)
            {
#if !DEBUG
                Thread dlLauncher = new Thread(DOWNLOAD_LAUNCHER);
                dlLauncher.Start();
#endif
            }
            else
            {
                string[] sVersionTempSplit = sVersionNew.Split(new Char[] { '.' });
                int i = 0;
                try
                {
                    string sVersion_Temp;
                    StringBuilder _TEMP = new StringBuilder();
                    while (i < sVersionTempSplit.Length)
                    {
                        _TEMP.Append(sVersionTempSplit[i]);
                        i++;
                    }
                    sVersion_Temp = _TEMP.ToString();
                    iVersion = Convert.ToInt32(sVersion_Temp);
                }
                catch
                {
                    MessageBox.Show(STRERR_UPDATE_FAIL, WindowName);
                }
                pictureBox1.Width += 46;
            #endregion
                try
                {
					string _temp;
					StreamReader sr = new StreamReader(Application.StartupPath + "\\VersionInfo.ver");
					while ((_temp = sr.ReadLine()) != null)
					{
						if (!_temp.Contains("#"))
						{
							if (_temp.Contains("LauncherVersion"))
							{
								string[] _versTemp = _temp.Split(new char[] { '	' });
								sVersionLocal = _versTemp[_versTemp.Length - 1];
								sVerLocal = _versTemp[_versTemp.Length - 1];
							}
						}
					}
					sr.Close();
                    string[] sVersionLocalSplit = sVersionLocal.Split(new Char[] { '.' });
                    StringBuilder _TEMP = new StringBuilder();
                    i = 0;
                    while (i < sVersionLocalSplit.Length)
                    {
                        _TEMP.Append(sVersionLocalSplit[i]);
                        i++;
                    }
                    sVersionLocal = _TEMP.ToString();
                    iVersionLocal = Convert.ToInt32(sVersionLocal);
                }
                catch
                {
                    MessageBox.Show(STRERR_FILE_MISSING, WindowName);
                }
                pictureBox1.Width += 46;
                if (iVersionLocal < iVersion)
                {
                    Thread dlLauncher = new Thread(DOWNLOAD_LAUNCHER);
                    dlLauncher.Start();
                }
                else if (iVersionLocal == iVersion)
                {
                    pictureBox1.Width = 185;
					Thread Start_Proc = new Thread(START_LAUNCHER);
                    Start_Proc.Start();
                }
                else if (iVersionLocal > iVersion)
                {
                    MessageBox.Show(STRERR_UPDATE_FAIL, WindowName);
                    Environment.Exit(0);
                }
            }
        }

        private void DOWNLOAD_LAUNCHER()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate { pictureBox1.Width = 0; });
                //this.Invoke((MethodInvoker)delegate { pictureBox1.Visible = true; }); Not used, is already visible
                WebClient client = new WebClient();
                client.Proxy = null;
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileAsync(new Uri(sLauncherURL), (Application.StartupPath + "\\launcher.atm"));
                while (client.IsBusy == true)
                {

                }
                client_DownloadFileCompleted();
            }
            catch
            {
                MessageBox.Show(STRERR_DOWNL_UPDATE_FAIL, WindowName);
                Environment.Exit(0);
            }
        }

        private void START_LAUNCHER()
        {
            try
            {
                Thread.Sleep(2000);
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = Application.StartupPath + "\\launcher.atm";
                startInfo.Arguments = sGameIP;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;

                Process.Start(startInfo);
                Environment.Exit(0);
            }
            catch
            {
                MessageBox.Show(STRERR_FILE_INVALID, WindowName);
                Environment.Exit(0);
            }
        }

        double percentage = 0;
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                if (int.Parse(Math.Truncate(percentage).ToString()) * 2 < 185)
                {
                    double bytesIn = double.Parse(e.BytesReceived.ToString());
                    double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    percentage = bytesIn / totalBytes * 100;
                    pictureBox1.Width = int.Parse(Math.Truncate(percentage).ToString()) * 2;
                    this.Text = "Download: " +  percentage.ToString("####0.00") + "%";
                }
                else
                {
                    pictureBox1.Width = 185;
                }
            });
        }

        void client_DownloadFileCompleted()
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                this.Text = WindowName;
                try
                {
					string text = File.ReadAllText("VersionInfo.ver");
					text = text.Replace(sVerLocal, sVersionNew);
					File.WriteAllText("VersionInfo.ver", text);
                    START_LAUNCHER();
                }
                catch
                {
                    MessageBox.Show(STRERR_FILE_MISSING , WindowName);
                    Environment.Exit(0);
                }
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Startup();
        }
    }
}
