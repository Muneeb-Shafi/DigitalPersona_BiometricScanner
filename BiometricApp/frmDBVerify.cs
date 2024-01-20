using DPUruNet;
using DPXUru;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UareUSampleCSharp;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using MySqlConnector;
using Org.BouncyCastle.Asn1.Cms;


namespace BiometricApp
{
    public partial class frmDBVerify : Form
    {
        TcpListener server = null;
        string scannedUser = null;
        private int matchId;

        public frmDBVerify()
        {
            InitializeComponent();
            this.panel1.Visible = false;
            this.cboReaders.Visible = false;
        }
        public frmDBVerify(int id)
        {
            InitializeComponent();
            this.panel1.Visible = false;
            this.cboReaders.Visible = false;
            this.label7.Text = id.ToString();

        }
        private Reader currentReader;
        public Reader CurrentReader
        {
            get { return currentReader; }
            set
            {
                currentReader = value;
                SendMessage(Action.UpdateReaderState, value);
            }
        }
        private ReaderCollection _readers;
        private void LoadScanners()
        {
            cboReaders.Text = string.Empty;
            cboReaders.Items.Clear();
            cboReaders.SelectedIndex = -1;

            try
            {
                _readers = ReaderCollection.GetReaders();

                foreach (Reader Reader in _readers)
                {
                    cboReaders.Items.Add(Reader.Description.Name);
                }

                if (cboReaders.Items.Count > 0)
                {
                    cboReaders.SelectedIndex = 0;
                    //btnCaps.Enabled = true;
                    //btnSelect.Enabled = true;
                }
                else
                {
                    //btnSelect.Enabled = false;
                    //btnCaps.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                //message box:
                String text = ex.Message;
                text += "\r\n\r\nPlease check if DigitalPersona service has been started";
                String caption = "Cannot access readers";
                MessageBox.Show(text, caption);
            }
        }
        private const int PROBABILITY_ONE = 0x7fffffff;
        private Fmd firstFinger;
        int count = 0;
        DataResult<Fmd> resultEnrollment;
        List<Fmd> preenrollmentFmds;
        /// <summary>
        /// Open a device and check result for errors.
        /// </summary>
        /// <returns>Returns true if successful; false if unsuccessful</returns>
        public bool OpenReader()
        {
            using (Tracer tracer = new Tracer("Form_Main::OpenReader"))
            {
                reset = false;
                Constants.ResultCode result = Constants.ResultCode.DP_DEVICE_FAILURE;

                // Open reader
                result = currentReader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);

                if (result != Constants.ResultCode.DP_SUCCESS)
                {
                    MessageBox.Show("Error:  " + result);
                    reset = true;
                    return false;
                }

                return true;
            }
        }
        /// <summary>
        /// Check quality of the resulting capture.
        /// </summary>
        public bool CheckCaptureResult(CaptureResult captureResult)
        {
            using (Tracer tracer = new Tracer("Form_Main::CheckCaptureResult"))
            {
                if (captureResult.Data == null || captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        reset = true;
                        throw new Exception(captureResult.ResultCode.ToString());
                    }

                    // Send message if quality shows fake finger
                    if ((captureResult.Quality != Constants.CaptureQuality.DP_QUALITY_CANCELED))
                    {
                        throw new Exception("Quality - " + captureResult.Quality);
                    }
                    return false;
                }

                return true;
            }
        }
        public bool StartCaptureAsync(Reader.CaptureCallback OnCaptured)
        {
            using (Tracer tracer = new Tracer("Form_Main::StartCaptureAsync"))
            {
                // Activate capture handler
                currentReader.On_Captured += new Reader.CaptureCallback(OnCaptured);

                // Call capture
                if (!CaptureFingerAsync())
                {
                    return false;
                }

                return true;
            }
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

        public bool CaptureFingerAsync()
        {
            using (Tracer tracer = new Tracer("Form_Main::CaptureFingerAsync"))
            {
                try
                {
                    GetStatus();

                    Constants.ResultCode captureResult = currentReader.CaptureAsync(Constants.Formats.Fid.ANSI, Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, currentReader.Capabilities.Resolutions[0]);
                    if (captureResult != Constants.ResultCode.DP_SUCCESS)
                    {
                        reset = true;
                        throw new Exception("" + captureResult);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:  " + ex.Message);
                    return false;
                }
            }
        }

        public Bitmap CreateBitmap(byte[] bytes, int width, int height)
        {
            byte[] rgbBytes = new byte[bytes.Length * 3];


            //byt to bytes
            for (int i = 0; i <= bytes.Length - 1; i++)
            {
                rgbBytes[(i * 3)] = bytes[i];
                rgbBytes[(i * 3) + 1] = bytes[i];
                rgbBytes[(i * 3) + 2] = bytes[i];
            }
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            for (int i = 0; i <= bmp.Height - 1; i++)
            {
                IntPtr p = new IntPtr(data.Scan0.ToInt64() + data.Stride * i);
                System.Runtime.InteropServices.Marshal.Copy(rgbBytes, i * bmp.Width * 3, p, bmp.Width * 3);
            }

            bmp.UnlockBits(data);

            return bmp;
        }

        public void OnCaptured(CaptureResult captureResult)
        {
            reset = true;
            try
            {
                // Check capture quality and throw an error if bad.
                if (!CheckCaptureResult(captureResult)) return;

                // Create bitmap
                foreach (Fid.Fiv fiv in captureResult.Data.Views)
                {
                    SendMessage(Action.SendBitmap, CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
                }


                //Verification Code
                try
                {
                    // Check capture quality and throw an error if bad.
                    if (!CheckCaptureResult(captureResult)) return;

                    SendMessage(Action.SendMessage, "Finger Print Captured");
                    DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                    if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        if (resultConversion.ResultCode != Constants.ResultCode.DP_TOO_SMALL_AREA)
                        {
                            Reset = true;
                        }
                        throw new Exception(resultConversion.ResultCode.ToString());
                    }

                    firstFinger = resultConversion.Data;

                    try
                    {
                        MySqlConnection conn = new MySqlConnection("server=127.0.0.1;user=root;database=wapda_3.0;port=3306;password=");
                        conn.Close();
                        conn.Open();
                        MySqlDataAdapter cmd = new MySqlDataAdapter("Select * from  person_fingers", conn);
                        DataTable dt = new DataTable();
                        cmd.Fill(dt);
                        conn.Close();
                        List<string> lstledgerIds = new List<string>();
                        count = 0;
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                Console.Write(dt.Rows[i].ToString());
                                lstledgerIds.Add(dt.Rows[i]["person_id"].ToString());
                                Fmd sencondFinger = Fmd.DeserializeXml(dt.Rows[i]["code"].ToString());
                                CompareResult compare = Comparison.Compare(firstFinger, 0, sencondFinger, 0);
                                if (compare.ResultCode != Constants.ResultCode.DP_SUCCESS)
                                {
                                    Reset = true;
                                    
                                    changeAlert("Scan Failed Try Again", "Red");
                                    throw new Exception(compare.ResultCode.ToString());                                    
                                }
                                if (Convert.ToDouble(compare.Score.ToString()) == 0)
                                {
                                    matchId = int.Parse(dt.Rows[i]["person_id"].ToString());

                                    scannedUser = lstledgerIds[i].ToString();
                                    count++;
                                    changeAlert("Scan Successful", "Green");
                                    fetchUserData();
                                    socketConnection();
                                    break;
                                }

                            }
                            if (count == 0)
                            {
                                SendMessage(Action.SendMessage, "Fingerprint not registered.");
                                changeAlert("No Match, Try Again", "Red");
                                resetUserFields();
                            }

                        }

                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    SendMessage(Action.SendMessage, "Error:  " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                SendMessage(Action.SendMessage, "Error:  " + ex.Message);
            }
            finally
            {
               //socketConnection();
            }
        }

        public Dictionary<int, Fmd> Fmds
        {
            get { return fmds; }
            set { fmds = value; }
        }
        private Dictionary<int, Fmd> fmds = new Dictionary<int, Fmd>();


        public bool Reset
        {
            get { return reset; }
            set { reset = value; }
        }
        private bool reset;


        private enum Action
        {
            UpdateReaderState,
            SendBitmap,
            SendMessage
        }
        private delegate void SendMessageCallback(Action state, object payload);
        private void SendMessage(Action action, object payload)
        {
            try
            {
                if (this.pbFingerprint.InvokeRequired)
                {
                    SendMessageCallback d = new SendMessageCallback(SendMessage);
                    this.Invoke(d, new object[] { action, payload });
                }
                else
                {
                    switch (action)
                    {
                        case Action.SendMessage:
                            break;
                        case Action.SendBitmap:
                            pbFingerprint.Image = (Bitmap)payload;
                            pbFingerprint.Refresh();
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void frmDBVerify_Load(object sender, EventArgs e)
        {
            // Reset variables
            LoadScanners();
            firstFinger = null;
            resultEnrollment = null;
            preenrollmentFmds = new List<Fmd>();
            pbFingerprint.Image = null;
            if (CurrentReader != null)
            {
                CurrentReader.Dispose();
                CurrentReader = null;
            }
            CurrentReader = _readers[cboReaders.SelectedIndex];
            if (!OpenReader())
            {
                //this.Close();
            }

            if (!StartCaptureAsync(this.OnCaptured))
            {
                //this.Close();
            }

        }

        public void changeAlert(string message, string color)
        {
            alertLabel.Invoke((MethodInvoker)delegate
            {
                alertLabel.Text = message;
                if (color == "Red")
                {
                    alertLabel.ForeColor= Color.Red;
                }
                else if (color == "Green")
                {
                    alertLabel.ForeColor = Color.Green;
                }
            });
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

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(scannedUser);

                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", scannedUser);

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

        private void fetchUserData()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection("server=127.0.0.1;user=root;database=wapda_3.0;port=3306;password=");
                conn.Close();
                conn.Open();
                MySqlDataAdapter cmd = new MySqlDataAdapter("Select * from  persons where user_id = '" + matchId.ToString() + "'", conn);
                DataTable dt = new DataTable();
                cmd.Fill(dt);
                conn.Close();

                label2.Invoke((MethodInvoker)delegate{
                    label2.Text = dt.Rows[0]["person_first_name_en"].ToString() + " " + dt.Rows[0]["person_last_name_en"].ToString();
                }); label3.Invoke((MethodInvoker)delegate{
                    label3.Text = dt.Rows[0]["contact_cell_1"].ToString();
                });label4.Invoke((MethodInvoker)delegate{
                    label4.Text = dt.Rows[0]["person_cnic"].ToString();
                });label5.Invoke((MethodInvoker)delegate{
                    label5.Text = dt.Rows[0]["per_linear_address"].ToString();
                });label8.Invoke((MethodInvoker)delegate{
                    label8.Text = dt.Rows[0]["user_id"].ToString();
                }); panel1.Invoke((MethodInvoker)delegate{
                    this.panel1.Visible = true;
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(Text, e.Message);
            }
        }


        public void resetUserFields()
        {
            label2.Invoke((MethodInvoker)delegate {
                label2.Text = "";
            }); label3.Invoke((MethodInvoker)delegate {
                label3.Text = "";
            }); label4.Invoke((MethodInvoker)delegate {
                label4.Text = "";
            }); label5.Invoke((MethodInvoker)delegate {
                label5.Text = "";
            }); label8.Invoke((MethodInvoker)delegate {
                label8.Text = "";
            }); panel1.Invoke((MethodInvoker)delegate {
                this.panel1.Visible = false;
            });
        }

        private void cboReaders_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pbFingerprint_Click(object sender, EventArgs e)
        {

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Close();
            });
        }
    }
}
