using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using AuthenticationServer.Protocol;
using System.Threading;

namespace Client
{
    class Client
    {
        static TcpClient client;
        static string adress = "127.0.0.1";
        static int port = 1234;

        static void Main(string[] args)
        {
            string text = "";
            string packet = "";
            client = new TcpClient();

            try
            {
                client.Connect(adress, port);
            }
            catch
            {
                Console.WriteLine("Error, couldn't connect to Server!");
            }

            Thread serverconnection = new Thread(ConnectThread);
            serverconnection.Start();
            packet = ((int)Opcodes.T_AC_HANDSHAKE).ToString();
            SendPacket(packet);
            while (true)
            {
                Console.WriteLine("\nWhich Paket should be sended?\n\n1-Send Active ClientID\n2-Send banned ClientID\n3-Request new ClientID\n4-Request Launcher Version\n5-Request Launcher Update Data\n6-Request Server data");
                text = Console.ReadLine();
                switch (text)
                {
                    case "1":
                        packet = (int)Opcodes.T_AC_SEND_CLIENT_ID + " 1";
                        SendPacket(packet);
                        break;
                    case "2":
                        packet = (int)Opcodes.T_AC_SEND_CLIENT_ID + " 2";
                        SendPacket(packet);
                        break;
                    case "3":
                        packet = ((int)Opcodes.T_AC_REQUEST_CLIENT_ID).ToString();
                        SendPacket(packet);
                        break;
                    case "4":
                        packet = ((int)Opcodes.T_AC_REQUEST_LAUNCHER_VERSION).ToString();
                        SendPacket(packet);
                        break;
                    case "5":
                        packet = ((int)Opcodes.T_AC_REQUEST_LAUNCHER_UPDATE_DATA).ToString();
                        SendPacket(packet);
                        break;
                    case "6":
                        packet = ((int)Opcodes.T_AC_REQUEST_SERVER_DATA).ToString();
                        SendPacket(packet);
                        break;
                }
                Thread.Sleep(1000);
            }
        }

        static void SendPacket(string packet)
        {
            if (client.Connected)
            {
                var stream = client.GetStream();
                var streamw = new StreamWriter(stream);
                for (int i = 0; i < 1; i++)
                {
                    streamw.WriteLine(packet);
                    streamw.Flush();
                }
            }
            else
            {
                Console.WriteLine("Disconnected from Server, trying to reconnect: " + adress + ":" + port.ToString());
                try
                {
                    client.Connect(adress, port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
        }

        static void ConnectThread()
        {
            try
            {
                string lol;
                var stream = client.GetStream();
                var streamr = new StreamReader(stream);
                while (true)
                {
                    using (streamr)
                    {
                        while ((lol = streamr.ReadLine().ToString()) != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("Incoming packet: " +  lol);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                }
            }
            catch
            { }
        }
    }
}
