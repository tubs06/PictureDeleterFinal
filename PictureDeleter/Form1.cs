using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PictureDeleter
{
    public partial class Form1 : Form
    {
        private List<string> ActiveFileNames = new List<string>();
        private List<string> FlaggedFileNames = new List<string>();
        private string ActiveDirectory;
        private int CurrentItemIndex;
        private bool DeletionFlag = false;
        private FileStream CurrImageFilestream;
        private void DisplayCurrentImage()
        {
            if (CurrImageFilestream != null)
            {
                CurrImageFilestream.Close();
            }
            //pictureBox1.Image = System.Drawing.Image.FromStream(CurrImageFilestream = new FileStream(ActiveFileNames[CurrentItemIndex], FileMode.Open, FileAccess.Read));
            try
            {
                // Try to open image
                pictureBox1.Image = System.Drawing.Image.FromStream(CurrImageFilestream = new FileStream(ActiveFileNames[CurrentItemIndex], FileMode.Open, FileAccess.Read));
            }
            catch
            {
                // If failed, show error message and remove from lists.
                MessageBox.Show("Error, the image \"" + ActiveFileNames[CurrentItemIndex].Substring(ActiveFileNames[CurrentItemIndex].LastIndexOf('\\') + 1, ActiveFileNames[CurrentItemIndex].Length - ActiveFileNames[CurrentItemIndex].LastIndexOf('\\') - 1) + "\" could not be loaded. Removing from list.", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ActiveFileNames.Count == 1)
                {
                    ResetApp();
                    return;
                }
                ActiveFileNames.RemoveAt(CurrentItemIndex);
                DeletionFlag = true;
                checkedListBox1.Items.RemoveAt(CurrentItemIndex);
                button1_Click(this, null);
                DeletionFlag = false;
            }
        }

        // After last file deletion, or reset of directory call this function.
        private void ResetApp()
        {
            // Disable all controls
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;

            //Disable check list.
            checkedListBox1.Enabled = false;

            // Clear both file lists.
            ActiveFileNames.Clear();
            FlaggedFileNames.Clear();

            // If file stream is open, close it and dispose image.
            if(CurrImageFilestream != null)
            {
                CurrImageFilestream.Close();
            }

            // Set other vars to default
            ActiveDirectory = null;
            CurrentItemIndex = 0;
            textBox1.Text = null;

            // Delete all items in check list.
            DeletionFlag = true;
            checkedListBox1.Items.Clear();
            DeletionFlag = false;
        }

        public Form1()
        {
            // Standard startup.
            InitializeComponent();

            // Disable functionality until valid directory recieved.
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            textBox1.Enabled = false;
            checkedListBox1.Enabled = false;
            checkedListBox1.Items.Clear();
        }

        // Get directory!
        private void button5_Click(object sender, EventArgs e)
        {

            // Session already open.
            if(textBox1.Text.Length != 0)
            {
                DialogResult SafetyCheck = MessageBox.Show("Are you sure you would like to switch directories?", "Safety Check", MessageBoxButtons.YesNo);
                if(SafetyCheck == DialogResult.No)
                {
                    return;
                }
                ResetApp();
            }
            // Get images directory
            var FDB = new FolderBrowserDialog();
            DialogResult res = FDB.ShowDialog();
            // If valid
            if (res == DialogResult.Cancel)
            {
                return;
            }
            if (res == DialogResult.OK && !string.IsNullOrWhiteSpace(FDB.SelectedPath))
            {
                // Add images to valid files
                ActiveFileNames.AddRange(Directory.GetFiles(FDB.SelectedPath, "*.jpg"));
                ActiveFileNames.AddRange(Directory.GetFiles(FDB.SelectedPath, "*.png"));

                // If directory has no images, return with error.
                if(ActiveFileNames.Count == 0)
                {
                    MessageBox.Show("Error, no images in directory!", "Directory Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ActiveDirectory = FDB.SelectedPath + "\\";
            }
            // If invalid directory, return with error.
            else
            {
                MessageBox.Show("Error, directory is invalid!", "Directory Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            // Add file names to ActiveFileNames
            foreach(string item in ActiveFileNames)
            {
                checkedListBox1.Items.Add(item.Substring(item.LastIndexOf('\\')+1, item.Length - item.LastIndexOf('\\') - 1));
            }

            textBox1.Text = ActiveDirectory;

            // Enable buttons
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            // Exit

            //Set selected item to first in list, display image
            checkedListBox1.SelectedIndex = (CurrentItemIndex = 0);
            checkedListBox1.Enabled = true;
        }

        // Previous button clicked
        private void button1_Click(object sender, EventArgs e)
        {
            // Subtract one from item index, and reset to end of list if at index 0.
            CurrentItemIndex = CurrentItemIndex == 0 ? ActiveFileNames.Count() - 1 : CurrentItemIndex - 1;
            checkedListBox1.SelectedIndex = CurrentItemIndex;
        }

        // Next button clicked
        private void button2_Click(object sender, EventArgs e)
        {
            // Add one to item index, but reset to 0 if at the end of the list.
            CurrentItemIndex = CurrentItemIndex == ActiveFileNames.Count - 1 ? 0 : CurrentItemIndex + 1;
            checkedListBox1.SelectedIndex = CurrentItemIndex;
        }


        // Queue button clicked
        private void button3_Click(object sender, EventArgs e)
        {
            // If the queue button is clicked, and the item is queued, remove check and remove from flagged list.
            if (FlaggedFileNames.Contains(ActiveFileNames[CurrentItemIndex]))
            {
                checkedListBox1.SetItemChecked(CurrentItemIndex, false);
                FlaggedFileNames.Remove(ActiveFileNames[CurrentItemIndex]);
            }
            // If the queue button is clicked, and the item is not queued, add cehck and add to flagged list.
            else
            {
                checkedListBox1.SetItemChecked(CurrentItemIndex, true);
                FlaggedFileNames.Add(ActiveFileNames[CurrentItemIndex]);
            }
        }

        // Item manually selected
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If this resulted from program deletion, ignore.
            if (DeletionFlag == true)
                return;
            CurrentItemIndex = checkedListBox1.SelectedIndex;
            DisplayCurrentImage();
        }

        // Item manually checked
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // If the new value is checked, add to flagged list.
            if(e.NewValue == CheckState.Checked)
            {
                FlaggedFileNames.Add(ActiveFileNames[CurrentItemIndex]);
            }
            // Otherwise, remove from flagged list.
            else
            {
                FlaggedFileNames.Remove(ActiveFileNames[CurrentItemIndex]);
            }
        }

        // Delete selected items
        private void button4_Click(object sender, EventArgs e)
        {
            // Ask for confirmation from user, exit if no.
            DialogResult SafetyCheck = MessageBox.Show("Are you sure you would like to delete the selected files?", "Safety Check", MessageBoxButtons.YesNo);
            if (DialogResult == DialogResult.No)
            {
                return;
            }

            // Close file stream and delete all flagged files.
            CurrImageFilestream.Close();
            foreach (string file in FlaggedFileNames) 
            {
                File.Delete(file);
            }


            // Remove all deleted files from checklist and active file list.
            DeletionFlag = true;
            foreach (string delFile in FlaggedFileNames)
            {
                checkedListBox1.Items.Remove(delFile.Substring(delFile.LastIndexOf('\\') + 1, delFile.Length - delFile.LastIndexOf('\\') - 1));
                ActiveFileNames.Remove(delFile);
            }
            DeletionFlag = false;

            // If the file count is 0, we have exhausted all files. Exit program.
            if(ActiveFileNames.Count == 0)
            {
                ResetApp();
                return;
            }

        }
    }
}
