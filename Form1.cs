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

namespace BigLogSearch
{
    public partial class Form1 : Form
    {
        delegate void SetLabelDelegate(string text);

        string m_Log;

        public Form1()
        {
            InitializeComponent();
        }

        #region Controls' event handlers

        private void txtLogPath_TextChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "...";
        }

        private void btnResultsToText_Click(object sender, EventArgs e)
        {
            txtResults.Text = GetResults();
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
                        this.SelLabel("Loading...");

                        DateTime now = DateTime.Now;

                        using (StreamReader reader = new StreamReader(txtLogPath.Text))
                        {
                            m_Log = reader.ReadToEnd();
                        }

                        TimeSpan t = DateTime.Now - now;

                        string timeStr = t.TotalMilliseconds <= 2000
                                        ? t.TotalMilliseconds.ToString("N0") + " ms"
                                        : t.TotalSeconds.ToString("N0") + " s";

                        FileInfo fi = new FileInfo(txtLogPath.Text);

                        string sizeStr = fi.Length > 2 * 1048576
                                        ? (fi.Length / 1048576).ToString() + " MB"
                                        : fi.Length > 2 * 1024
                                        ? (fi.Length / 1024).ToString() + " KB"
                                        : fi.Length + " B";

                        this.SelLabel("Loaded successfully (" + sizeStr + " in " + timeStr + ").");
                    }
                )
            ).Start();
        }

        private void SelLabel(string text)
        {
            if (this.lblStatus.InvokeRequired)
            {
                SetLabelDelegate d = new SetLabelDelegate(SelLabel);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lblStatus.Text = text;
            }
        }

        private string GetResults()
        {
            StringBuilder sbRe = new StringBuilder(txtRegex.Text);
            sbRe.Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");

            //Regex re = new Regex(txtRegex.Text);
            Regex re = new Regex(sbRe.ToString());
            var matches = re.Matches(m_Log);

            StringBuilder sbFormula = new StringBuilder(txtResultFormula.Text);
            sbFormula.Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");

            var formula = sbFormula.ToString();

            StringBuilder result = new StringBuilder();

            //MessageBox.Show(matches.Count.ToString());

            //for (var i = 0; i < matches.Count; i++)
            //for (var i = 0; i < 5; i++)
            //{
            //    result.Append(matches[i].Value);
            //    result.Append(formula);
            //}

            var match = re.Match(m_Log);
            while (match.Success)
            {
                result.Append(match.Value);
                result.Append(formula);
                match = match.NextMatch();
            }

            return result.ToString();
        }

        #endregion
    }
}
