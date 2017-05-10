using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Logging;

namespace BigLogSearch
{
    public partial class MainForm : Form
    {
        delegate void SetLabelDelegate(Label l, string text);

        string m_Log;
        Regex m_Re;
        Match m_Match;
        ILogger m_Logger;

        public MainForm(ILogger logger)
        {
            InitializeComponent();
            m_Logger = logger;
        }

        #region Controls' event handlers

        private void txtLogPath_TextChanged(object sender, EventArgs e)
        {
            SetLabel(lblStatus, "...");
        }

        private void btnResultsToText_Click(object sender, EventArgs e)
        {
            string result;

            if (!GetRe())
            {
                MessageBox.Show("Regex is not specified.");
                m_Logger.Log("Regex is not specified. Exit.");
                return;
            }

            result = GetAllMatches();

            m_Logger.Log("Result is ready.");

            m_Match = null;

            txtResults.Text = result;
            m_Logger.Log("Results put to textarea.");
        }

        private void btnResultsToFile_Click(object sender, EventArgs e)
        {
            string result;

            if (!GetRe())
            {
                MessageBox.Show("Regex is not specified.");
                m_Logger.Log("Regex is not specified. Exit.");
                return;
            }

            result = GetAllMatches();

            m_Logger.Log("Result is ready.");

            m_Match = null;

            SaveToFile(result);
            m_Logger.Log("Results saved to file.");
        }

        private void btnSelectLogFile_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                txtLogPath.Text = openFileDialog1.FileName;
                LoadLog();
            }
        }

        #endregion


        #region Helper Methods

        private void LoadLog()
        {
            new Thread
            (
                new ThreadStart
                (
                    () =>
                    {
                        m_Logger.Log("Trying to load log file...");

                        SetLabel(lblStatus, "Loading...");

                        DateTime begin = DateTime.Now;

                        try
                        {
                            using (StreamReader reader = new StreamReader(txtLogPath.Text, Encoding.Unicode))
                            {
                                m_Log = reader.ReadToEnd();
                                m_Logger.Log("Log file successfully loaded.");
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error while reading from the specified file.");
                            m_Logger.Log("Error while reading from the specified file.");
                            SetLabel(lblStatus, "...");
                            return;
                        }

                        string timeStr = GetTimeStr(begin, DateTime.Now);
                        string sizeStr = GetSizeStr(txtLogPath.Text);

                        SetLabel(lblStatus, "Loaded successfully (" + sizeStr + " in " + timeStr + ").");
                    }
                )
            ).Start();
        }

        private static string GetTimeStr(DateTime begin, DateTime end)
        {
            TimeSpan t = end - begin;

            string timeStr = t.TotalMilliseconds <= 2000
                            ? t.TotalMilliseconds.ToString("N0") + " ms"
                            : t.TotalSeconds.ToString("N0") + " s";
            return timeStr;
        }

        private string GetSizeStr(string path)
        {
            FileInfo fi = new FileInfo(path);

            string sizeStr = fi.Length > 2 * 1048576
                            ? (fi.Length / 1048576).ToString() + " MB"
                            : fi.Length > 2 * 1024
                            ? (fi.Length / 1024).ToString() + " KB"
                            : fi.Length + " B";
            return sizeStr;
        }

        private void SetLabel(Label l, string text)
        {
            if (l.InvokeRequired)
            {
                SetLabelDelegate d = new SetLabelDelegate(SetLabel);
                this.Invoke(d, new object[] { l, text });
            }
            else
            {
                l.Text = text;
            }
        }

        private string GetNextMatch()
        {
            StringBuilder sbFormula = new StringBuilder(txtResultFormula.Text);
            sbFormula.Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");

            var formula = sbFormula.ToString();

            StringBuilder result = new StringBuilder();

            if (m_Match == null)
            {
                m_Logger.Log("Creating new match...");
                m_Match = m_Re.Match(m_Log);
            }
            else
            {
                if (m_Match.Success)
                {
                    m_Match = m_Match.NextMatch();
                }
                else
                {
                    m_Match = Match.Empty;
                }
            }
            
            return m_Match != Match.Empty ? m_Match.Value + formula : null;
        }

        private bool GetRe()
        {
            if (txtRegex.Text == string.Empty)
            {
                return false;
            }

            StringBuilder sbRe = new StringBuilder(txtRegex.Text);
            sbRe.Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");

            m_Re = new Regex(sbRe.ToString());

            return true;
        }

        private string GetAllMatches()
        {
            string result;
            StringBuilder m_Result = new StringBuilder();

            m_Logger.Log("Getting matches started.");

            do
            {
                result = GetNextMatch();
                m_Logger.Log("Next match.");

                if (result != null)
                {
                    m_Result.Append(result);
                }
            } while (result != null);

            m_Logger.Log("All results found.");

            return m_Result.ToString();
        }

        private void SaveToFile(string info)
        {
            DialogResult dlgResult = saveFileDialog1.ShowDialog();

            if (dlgResult == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog1.FileName))
                    {
                        m_Logger.Log("Begin write to file.");

                        writer.WriteLine(info);

                        m_Logger.Log("Successfully write to file.");
                    }
                }
                catch (Exception e)
                {
                    m_Logger.Log("Error while saving to file: " + e.ToString());
                }
            }
        }

        #endregion
    }
}
