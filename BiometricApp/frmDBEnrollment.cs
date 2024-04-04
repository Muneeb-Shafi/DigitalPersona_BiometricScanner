using DPUruNet;
using MySqlConnector;
using Org.BouncyCastle.Asn1.Cms;
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
using System.Net.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Runtime.InteropServices;

namespace BiometricApp
{
    public partial class frmDBEnrollment : Form
    {
        public int enrollementId;
        public string type;
        int count;
        private const int PROBABILITY_ONE = 0x7fffffff;
        private Fmd firstFinger;
        DataResult<Fmd> resultEnrollment;
        List<Fmd> preenrollmentFmds;


        public frmDBEnrollment()
        {
            InitializeComponent();
        }

        public frmDBEnrollment(int id, string ftype)
        {
            enrollementId = id;
            type = ftype;
            InitializeComponent();
            this.doneButton.Enabled = false;
            this.lblPlaceFinger.Text = "Place "+ftype+" on the scanner";
            count = 0;

        }
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


        private enum Action
        {
            UpdateReaderState,
            SendBitmap,
            SendMessage
        }
        private delegate void SendMessageCallback(Action state, object payload);
        private void SendMessage(Action action, object payload)
        {
            if (count > 4) { return; }
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
                            MessageBox.Show((string)payload);
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

        private Reader _reader;

        /// <summary>
        /// Hookup capture handler and start capture.
        /// </summary>
        /// <param name="OnCaptured">Delegate to hookup as handler of the On_Captured event</param>
        /// <returns>Returns true if successful; false if unsuccessful</returns>
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
        /// <summary>
        /// Check the device status before starting capture.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Function to capture a finger. Always get status first and calibrate or wait if necessary.  Always check status and capture errors.
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Cancel the capture and then close the reader.
        /// </summary>
        /// <param name="OnCaptured">Delegate to unhook as handler of the On_Captured event </param>
        public void CancelCaptureAndCloseReader(Reader.CaptureCallback OnCaptured)
        {
            using (Tracer tracer = new Tracer("Form_Main::CancelCaptureAndCloseReader"))
            {
                if (currentReader != null)
                {
                    currentReader.CancelCapture();

                    // Dispose of reader handle and unhook reader events.
                    currentReader.Dispose();

                    if (reset)
                    {
                        CurrentReader = null;
                    }
                }
            }
        }
        // When set by child forms, shows s/n and enables buttons.
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
                }
                else
                {

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
        private void frmDBEnrollment_Load(object sender, EventArgs e)
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
            }

            if (!StartCaptureAsync(this.OnCaptured))
            {
            }



        }

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


        public void OnCaptured(CaptureResult captureResult)
        {
            if(count == 4) { return; }

            try
            {
                if (!CheckCaptureResult(captureResult)) return;

                foreach (Fid.Fiv fiv in captureResult.Data.Views)
                {
                    SendMessage(Action.SendBitmap, CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
                }

                try
                {
                    count++;
                    DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);


                    Invoke((MethodInvoker)delegate { label2.Text = count.ToString(); });
                    Invoke((MethodInvoker)delegate { label3.Text = (4 - count).ToString(); });                
                    if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        Reset = true;
                        throw new Exception(resultConversion.ResultCode.ToString());
                    }

                    preenrollmentFmds.Add(resultConversion.Data);

                    if (count == 4)
                    {
                        resultEnrollment = DPUruNet.Enrollment.CreateEnrollmentFmd(Constants.Formats.Fmd.ANSI, preenrollmentFmds);

                        if (resultEnrollment.ResultCode == Constants.ResultCode.DP_SUCCESS)
                        {
                            preenrollmentFmds.Clear();
                            saveToDB();
                            doneButton.Invoke((MethodInvoker)delegate
                            {
                                doneButton.Enabled = true;
                            });
                            return;
                        }
                        else if (resultEnrollment.ResultCode == Constants.ResultCode.DP_ENROLLMENT_INVALID_SET)
                        {
                            SendMessage(Action.SendMessage, "Enrollment was unsuccessful.  Please try again.");
                            preenrollmentFmds.Clear();
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    SendMessage(Action.SendMessage, "Error:  " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                SendMessage(Action.SendMessage, "Error:  " + ex.Message);
            }
        }
        public Bitmap CreateBitmap(byte[] bytes, int width, int height)
        {
            byte[] rgbBytes = new byte[bytes.Length * 3];

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


        private void frmDBEnrollment_FormClosing(object sender, FormClosingEventArgs e)
        {
            CancelCaptureAndCloseReader(this.OnCaptured);
        }


        public async void saveToDB()
        {
            Console.WriteLine("In Database Function");
            if (resultEnrollment != null)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection("server=127.0.0.1;user=root;database=wapda_3.0;port=3306;password="))
                    {
                        conn.Open();

                        //string sql = "INSERT INTO person_fingers (person_id, type, code, created_at, updated_at) VALUES (@v1, @v2, @v3, @v5, @v6)";
                        string sql = "INSERT INTO person_identifications (person_id, type, code, created_at, updated_at) VALUES (@v1, @v2, @v3, @v5, @v6)";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@v1", enrollementId);
                            cmd.Parameters.AddWithValue("@v2", type);
                            cmd.Parameters.AddWithValue("@v3", Fmd.SerializeXml(resultEnrollment.Data));
                            cmd.Parameters.AddWithValue("@v5", DateTime.Now);
                            cmd.Parameters.AddWithValue("@v6", DateTime.Now);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show(type + " Saved Succesfully to Database");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    MessageBox.Show("Error While Inserting Data in Database Error: " + ex.Message);
                }
            }
            Console.WriteLine(type);
            try
            {

                Image image = pbFingerprint.Image;
                string imageData = null;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    var raw = image.RawFormat;
                    image.Save(memoryStream, ImageFormat.Png);
                    byte[] imageBytes = memoryStream.ToArray();
                    imageData = Convert.ToBase64String(imageBytes);
                }
                string fileName = $"{enrollementId}_{type}";

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var formData = new MultipartFormDataContent())
                        {
                            formData.Add(new StringContent(imageData), "image");
                            formData.Add(new StringContent(fileName), "filename");
                            formData.Add(new StringContent(type), "type");
                            formData.Add(new StringContent($"{enrollementId}"), "hr_id");
                            var response = await httpClient.PostAsync("http://wapda.test/save-fingerprint-images", formData);
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Image uploaded successfully.");
                                string responseContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine("Response content:");
                                Console.WriteLine(responseContent);
                            }
                            else
                            {
                                Console.WriteLine("Failed to upload image. Status code: " + response.StatusCode);
                                string responseContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine("Response content:");
                                Console.WriteLine(responseContent);
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("HTTP request failed: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Close();
            });
        }
    }
}
