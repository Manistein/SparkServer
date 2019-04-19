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
            Console.WriteLine("Please select app type \n1 Server \n2 Client \n3 BattleServer \n4 LoggerTestUtil");
            string inputStr = Console.ReadLine();
            int mode = 0;
            if (Int32.TryParse(inputStr, out mode))
            {
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
            else
            {
                Console.WriteLine("Unknow input mode {0}", inputStr);
            }
        }
    }
}
