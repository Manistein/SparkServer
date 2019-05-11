using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkServer.Network;
using SparkServer.Framework;
using SparkServer.Framework.Utility;
using SparkServer.Test;

namespace SparkServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputMode = args[0];
            int mode = 0;
            if (inputMode == "TestCases")
            {
                mode = 1;
            }
            else if (inputMode == "SparkServer")
            {
                mode = 2;
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
                        string caseName = args[1];

                        TestCases testCases = new TestCases();
                        testCases.Run(caseName);
                    } break;
                case 2:
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
            SparkServerUtility.NewService("SparkServer.Game.Service.BattleTaskDispatcher", "BattleDispatcher");
        }
    }
}
