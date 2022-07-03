using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public static class Log
    {
        public static Task Msg(LogMessage msg)
        {
            Console.WriteLine("[Message] " + msg.ToString() + Environment.NewLine);
            return Task.CompletedTask;
        }

        public static Task Error(string msg)
        {
            Console.WriteLine("[Error] " + DateTime.Now.ToString("HH:mm:ss") + " " + msg + Environment.NewLine);
            return Task.CompletedTask;
        }

        public static Task Info(string msg)
        {
            Console.WriteLine("[Info] " + DateTime.Now.ToString("HH:mm:ss") + " " + msg + Environment.NewLine);
            return Task.CompletedTask;
        }
    }
}
