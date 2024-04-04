using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using DPUruNet;
using System.Reflection;
using System.ComponentModel;
using BiometricApp;


namespace UareUSampleCSharp
{
    public partial class Form_Main : Form
    {
        /// <summary>
        /// Holds fmds enrolled by the enrollment GUI.
        /// </summary>
        public Dictionary<int, Fmd> Fmds
        {
            get { return fmds; }
            set { fmds = value; }
        }
        private Dictionary<int, Fmd> fmds = new Dictionary<int, Fmd>();

        /// <summary>
        /// Reset the UI causing the user to reselect a reader.
        /// </summary>
        public bool Reset
        {
            get { return reset; }
            set { reset = value; }
        }
        private bool reset;

        public Form_Main()
        {
            using (Tracer tracer = new Tracer("Form_Main::Form_Main"))
            {

                InitializeComponent();
            }
        }

        // When set by child forms, shows s/n and enables buttons.
        public Reader CurrentReader
        {
            get { return currentReader; }
            set
            {
                currentReader = value;
                //SendMessage(Action.UpdateReaderState, value);
            }
        }
        private Reader currentReader;

        private void btnReaderSelect_Click(System.Object sender, System.EventArgs e)
        {
            registerUser form = new registerUser();
            form.ShowDialog();
        }

        public void GetStatus()
        {
            using (Tracer tracer = new Tracer("Form_Main::GetStatus"))
            {
                Constants.ResultCode result = currentReader.GetStatus();

                if ((result != Constants.ResultCode.DP_SUCCESS))
                {
                    reset = true;
                    throw new Exception("" + result);
                }

                if ((currentReader.Status.Status == Constants.ReaderStatuses.DP_STATUS_BUSY))
                {
                    Thread.Sleep(50);
                }
                else if ((currentReader.Status.Status == Constants.ReaderStatuses.DP_STATUS_NEED_CALIBRATION))
                {
                    currentReader.Calibrate();
                }
                else if ((currentReader.Status.Status != Constants.ReaderStatuses.DP_STATUS_READY))
                {
                    throw new Exception("Reader Status - " + currentReader.Status.Status);
                }
            }
        }


        private enum Action
        {
            UpdateReaderState
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmDBVerify newform = new frmDBVerify();
            newform.ShowDialog();

        }
    }
}