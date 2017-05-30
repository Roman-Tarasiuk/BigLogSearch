using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Logger;

namespace Infrastructure.LogNoLog
{
    public class Logger : ILogger
    {
        public bool Log(string info)
        {
            return true;
        }
    }
}
