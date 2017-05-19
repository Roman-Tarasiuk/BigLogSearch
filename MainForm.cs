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
using Model.Search;
using Infrastructure.FileManager;

namespace BigLogSearch
{
    public partial class MainForm : Form
    {
        delegate void SetLabelDelegate(Label l, string text);

        #region Fields

        private ILogger m_Logger;

        private string m_LogData;

        private FileManager m_FileManager;
        private Searcher m_Searcher;

        private Encoding m_Encoding = Encoding.Default;
        private ToolStripMenuItem m_CurrentEncodingItem;

        #endregion


        #region Constructors

        public MainForm(ILogger logger)
        {
            InitializeComponent();

            m_Logger = logger;

            m_FileManager = new FileManager(null, logger, Encoding.Default);
            m_Searcher = new Searcher(null, null, logger);

            BuildEncodingsMenu();
        }

        #endregion


        #region Controls' event handlers

        private void txtLogPath_TextChanged(object sender, EventArgs e)
        {
            SetLabel(lblStatus, "...");
        }

        private void btnResultsToText_Click(object sender, EventArgs e)
        {
            var results = GetResults();

            txtResults.Text = results;
            m_Logger.Log("Results put to textarea.");
        }

        private void btnResultsToFile_Click(object sender, EventArgs e)
        {
            string savePath = null;
            DialogResult dlgResult = saveFileDialog1.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                savePath = saveFileDialog1.FileName;
                m_Logger.Log("Results file selected: " + savePath);
            }
            else
            {
                return;
            }

            var results = GetResults();

            var saved = SaveToFile(results, savePath);
            if (saved)
            {
                m_Logger.Log("Results saved to file.");
            }
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion


        #region Helper Methods

        private void BuildEncodingsMenu()
        {
            var encodings = Encoding.GetEncodings();

            var encodungGroupsNames = new string[] { "ibm", "windows", "x-mac", "x-", "iso-", "others" };
            ToolStripMenuItem[] subItems = new ToolStripMenuItem[encodungGroupsNames.Length];

            var i = 0;
            foreach (var g in encodungGroupsNames)
            {
                subItems[i++] = (ToolStripMenuItem)encodingToolStripMenuItem.DropDownItems.Add(g);
            }

            foreach (var e in encodings)
            {
                var index = GetEncodingGroupIndex(e.Name, encodungGroupsNames);

                subItems[index].DropDownItems.Add(e.Name);
                var count = subItems[index].DropDownItems.Count;
                subItems[index].DropDownItems[count - 1].Click += OnEncodingSelected;
            }

            foreach (var s in subItems)
            {
                ResortToolStripItemCollection(s.DropDownItems);
            }
        }

        private int GetEncodingGroupIndex(string encodingName, string[] groupNames)
        {
            for (int i = 0; i <= groupNames.Length - 2; i++)
            {
                if (encodingName.ToLower().StartsWith(groupNames[i].ToLower()))
                {
                    return i;
                }
            }

            return groupNames.Length - 1;
        }

        private void OnEncodingSelected(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (m_CurrentEncodingItem != null)
            {
                m_CurrentEncodingItem.Checked = false;
            }

            m_Encoding = Encoding.GetEncoding(item.Text);
            item.Checked = true;
            m_CurrentEncodingItem = item;
        }

        // next 2 methods – http://stackoverflow.com//questions/5102205/how-to-sort-items-in-toolstripitemcollection
        //
        private void ResortToolStripItemCollection(ToolStripItemCollection coll)
        {
            System.Collections.ArrayList oAList = new System.Collections.ArrayList(coll);
            oAList.Sort(new ToolStripItemComparer());
            coll.Clear();

            foreach (ToolStripItem oItem in oAList)
            {
                coll.Add(oItem);
            }
        }

        public class ToolStripItemComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                ToolStripItem oItem1 = (ToolStripItem)x;
                ToolStripItem oItem2 = (ToolStripItem)y;
                //return string.Compare(oItem1.Text, oItem2.Text, StringComparison.Ordinal);
                return string.Compare(oItem1.Text, oItem2.Text, true);
            }
        }

        private void LoadLog()
        {
            new Thread
            (
                new ThreadStart
                (
                    () =>
                    {
                        m_Logger.Log("Trying to load log file: " + txtLogPath.Text);

                        SetLabel(lblStatus, "Loading...");

                        DateTime begin = DateTime.Now;

                        try
                        {
                            m_LogData = m_FileManager.Load(txtLogPath.Text, m_Encoding);
                        }
                        catch (Exception)
                        {
                            m_Logger.Log("Error while reading from the specified file.");
                            SetLabel(lblStatus, "...");
                            MessageBox.Show("Error while reading from the specified file.");
                            return;
                        }

                        string timeStr = GetTimeStr(begin, DateTime.Now);
                        string sizeStr = GetSizeStr(txtLogPath.Text);

                        m_Logger.Log("Log file successfully loaded (" + sizeStr + " in " + timeStr + ").");
                        SetLabel(lblStatus, "Loaded successfully (" + sizeStr + " in " + timeStr + ").");
                    }
                )
            ).Start();
        }

        private bool SaveToFile(string info, string path)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog1.FileName))
                {
                    m_Logger.Log("Begin write to file...");

                    writer.WriteLine(info);

                    m_Logger.Log("Successfully written to file.");
                }

                return true;
            }
            catch (Exception e)
            {
                m_Logger.Log("Error while saving to file: " + e.ToString());
            }

            return false;
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

        private string GetResults()
        {
            string result;

            if (txtRegex.Text == string.Empty)
            {
                MessageBox.Show("Regex is not specified.");
                m_Logger.Log("Regex is not specified. Exit.");
                return null;
            }

            m_LogData = m_FileManager.Load(txtLogPath.Text, m_Encoding);
            m_Searcher.Reset(m_LogData, txtRegex.Text, m_Logger);
            result = m_Searcher.GetAllMatches();

            m_Logger.Log("Results are ready.");

            return result;
        }

        #endregion
    }
}
