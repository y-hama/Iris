using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Iris.Forms
{
    public partial class MainForm : Form
    {
        enum Mode
        {
            ToMap,
            ToBox,
        }
        Mode mode;

        public MainForm()
        {
            InitializeComponent();

            ToMapTarget.Text = Properties.Settings.Default.ToMapTarget;
            ToMapPassword.Text = Properties.Settings.Default.ToMapPassword;
            CompressName.Text = Properties.Settings.Default.ToMapName;
            Comment.Text = Properties.Settings.Default.ToMapComment;
            ToBoxTarget.Text = Properties.Settings.Default.ToBoxTarget;
            ToBoxPassword.Text = Properties.Settings.Default.ToBoxPassword;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = "";
            switch (mode)
            {
                case Mode.ToMap:
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.SelectedPath = ToMapTarget.Text;
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        path = fbd.SelectedPath;
                        ToMapTarget.Text = path;
                    }
                    break;
                case Mode.ToBox:
                default:
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "bitmap|*.bmp";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        path = ofd.FileName;
                        ToBoxTarget.Text = path;
                    }
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (mode)
            {
                case Mode.ToMap:
                    {
                        string target = ToMapTarget.Text;
                        Regex re = new Regex(@"[^0-9]");
                        int _password = Convert.ToInt32(re.Replace(ToMapPassword.Text, ""));
                        while (_password > byte.MaxValue)
                        {
                            _password -= byte.MaxValue;
                        }
                        while (_password < 0)
                        {
                            _password += byte.MaxValue / 2;
                        }
                        byte password = (byte)_password;
                        string comment = Comment.Text;
                        string name = CompressName.Text;
                        if (name != "")
                        {
                            Utility.Shared.Config.ToMap(target, name, Utility.Shared.Config.RemoveFlag.AllCondition, password, comment);
                        }
                    }
                    break;
                case Mode.ToBox:
                default:
                    {
                        string target = ToBoxTarget.Text;
                        Regex re = new Regex(@"[^0-9]");
                        int _password = Convert.ToInt32(re.Replace(ToBoxPassword.Text, ""));
                        while (_password > byte.MaxValue)
                        {
                            _password -= byte.MaxValue;
                        }
                        while (_password < 0)
                        {
                            _password += byte.MaxValue / 2;
                        }
                        byte password = (byte)_password;
                        if (!Utility.Shared.Config.ToBox(target, password))
                        {
                            MessageBox.Show("Error\n1. File is not Exists.\n2. Bad Password.");
                        }
                    }
                    break;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ToMapTarget = ToMapTarget.Text;
            Properties.Settings.Default.ToMapPassword = ToMapPassword.Text;
            Properties.Settings.Default.ToMapName = CompressName.Text;
            Properties.Settings.Default.ToMapComment = Comment.Text;
            Properties.Settings.Default.ToBoxTarget = ToBoxTarget.Text;
            Properties.Settings.Default.ToBoxPassword = ToBoxPassword.Text;
            Properties.Settings.Default.Save();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            CheckChanged();
            ToMapPanel.Visible = false;
            ToBoxPanel.Visible = false;
            switch (mode)
            {
                case Mode.ToMap:
                    ToMapPanel.Visible = true;
                    break;
                case Mode.ToBox:
                default:
                    ToBoxPanel.Visible = true;
                    break;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            CheckChanged();
            ToMapPanel.Visible = false;
            ToBoxPanel.Visible = false;
            switch (mode)
            {
                case Mode.ToMap:
                    ToMapPanel.Visible = true;
                    break;
                case Mode.ToBox:
                default:
                    ToBoxPanel.Visible = true;
                    break;
            }
        }

        private void CheckChanged()
        {
            if (radioButton1.Checked) { mode = Mode.ToMap; }
            if (radioButton2.Checked) { mode = Mode.ToBox; }
        }
    }
}
