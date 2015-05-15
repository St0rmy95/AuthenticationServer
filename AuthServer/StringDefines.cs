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

namespace AuthServer
{
    public partial class Form1
    {
        string PACKET_KEY = "Yu2T6ag8xoE4Xj4mIfR5fNIpmqX11TLt";

        string COMMAND_SERVER = "server";
        string COMMAND_STOP = "stop";
        string COMMAND_CLIENT = "client";
        string COMMAND_BLOCK = "block";
        string COMMAND_UNBLOCK = "unblock";
        string COMMAND_HELP = "help";
        string COMMAND_HELP_2 = "?";
        string COMMAND_LIST = "list";

        string QUERY_CLIENTID_EXIST = "SELECT ClientID FROM atum2_db_account.dbo.td_AuthBan WHERE ClientID = '{0}'";
        string QUERY_BAN_CLIENTID = "UPDATE atum2_db_account.dbo.td_AuthBan SET Banned = 1, BanDate = GETDATE() WHERE ClientID = '{0}'";
        string QUERY_BAN_NEWCLIENTID = "INSERT INTO atum2_db_account.dbo.td_AuthBan (ClientID, Banned, BanDate) VALUES ('{0}', 1, GETDATE())";
        string QUERY_UNBAN_CLIENTID = "UPDATE atum2_db_account.dbo.td_AuthBan SET Banned = 0 WHERE ClientID = '{0}'";
        string QUERY_GET_CLIENT_BANNED = "SELECT * FROM atum2_db_account.dbo.td_AuthBan WHERE ClientID = '{0}' AND Banned = 1";
        string QUERY_GET_CLIENT = "SELECT * FROM atum2_db_account.dbo.td_Auth WHERE ClientID = '{0}'";
        string QUERY_UPDATE_LAST_LOGIN = "UPDATE atum2_db_account.dbo.td_Auth SET LastLoginIP = '{0}', LastLoginTime = GETDATE() WHERE ClientID = '{1}'";
        string QUERY_INSERT_NEW_CLIENT = "INSERT INTO atum2_db_account.dbo.td_Auth VALUES ('{0}', '{1}', GETDATE())";
    }
}
