using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;

namespace HDDLED
{
    public partial class InvisibleForm : Form
    {
        #region Global Variables
        NotifyIcon hddLedIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddLedWorker;
        #endregion

        #region Main Form
        public InvisibleForm()
        {
            InitializeComponent();

            //Load icons from files into objects
            activeIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

            //Create notify icons
            hddLedIcon = new NotifyIcon();
            hddLedIcon.Icon = idleIcon;
            hddLedIcon.Visible = true;

            //Create all contextmenu items and add them to notification tray icon
            MenuItem progNameMenuItem = new MenuItem("HDD led v0.1 by: Martin");
            MenuItem quitMenuItem = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            hddLedIcon.ContextMenu = contextMenu;

            //Wire up quit button to close application
            quitMenuItem.Click += QuitMenuItem_Click;

            //Hide the form because we dont need it
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            //Start worker thread that pulls HDD activity
            hddLedWorker = new Thread(new ThreadStart(HddActivityThread));
            hddLedWorker.Start();
        }
        #endregion

        #region Event handlers
        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            hddLedWorker.Abort();
            hddLedIcon.Dispose();
            this.Close();
        }
        #endregion

        #region Threads
        /// <summary>
        /// This is the thread that monitors hdd activity
        /// </summary>
        public void HddActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");

            try
            {
                while (true)
                {
                    ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
                    foreach(ManagementObject obj in driveDataClassCollection)
                    {
                        if(obj["Name"].ToString() == "_Total")
                        {
                            if(Convert.ToUInt64(obj["DiskBytesPersec"]) > 0){
                                hddLedIcon.Icon = activeIcon;
                            }
                            else
                            {
                                hddLedIcon.Icon = idleIcon;
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
            }catch(ThreadAbortException tbe)
            {
                driveDataClass.Dispose();
            }
        }
        #endregion
    }
}
