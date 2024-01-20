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
            if (args.Length > 0)
            {
                if (args[0] == "Register") 
                {
                    Application.Run(new registerUser(int.Parse(args[1])));
                }
                else if (args[0] == "Verify")
                {
                    Application.Run(new frmDBVerify(int.Parse(args[1])));
                }
            }
            else
            {
                Application.Run(new Form_Main());
            }

        }
    }
}
