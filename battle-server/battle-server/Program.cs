using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleServer.Network;
using BattleServer.Examples;
using BattleServer.Game;
using BattleServer.TestUtil;

namespace BattleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputMode = args[0];
            int mode = 0;
            if (inputMode == "TCPServerExample")
            {
                mode = 1;
            }
            else if (inputMode == "TCPClientExample")
            {
                mode = 2;
            }
            else if (inputMode == "BattleServer")
            {
                mode = 3;
            }
            else if (inputMode == "LoggerTest")
            {
                mode = 4;
            }
            else
            {
                Console.WriteLine("Unknow input mode {0}", inputMode);
                return;
            }

            switch(mode)
            {
                case 1:
                    {
                        TCPServerExample tcpServerExample = new TCPServerExample();
                        tcpServerExample.Run();
                    }break;
                case 2:
                    {
                        TCPClientExample tcpClientExample = new TCPClientExample();
                        tcpClientExample.Run();
                    }break;
                case 3:
                    {
                        Server battleServer = new Server();
                        battleServer.Run();
                    }break;
                case 4:
                    {
                        LoggerTestUtil loggerUtil = new LoggerTestUtil();
                        loggerUtil.Run();
                    } break;
                default:
                    {
                        Console.WriteLine("Mode:{0} not supported", mode);
                    }break;
            }
        }
    }
}
