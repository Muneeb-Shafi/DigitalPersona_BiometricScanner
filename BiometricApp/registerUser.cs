﻿using DPUruNet;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using WebSocketSharp;
using Fleck;

namespace BiometricApp
{
    public partial class registerUser : Form
    {
        public int id;
        public List<string> Finger_data;
        bool sendData;
        int count;       
        public registerUser(int E_id)
        {
            id = E_id;
            InitializeComponent();
            this.hr_idfield.Text = E_id.ToString();
            button1.Enabled = false;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            sendData = false;
            Finger_data = new List<string>();
        }
        public registerUser()
        {
            InitializeComponent();
            sendData = false;
        }

        #region Database FP Data Check

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection("server=127.0.0.1;user=root;database=wapda_3.0;port=3306;password=");
                conn.Close();
                conn.Open();
                MySqlDataAdapter cmd = new MySqlDataAdapter("Select * from  person_identifications where person_id = '" + id.ToString() + "'", conn);
                //MySqlDataAdapter cmd = new MySqlDataAdapter("Select * from  person_fingers where person_id = '" + id.ToString() + "'", conn);
                DataTable dt = new DataTable();
                cmd.Fill(dt);
                conn.Close();

                foreach(DataRow dr in dt.Rows)
                {
                    string type = dr["type"].ToString();

                    if (type == "Left_Thumb")
                    {
                        lthumbbtn.Enabled = false;
                    }
                    else if (type == "Left_Index")
                    {
                        leftindexButton.Enabled = false;
                    }
                    else if (type == "Left_Middle")
                    {
                        lmiddlebtn.Enabled = false;
                    }
                    else if (type == "Left_Ring")
                    {
                        lringbtn.Enabled = false;
                    }
                    else if (type == "Left_Pinky")
                    {
                        lpinkybtn.Enabled = false;
                    }
                    else if (type == "Right_Thumb")
                    {
                        rthumbbtn.Enabled = false;
                    }
                    else if (type == "Right_Index")
                    {
                        rindexbtn.Enabled = false;
                    }
                    else if (type == "Right_Middle")
                    {
                        rmiddlebtn.Enabled = false;
                    }
                    else if (type == "Right_Ring")
                    {
                        rringbtn.Enabled = false;
                    }
                    else if (type == "Right_Pinky")
                    {
                        rpinkybtn.Enabled = false;
                    }



                }
                count = dt.Rows.Count;  
                printscount.Text = dt.Rows.Count.ToString();
                List<string> lstledgerIds = new List<string>();
                button1.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Socket Connection 

        public void socketConnection()
        {
            var server = new WebSocketServer("ws://0.0.0.0:8111");
            var allSockets = new List<IWebSocketConnection>();
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Client connected!");
                    allSockets.Add(socket);
                    var timer = new System.Threading.Timer(state =>
                    {
                        if (socket.IsAvailable && sendData == true) // Check if the socket is still open
                        {
                            if (Finger_data == null) { socket.Send("Null"); }
                            else
                            {
                                foreach (var s in Finger_data)
                                {
                                    socket.Send($"{s}");
                                }
                            }
                            socket.Close();                           
                        }
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
                };

                socket.OnClose = () =>
                {
                    Console.WriteLine("Client disconnected!");
                    allSockets.Remove(socket);
                };
            });

            Console.WriteLine("WebSocket server started. Press any key to exit.");
        }

        #endregion

        #region btn calls for scans

        private void lthumbbtn_Click(object sender, EventArgs e)
        {
            
            frmDBEnrollment frm = new frmDBEnrollment(id, "Left_Thumb", this);
            frm.ShowDialog();
            if(frm.success == true) { lthumbbtn.Enabled = false; }

        }

        private void rindexbtn_click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Right_Index", this);
            frm.ShowDialog();
            if (frm.success == true) { rindexbtn.Enabled = false; }

        }

        private void leftindexButton_Click_1(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Left_Index", this);
            frm.ShowDialog();
            if (frm.success == true) { leftindexButton.Enabled = false; }

        }

        private void lmiddlebtn_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Left_Middle", this);
            frm.ShowDialog();
            if (frm.success == true) { lmiddlebtn.Enabled = false; }

        }

        private void lringbtn_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Left_Ring" , this);
            frm.ShowDialog();
            if (frm.success == true) { lringbtn.Enabled = false; }

        }

        private void lpinkybtn_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Left_Pinky", this);
            frm.ShowDialog();
            if (frm.success == true) { lpinkybtn.Enabled = false; }

        }

        private void rthumbbtn_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Right_Thumb" , this);
            frm.ShowDialog();
            if (frm.success == true) { rthumbbtn.Enabled = false; }

        }

        private void rmiddlebtn_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Right_Middle" , this);
            frm.ShowDialog();
            if (frm.success == true) { rmiddlebtn.Enabled = false; }

        }

        private void rringbtn_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Right_Ring" , this);
            frm.ShowDialog();
            if (frm.success == true) { rringbtn.Enabled = false; }

        }

        private void rpinkybtn_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Right_Pinky" , this);
            frm.ShowDialog();
            if (frm.success == true) { rpinkybtn.Enabled = false; }

        }

        #endregion

        #region Bottom Buttons

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("All Required FingerPrints are Saved");
            sendData = true;
            socketConnection();
            MessageBox.Show("Closing in 5 Secs");
            Thread.Sleep(5000);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("All FingerPrint Data Will Be Lost", "Are You Sure", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                sendData = true;
                Finger_data = null;
                socketConnection();
                Thread.Sleep(5000);
                this.Close();
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
            
        }

        #endregion
    }
} 