using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniReaderLib;
using System.IO;

namespace AuthenticationServer.Config
{
    public class Config
    {
        public string configpath = "..\\Config\\config.ini";

        public string sqlIp = "";
        public string sqlLogin = "";
        public string sqlPwd = "";
        
        public string IP = "";
        public int Port = 0;
        public int NextClientID = 0;
        public string LVersionURL = "";
        public string LDownloadURL = "";
        public string LVersion = "";

        IniReader cfgreader;
        public void ReadConfig()
        {
                if (File.Exists(configpath) == true)
                {
                    cfgreader = new IniReader(configpath);
                    //Read Mainsection
                    cfgreader.Section = "Main";
                    IP = cfgreader.ReadString("IPAdress");
                    Port = cfgreader.ReadInteger("Port");
                    NextClientID = cfgreader.ReadInteger("NextClientID");
                    LVersionURL = cfgreader.ReadString("LauncherVersionURL");
                    LDownloadURL = cfgreader.ReadString("LauncherLocation");
                    //Read SQl Section
                    cfgreader.Section = "SQL";
                    sqlIp = cfgreader.ReadString("DBIP");
                    sqlLogin = cfgreader.ReadString("DBLogin");
                    sqlPwd = cfgreader.ReadString("DBPassword");
                }
                else
                {
                    throw new System.ArgumentException("Config Read Error", "ConfigFile");
                }
        }

        public void RiseNextClientID()
        {
            cfgreader.Section = "Main";
            NextClientID++;
            cfgreader.Write("NextClientID", NextClientID);
        }
    }
}
