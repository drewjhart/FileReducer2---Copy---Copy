using Microsoft.WindowsAPICodePack.Dialogs;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Packaging;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;
using SchwabenCode.QuickIO;
using ZetaLongPaths;

namespace FileReducer2
{
	

	public partial class Form1 : Form
    {

        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        BackgroundWorker m_oWorker;
        bool cancel = false;
        Form2 compressWindow = new Form2();
        string ELString = DateTime.Now.ToString("ddMMyyss_hhmm.");
		string path;
		string path2;
		int cleanCount = 0;
		int errorCount = 0;
		int infectedCount = 0;
		StorageInfo myRoot;
		string checkString = "";
		//known bad call backs Dos command, exe, delete, copy
		string badtextDOS = "0044004F00530043006F006D006D0061006E0064";
		string badtextEXE = "65007B0065";
		string badtextDelete = "00640065006C006500740065";
		string badtextCopy = "63006F00700079";

		public Form1()
        {
                       
            InitializeComponent();           
            m_oWorker = new BackgroundWorker();
            m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            m_oWorker.ProgressChanged += new ProgressChangedEventHandler(m_oWorker_ProgressChanged);
            m_oWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_oWorker_RunWorkerCompleted);
            m_oWorker.WorkerReportsProgress = true;
            m_oWorker.WorkerSupportsCancellation = true;
            string tempLocation = AppDomain.CurrentDomain.BaseDirectory + @"errorLogs\";
            path = Path.Combine(tempLocation, ELString+"ErrorLog.txt");
			path2 = Path.Combine(tempLocation, ELString + "ScanLog.txt");
			//Console.WriteLine(tempLocation + "**" + copiedFile);
			using (StreamWriter sw = File.AppendText(path2))
			{
				sw.WriteLine("Infected Files:");
			}


			Console.WriteLine("ELString" + path);
            
        }
       
                         
        private void button2_Click(object sender, EventArgs e)
        {
            label7.Text = "";
            label7.Refresh();
            if (listBox1.Items.Count > 0)
            {
                ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(listBox1);
                selectedItems = listBox1.SelectedItems;

                if (listBox1.SelectedIndex != -1)
                {
                    for (int i = selectedItems.Count -1; i>=0; i--)
                        listBox1.Items.Remove(listBox1.SelectedItems[i]);
                }
            }
            else
                label7.Text = "No Directories Loaded";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label7.Text = "";
            label7.Refresh();
            bool dup = false;           
            var folderSelectorDialog = new CommonOpenFileDialog();
            folderSelectorDialog.Title = "Select Directory";
            folderSelectorDialog.IsFolderPicker = true;
            folderSelectorDialog.EnsureReadOnly = true;
            if (folderSelectorDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string line = @folderSelectorDialog.FileName;

                if (listBox1.Items.Count > 0)
                {
                    dup = checkLBDup(line);
                    if (dup)
                    {
                        label7.Text = "Duplicate Directory Detected";
                    }
                    else
                        listBox1.Items.Add(line);
                }
                else
                    listBox1.Items.Add(line);
            }
            else
            {
                return;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            label7.Text = "";
            label7.Refresh();
            bool dup = false;           
            OpenFileDialog openFDialog = new OpenFileDialog();
            openFDialog.Title = "Select Text File";
            openFDialog.Filter = "Text|*.txt";
            if (openFDialog.ShowDialog() == DialogResult.OK)
            {

                string newDir = @openFDialog.FileName;
                using (var stream = File.OpenRead(newDir))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = "";
                    while ((line = @reader.ReadLine()) != null)
                    {
                        line = checkLBValid(line);
                        if (line.Equals("invalid"))
                        {
                            label7.Text = "Invalid Directory Detected";
                        }
                        else if (listBox1.Items.Count > 0 && !line.Equals("invalid"))
                        {
                            dup = checkLBDup(line);
                           
                            if (dup)
                            {
                                label7.Text = "Duplicate Directory Detected";
                            }
                            else
                            {
                                listBox1.Items.Add(line);
                            }
                        }
                        else
                            listBox1.Items.Add(line);
                    }
                }
            }
            else
            {
                return;
            }
        }
        private string checkLBValid (string line)
        {
            string validCheck = "invalid";
            Console.WriteLine("Checking");
            try
            {
                validCheck = Path.GetFullPath(line);
            
            }
            catch
            {
                Console.WriteLine("Found");
                return "invalid";
            }
            return validCheck;
        }

