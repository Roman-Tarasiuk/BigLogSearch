using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Logging;

namespace LogToFile
{
    public class Logger : ILogger
    {
        private string m_Path;

        public Logger(string path)
        {
            m_Path = path;
        }

        public bool Log(string info)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(m_Path, true))
                {
                    writer.WriteLine(string.Format("{0:dd.mm.yyyy hh:mm:ss.ffff}\t{1}", DateTime.Now, info));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error while writing to log file{0}    {1}",
                    Environment.NewLine,
                    exception);

                return false;
            }

            return true;
        }
    }
}
