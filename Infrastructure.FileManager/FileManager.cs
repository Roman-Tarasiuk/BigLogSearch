using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FileManager
{
    public class FileManager
    {
        #region Fields

        private string m_FileName;
        private long m_FileLength = 0L;
        private DateTime m_FileCreationTime;
        private DateTime m_FileLastWriteTime;
        private Encoding m_Encoding;

        private string m_Data;

        private ILogger m_Logger;

        private static string LogPrefix = "File manager: ";

        #endregion


        #region Properties

        public string Data
        {
            get { return Load(m_FileName, m_Encoding); }
            protected set { m_Data = value; }
        }

        #endregion


        #region Constructors

        public FileManager()
        {
        }

        public FileManager(string path, ILogger logger, Encoding encoding)
        {
            m_FileName = path;
            m_Logger = logger;
            m_Encoding = encoding;
        }

        #endregion


        #region Public Methods

        public string Load()
        {
            return Load(m_FileName, m_Encoding);
        }

        public string Load(string path, Encoding encoding)
        {
            m_Logger.Log(LogPrefix + "start loading...");
            FileInfo fi = null;

            try
            {
                fi = new FileInfo(path);
            }
            catch
            {
                m_Logger.Log(LogPrefix + "error while getting the file info.");
                throw;
            }

            var needRead =
                (m_Data == null) ||
                (path != m_FileName) ||
                (fi.Length != m_FileLength) ||
                (fi.CreationTime != m_FileCreationTime) ||
                (fi.LastWriteTime != m_FileLastWriteTime) ||
                (encoding != m_Encoding);

            if (needRead)
            {
                try
                {
                    m_FileName = path;
                    m_FileLength = fi.Length;
                    m_FileCreationTime = fi.CreationTime;
                    m_FileLastWriteTime = fi.LastWriteTime;
                    m_Encoding = encoding;

                    m_Logger.Log(LogPrefix + "begin reading...");

                    using (StreamReader reader = new StreamReader(m_FileName, encoding))
                    {
                        Data = reader.ReadToEnd();
                    }

                    m_Logger.Log(LogPrefix + "successfully read.");
                }
                catch
                {
                    m_Logger.Log(LogPrefix + "error while reading.");
                    throw;
                }
            }
            else
            {
                m_Logger.Log(LogPrefix + "data already loaded (log file has'n changed after last reading).");
            }

            return m_Data;
        }

        #endregion
    }
}
