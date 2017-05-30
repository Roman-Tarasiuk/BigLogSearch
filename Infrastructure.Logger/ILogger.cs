using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Logger
{
    public interface ILogger
    {
        bool Log(string info);
    }
}
