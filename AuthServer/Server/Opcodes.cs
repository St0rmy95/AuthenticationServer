using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationServer.Protocol
{
    public enum Opcodes : int
    {
        //Server Side packets (sending to client)
        T_AC_SEND_NEW_CLIENT_ID             =   0x000,
        T_AC_CLIENT_VALID_OK                =   0x001,
        T_AC_SEND_LAUNCHER_VERSION          =   0x002,
        T_AC_SEND_LAUNCHER_UPDATE_DATA      =   0x003,
        T_AC_SEND_SERVER_DATA               =   0x004,
        T_AC_SOCKET_CLOSE                   =   0x005,

        //Error Codes
        T_AC_ERR_UNKNOWN                    =   0x006,
        T_AC_ERR_CLIENT_BANNED              =   0x007,
        T_AC_ERR_CLIENT_NOT_EXISTS          =   0x008,

        //Client Side packets (incoming packets)
        T_AC_HANDSHAKE                      =   0x009,
        T_AC_SEND_CLIENT_ID                 =   0x010,
        T_AC_REQUEST_CLIENT_ID              =   0x011,
        T_AC_REQUEST_LAUNCHER_VERSION       =   0x012,
        T_AC_REQUEST_LAUNCHER_UPDATE_DATA   =   0x013,
        T_AC_REQUEST_SERVER_DATA            =   0x014,

        T_INVALID_PROTOCOL                  =   0xFFF

    }

    public class Protocol
    {
        public string GetPacketString(int Packet)
        {
            switch (Packet)
            { 
                case (int)Opcodes.T_AC_SEND_NEW_CLIENT_ID:
                    return "T_AC_SEND_NEW_CLIENT_ID";
                case (int)Opcodes.T_AC_CLIENT_VALID_OK:
                    return "T_AC_CLIENT_VALID_OK";
                case (int)Opcodes.T_AC_SEND_LAUNCHER_VERSION:
                    return "T_AC_SEND_LAUNCHER_VERSION";
                case (int)Opcodes.T_AC_SEND_LAUNCHER_UPDATE_DATA:
                    return "T_AC_SEND_LAUNCHER_UPDATE_DATA";
                case (int)Opcodes.T_AC_SEND_SERVER_DATA:
                    return "T_AC_SEND_SERVER_DATA";
                case (int)Opcodes.T_AC_SOCKET_CLOSE:
                    return "T_AC_SOCKET_CLOSE";
                case (int)Opcodes.T_AC_ERR_UNKNOWN:
                    return "T_AC_ERR_UNKNOWN";
                case (int)Opcodes.T_AC_ERR_CLIENT_BANNED:
                    return "T_AC_ERR_CLIENT_BANNED";
                case (int)Opcodes.T_AC_ERR_CLIENT_NOT_EXISTS:
                    return "T_AC_ERR_CLIENT_NOT_EXISTS";
                case (int)Opcodes.T_AC_HANDSHAKE:
                    return "T_AC_HANDSHAKE";
                case (int)Opcodes.T_AC_SEND_CLIENT_ID:
                    return "T_AC_SEND_CLIENT_ID";
                case (int)Opcodes.T_AC_REQUEST_CLIENT_ID:
                    return "T_AC_REQUEST_CLIENT_ID";
                case (int)Opcodes.T_AC_REQUEST_LAUNCHER_VERSION:
                    return "T_AC_REQUEST_LAUNCHER_VERSION";
                case (int)Opcodes.T_AC_REQUEST_LAUNCHER_UPDATE_DATA:
                    return "T_AC_REQUEST_LAUNCHER_UPDATE_DATA";
                case (int)Opcodes.T_AC_REQUEST_SERVER_DATA:
                    return "T_AC_REQUEST_SERVER_DATA";
                case (int)Opcodes.T_INVALID_PROTOCOL:
                    return "T_INVALID_PROTOCOL";
                default:
                    return "UNKNOWN_PACKET_TYPE";
            }
        }
    }
}