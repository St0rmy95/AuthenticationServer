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
        void OnHandshake(Connection con, string[] packet, string adress)
        {
            this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Client connected (" + adress + ")", m_sFilepath); });
        }

        void OnSendClient(Connection con, string[] packet, string adress)
        {
            bool ClientExist = true;
            bool ClientBanned = false;
            //Check ob ClientID Valid (wenn nicht Error oder Banned)
            ClientBanned = g_Sql.HasRows(String.Format(QUERY_GET_CLIENT_BANNED, packet[1]));
            if (ClientBanned)
            {
                if (m_bDebugMode)
                {
                    this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Client is banned: " + packet[1]); });
                }
                Send(con, (int)Opcodes.T_AC_ERR_CLIENT_BANNED);
                Send(con, (int)Opcodes.T_AC_SOCKET_CLOSE);
                SocketClose(con, adress, "AccountBlocked");
                this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("Client is blocked (" + packet[1] + ") (" + adress + ")", m_sFilepath); });
                return;
            }
            else
            {
                if (m_bDebugMode)
                {
                    this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Client login: " + packet[1]); });
                }
                ClientExist = g_Sql.HasRows(String.Format(QUERY_GET_CLIENT, packet[1]));
                if (ClientExist == false)
                {
                    Send(con, ((int)Opcodes.T_AC_ERR_CLIENT_NOT_EXISTS).ToString());
                }
                else
                {
                    g_Sql.Execute(String.Format(QUERY_UPDATE_LAST_LOGIN, adress, packet[1]));
                    Send(con, (int)Opcodes.T_AC_CLIENT_VALID_OK);
                }
            }
        }

        void OnRequestClientID(Connection con, string[]packet, string adress)
        {
            try
            {
                //Sende neue Client ID
                g_Sql.Execute(String.Format(QUERY_INSERT_NEW_CLIENT, g_Config.NextClientID, adress));
                this.Invoke((MethodInvoker)delegate { g_Log.WriteLog("New Client registered (" + g_Config.NextClientID.ToString() + ") (" + adress + ")", m_sFilepath); });
                g_Config.RiseNextClientID();
                if (m_bDebugMode)
                {
                    this.Invoke((MethodInvoker)delegate { ConsoleWriteLine("Set NextClientID to : " + g_Config.NextClientID); });
                }
               Send(con, (int)Opcodes.T_AC_SEND_NEW_CLIENT_ID + " " + (g_Config.NextClientID - 1).ToString());
            }
            catch (Exception ex)
            {
                ConsoleWriteLine(ex.Message);
                Send(con, (int)Opcodes.T_AC_ERR_UNKNOWN);
            }
        }

        void OnRequestLauncherVersion(Connection con, string[] packet, string adress)
        {
            Send(con, (int)Opcodes.T_AC_SEND_LAUNCHER_VERSION + " " + g_Config.LVersion);
        }

        void OnRequestUpdateData(Connection con, string[] packet, string adress)
        {
            Send(con, (int)Opcodes.T_AC_REQUEST_LAUNCHER_UPDATE_DATA + " " + g_Config.LDownloadURL);
        }

        void OnRequestServerData(Connection con, string[] packet, string adress)
        {
            Send(con, (int)Opcodes.T_AC_SEND_SERVER_DATA + " " + g_Config.IP);
        }
    }
}
