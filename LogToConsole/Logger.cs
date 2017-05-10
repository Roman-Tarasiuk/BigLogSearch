using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Logging;

namespace LogToConsole
{
    public class Logger : ILogger
    {
        public bool Log(string info)
        {
            Console.WriteLine(string.Format("{0:dd.mm.yyyy hh:mm:ss.ffff}\t{1}", DateTime.Now, info));

            return true;
        }
    }
}
