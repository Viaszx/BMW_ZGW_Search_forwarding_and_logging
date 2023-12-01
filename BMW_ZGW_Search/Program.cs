/*
 * Created in SharpDevelop.
 * User: Vias
 * Date: 01/01/2023
 */
using System;
using System.Windows.Forms;

namespace BMW_ZGW_Search
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
