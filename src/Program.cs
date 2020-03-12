using System;
using System.Globalization;
using System.Windows.Forms;

namespace RogueSurvivor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.CreateFile();
            Logger.WriteLine(Logger.Stage.INIT, "starting program...");
            Logger.WriteLine(Logger.Stage.INIT, String.Format("date : {0}.", DateTime.Now.ToString()));
            Logger.WriteLine(Logger.Stage.INIT, String.Format("game version : {0}.", SetupConfig.GAME_VERSION));

            Application.CurrentCulture = CultureInfo.InvariantCulture;  // avoids nasty "," vs "." format confusion.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SetupConfig.CreateDir();

            using (RogueForm form = new RogueForm())
            {
                // Debug mode : don't catch exceptions, I want to debug them.
                // Release mode : catch exceptions cleanly and report.
#if DEBUG
                form.Run();
#else
                try
                {
                    form.Run();
                }
                catch (Exception e)
                {
                    using (Bugreport report = new Bugreport(e))
                    {
                        report.ShowDialog();
                    }
                    Application.Exit();
                }
#endif
            }
            Logger.WriteLine(Logger.Stage.CLEAN, "exiting program...");
        }
    }
}
