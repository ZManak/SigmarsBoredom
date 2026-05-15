using System.Windows.Forms;

namespace SigmarsBoredom
{
    /// <summary>
    /// Sigmar's Boredom.
    /// A solver for Opus Magnum's mini-game: Sigmar's Garden.
    /// The program assumes that you have Opus Magnum up and running, that you have opened Sigmar's Garden,
    /// and that the board is currently in its initial position.
    /// It will capture Opus Magnum's window, and manipulate the mouse cursor to click on certain spots.
    /// It is designed to automatically solve Sigmar's Garden games in an infinite loop, starting new games automatically when it's done.
    /// The game must be run as a borderless window. Supported resolutions are scaled from a 1920x1080 reference layout.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Name of the process that runs Opus Magnum.
        /// </summary>
        public const string OpusMagnumProcessName = "Lightning";

        [System.STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OverlayForm());
        }
    }
}
