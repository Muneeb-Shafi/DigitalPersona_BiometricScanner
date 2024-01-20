using DPUruNet;
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
using System.Net.WebSockets;

namespace BiometricApp
{
    public partial class registerUser : Form
    {
        TcpListener server = null;
        public int id;
        public registerUser(int E_id)
        {
            id = E_id;
            InitializeComponent();
            this.hr_idfield.Text = E_id.ToString();
            button1.Enabled = false;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            //btnBack.Enabled = false;
        }
        public registerUser()
        {
            InitializeComponent();
        }

        private void leftindexButton_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Left_Index");
            frm.ShowDialog();
        }

        private void leftthumbbutton_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Left_Thumb");
            frm.ShowDialog();
        }

        private void rightindexButton_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Right_Index");
            frm.ShowDialog();
        }

        private void rightthumbbutton_Click(object sender, EventArgs e)
        {
            frmDBEnrollment frm = new frmDBEnrollment(id, "Right_Thumb");
            frm.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection("server=127.0.0.1;user=root;database=wapda_3.0;port=3306;password=");
                conn.Close();
                conn.Open();
                MySqlDataAdapter cmd = new MySqlDataAdapter("Select * from  person_fingers where person_id = '" + id.ToString() + "'", conn);
                DataTable dt = new DataTable();
                cmd.Fill(dt);
                conn.Close();
                List<string> lstledgerIds = new List<string>();
                if (dt.Rows.Count == 4)
                {
                    button1.Enabled = true;
                    btnBack.Enabled = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("All Required FingerPrints are Saved");
            socketConnection();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            socketConnection();
        }

        private void registerUser_Load(object sender, EventArgs e)
        {

        }

        private void socketConnection()
        {
            try
            {
                Int32 port = 8000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(localAddr, port);
                server.Start();
                Console.WriteLine("Socket Listening at " + localAddr.ToString() + ":" + port);
                Byte[] bytes = new Byte[256];

                Console.Write("Waiting for a connection... ");

                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                NetworkStream stream = client.GetStream();

                byte[] msg = System.Text.Encoding.ASCII.GetBytes("Saved Successfully");

                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", "Saved Successfully");

                client.Close();

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
                Thread.Sleep(2000);
                System.Windows.Forms.Application.Exit();
            }

        }
    }
}
