using System;
using System.Threading;

using SparkServer.Framework.Service;
using SparkServer.Framework.Service.Logger;


namespace SparkServer.TestUtil
{
    class LoggerTestUtil
    {
        public void Run()
        {
            LoggerService logerService = new LoggerService();
            logerService.Init();

            Random ro = new Random();

            Message message = new Message();
            
            for (; ; )
            {
                message.Source = ro.Next();

                message.Destination = ro.Next();

                message.Data = GenData(ro.Next(20, 100));

                logerService.Callback(message);

                Thread.Sleep(1000);
            }
        }

        private byte[] GenData(int iSize)
        {
            byte[] data = new byte[iSize];

            byte[] charSet = new byte[] { 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47};

            for (int i = 0; i < iSize; ++i)
            {
                data[i] = charSet[i%charSet.Length];
            }
            return data;
        }
    };
}