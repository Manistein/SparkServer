using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Game.Process.TestSender
{
    class Boot : ServiceContext
    {
        protected override void Init()
        {
            base.Init();

            for (int i = 0; i < 100; i ++)
            {
                SparkServerUtility.NewService("SparkServer.Game.Process.TestSender.Sender");
            }
        }
    }
}