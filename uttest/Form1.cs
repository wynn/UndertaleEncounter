using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace uttest
{
    public partial class Form1 : Form
    {
        string encounterInfoFile = "undertale encounters.txt";
        string undertaleDataFile = "data.win";
        bool loadedUndertaleDataFile = false;

        const long BattleGroupOffset = 0x9EB414; //Windows 1.001
        const long ActivateDebugModeOffset = 0x725D8C; //Also for Windows 1.001

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(encounterInfoFile);
            if (fileInfo != null && fileInfo.Exists)
            {
                StreamReader file = new StreamReader(encounterInfoFile);
                string text = file.ReadToEnd();
                var encounters = text.Split(new string[] { "\n" }, StringSplitOptions.None);

                foreach (string s in encounters)
                {
                    var x = s.Split(new string[] { "\t" }, StringSplitOptions.None);
                    if (x.Length >= 2) {
                        this.comboBox1.Items.Add(new ComboboxItem(x[0], x[1]));
                    }
                }
                file.Close();
            }
            else
            {
                MessageBox.Show("Encounter list not found (put it in the same folder as this program), exiting.");
                this.Close();
            }

            FileInfo UTDataFileInfo = new FileInfo(undertaleDataFile);
            if (UTDataFileInfo != null && UTDataFileInfo.Exists)
            {
                loadedUndertaleDataFile = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("Undertale").Length > 0)
            {
                MessageBox.Show("Close Undertale before trying to edit its data file.");
                return;
            }
            if(!loadedUndertaleDataFile)
            {
                MessageBox.Show("Please load an Undertale data file first.");
                return;
            }

            var selected = (ComboboxItem)this.comboBox1.SelectedItem;
            if(selected != null)
            {
                FileInfo fileInfo = new FileInfo(undertaleDataFile);
                try
                {
                    //make sure the user has a copy of Undertale's data in case something goes wrong
                    var directory = fileInfo.Directory;
                    FileInfo backup = new FileInfo(directory.ToString() + @"\backup.win");
                    //create a copy of data.win if none exists
                    if (!backup.Exists)
                    {
                        fileInfo.CopyTo(backup.ToString());
                    }

                    FileStream fs = new FileStream(undertaleDataFile, FileMode.Open, FileAccess.ReadWrite);
                    byte monsterByte = Convert.ToByte(selected.BattleID);

                    fs.Seek(BattleGroupOffset, SeekOrigin.Begin);
                    fs.WriteByte(monsterByte);
                    MessageBox.Show("Successfully wrote to file.");
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error description:" + "\n" + ex.ToString(), "Error");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "data.win";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.ReadWrite);
                    fs.Seek(ActivateDebugModeOffset, SeekOrigin.Begin);
                    int activated = fs.ReadByte();
                    if(activated == 0)
                    {
                        var result = MessageBox.Show("The selected data file does not have Debug Mode enabled. Do you want to enable Debug Mode?", "Information", MessageBoxButtons.YesNo);
                        if(result == DialogResult.Yes)
                        {
                            fs.Seek(ActivateDebugModeOffset, SeekOrigin.Begin);
                            fs.WriteByte(1);
                            loadedUndertaleDataFile = true;
                        }
                    }
                    else if(activated != 1)
                    {
                        MessageBox.Show("Byte at offset " + ActivateDebugModeOffset + " not equal to 1 or 0 - invalid Undertale data file detected.");
                    }
                    else
                    {
                        undertaleDataFile = openFileDialog1.FileName;
                        loadedUndertaleDataFile = true;
                    }
                    fs.Close();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Insufficient rights to edit the selected file - Please run this program as an admin.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error description:" + "\n" + ex.ToString(), "Error");
                }
            }
        }
    }
}
