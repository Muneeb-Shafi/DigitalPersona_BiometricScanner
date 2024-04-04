using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UareUSampleCSharp;

namespace BiometricApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Console.WriteLine(args[0] + " " + args[1]);
            if (args.Length > 0)
            {
                string value1 = args[0].Split('%')[0];

                string value2 = args[1].Split('/')[0];
                if (value1 == "1") 
                {
                    Application.Run(new registerUser(int.Parse(value2)));
                }
                else if (value1 == "2")
                {
                    Application.Run(new frmDBVerify(int.Parse(value2)));
                }
            }
            else
            {
                Form form = new registerUser();
                Application.Run(form);

            }

        }
    }
}
