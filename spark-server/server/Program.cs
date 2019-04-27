using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkServer.Network;
using SparkServer.Examples;
using SparkServer.Framework;
using SparkServer.TestUtil;

namespace SparkServer
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
            else if (inputMode == "SparkServer")
            {
                mode = 3;
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
                        string bootPath = args[1];

                        Server battleServer = new Server();
                        battleServer.Run(bootPath, InitBattleServices);
                    }break;
                default:
                    {
                        Console.WriteLine("Mode:{0} not supported", mode);
                    }break;
            }
        }

        public static void InitBattleServices()
        {

        }
    }
}