        private bool checkLBDup (string line) //checks for duplicate directories before adding to listbox1
        {
            foreach (var item in listBox1.Items)
            {
                string currentDir = Convert.ToString(item);
                if (line.Equals(currentDir))
                {
                    
                    return true;
                }               
            }
            return false;
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            label7.Text = "";
            cancel = false;            

            if (listBox1.Items.Count > 0)
            {               
                List<string> packageForWorker = new List<string>();
                foreach (var item in listBox1.Items)
                {                        
                    packageForWorker.Add(item.ToString());
                }                   

                m_oWorker.RunWorkerAsync(packageForWorker); //backgroundworker starts file reduction on other thread
                
            }
            else
                label7.Text = "No Directories Loaded";
        }           

        //backgroundworker updates the GUI thread on the progress of the file reduction here
        void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
            if (e.UserState.Equals("Initializing..."))
            {
                label4.Text = (string)e.UserState;
                label4.Refresh(); 
            }
            else
            {
                label4.Text = "Scanning Files...";
                label4.Refresh();

                string[]PP = (string[])e.UserState;
                label8.Text = "File:" + PP[2] + "/" + PP[3];
                label8.Refresh();
                label5.Text = PP[0];
                label5.Refresh();
                label9.Text = "Estimated Time Remaining (min): " + PP[1];
                label9.Refresh();
            }                     
        }

        void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                label4.Text = "File Scan Stopped";
                label4.Refresh();
            }
            else if (e.Error != null)
            {
                string tempLocation = AppDomain.CurrentDomain.BaseDirectory + @"errorLogs\";
                string path = Path.Combine(tempLocation, ELString + "ErrorLog.txt");
                StreamWriter sw = new StreamWriter(path, append: true);
                using (sw)
                {
                    sw.WriteLine(e);

                }
                sw.Close();
                label4.Text = "Error";
                label4.Refresh();
            }
            else
            {
                label4.Text = "File Scan Completed (" + Math.Round((watch.ElapsedMilliseconds * 0.0000167), 2)+" min)" ;
                label4.Refresh();
                watch.Stop();
            }
			int totalCount = infectedCount + cleanCount +errorCount;
			using (StreamWriter sw = File.AppendText(path2))
			{
				sw.WriteLine("-----------------------------------------------------------");
				sw.WriteLine("Summary:");
				sw.WriteLine("Total Files Scanned: " +totalCount);
				sw.WriteLine("Clean Files Detected: " + cleanCount);
				sw.WriteLine("Infected Files Detected: " + infectedCount);
				sw.WriteLine("Errored Files Detected: " + errorCount);
			}
		}

		void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            watch.Start();
            int fileNum = 0;
            string currentDirectory = "";
            long compressionLevel = 15L;
            List<string> pkgForWorkerR = (List<string>)e.Argument;
		


			foreach (var item in pkgForWorkerR)
            {
				Console.WriteLine("Item: " + item);
				
				var ZlpcurrentDirectory = new ZlpDirectoryInfo(item);
				currentDirectory = ZlpcurrentDirectory.FullName;


				if (Directory.Exists(currentDirectory))
                {
                    
                        fileNum = FileCounter(currentDirectory, fileNum);
                        m_oWorker.ReportProgress(fileNum, "Initializing...");//report progress back - update label status to initializing
                    
                }   
            }
			Console.WriteLine("NUMBER OF FILES: " + fileNum);
			
            int currentNum = 0;
            foreach (var item in pkgForWorkerR)
            {
				var ZlpcurrentDirectory = new ZlpDirectoryInfo(item);
				currentDirectory = ZlpcurrentDirectory.FullName;
				Console.WriteLine(currentDirectory);
				if (Directory.Exists(currentDirectory))
                {                    
                    
                    currentNum = ProcessDirectory(currentDirectory, currentNum, fileNum, compressionLevel); //begin actual work of converting files
                    if (cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                   
                }
                
            }
        }

        /*this counts all the files, to output to the user and for the calculation of the estimated time
         * this is the "initializing..." phase
         */
        public int FileCounter(string targetdirectory, int numFile)
        {

			var fileEntries = @Directory.GetFiles(targetdirectory, "*.max", SearchOption.AllDirectories);
			Console.WriteLine(fileEntries.Length);
			numFile = fileEntries.Length;
			
			return numFile;


			
			var folderPath = new ZlpDirectoryInfo(targetdirectory);
            foreach (var ZlpFileName in folderPath.GetFiles())
            {
				string fileName = ZlpFileName.FullName;
               // Console.WriteLine("fileNAme: " + fileName);
                string testForMax = fileName.Substring(fileName.Length - 4);
           

                if (testForMax.Equals(".max") || testForMax.Equals(".Max") || testForMax.Equals(".MAX"))
                {                  
                    numFile++;
					Console.WriteLine(numFile);
                }
            }

			
			foreach (var ZlpSubdirectory in folderPath.GetDirectories())
			{
				
				string subdirectory = ZlpSubdirectory.FullName;
				numFile = FileCounter(subdirectory, numFile);
				
				numFile++;
			}

			return numFile;

		
		}
       
        /*copies files locally, sends them to convert, copies files back, deletes files
         * this file movement is the fastest way of working over the network, just using Image.FromFile() remotely takes far longer 
         */
        public int ProcessDirectory(string targetDirectory, int currentNum, int totalNum, long compressionLevel) 
        {

			var folderPath = new ZlpDirectoryInfo(targetDirectory);
			
			foreach (var ZlpFileName in folderPath.GetFiles())
			{
				string fileName = ZlpFileName.FullName;
				string testForMax = fileName.Substring(fileName.Length - 4);
                if (testForMax.Equals(".max") || testForMax.Equals(".Max") || testForMax.Equals(".MAX"))
                {
					
					string justFile = "";
					
					string copiedFile = "";
                    string pdfLocalFile = "";
                    string pdfRemoteFile = "";
                    string pdfFileName = "";
                    string originalPath = "";
           
                

                    currentNum++;
			

					double timeMin = Math.Round((watch.ElapsedMilliseconds * 0.0000167), 2);
                    double filesRemaining = (double)totalNum / (double)currentNum;
                    double timeRemaining = (timeMin * (filesRemaining) - timeMin);
                    timeRemaining = Math.Round(timeRemaining, 2);

                    string[] PP = new string[4];
                    PP[0] = fileName;
                    PP[1] = Convert.ToString(timeRemaining);
                    PP[2] = Convert.ToString(currentNum);
                    PP[3] = Convert.ToString(totalNum);

                    m_oWorker.ReportProgress(5, PP);

                    //Console.WriteLine("Writing: " + fileName);
                    
                }
                if (m_oWorker.CancellationPending)
                {                    
                    //e.Cancel = true;
                    cancel = true;
                    return currentNum;
                }

            }
			string[] subdirectoryEntries;
			try
			{
				subdirectoryEntries = Directory.GetDirectories(targetDirectory);
				foreach (string subdirectory in subdirectoryEntries)
				{
					currentNum = ProcessDirectory(subdirectory, currentNum, totalNum, compressionLevel);
				}
			}
			catch (PathTooLongException e)
			{
				using (StreamWriter sw = File.AppendText(path))
				{

					sw.WriteLine("-----------------------------------------------------------------------------");
					sw.WriteLine("Directory Name:" + "PathTooLongException");
					sw.WriteLine("Exception: ");
					sw.WriteLine(e);
					errorCount++;
					return currentNum;
				}
			}
		



			return currentNum;
        }


        private void button5_Click_1(object sender, EventArgs e)
        {

            label7.Text = "";
            label7.Refresh();

            if (m_oWorker.IsBusy)
            {
                m_oWorker.CancelAsync();
            }
            else
            {
                label7.Text = "Process Not Running";
                label7.Refresh();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


		private string fileCopier(string fileName, string justFile) //copies image to local folder, faster than working w networked file
		{

			string tempLocation = @"C:\IBI\fileScanner\";
			string copiedFile = tempLocation + justFile;
	

			try
			{
				System.IO.File.Copy(fileName, copiedFile, true);
			}
			catch (System.IO.IOException e)
			{
				Console.WriteLine(e.Message);
			}
			return copiedFile;
		}
		
	}
	

}
