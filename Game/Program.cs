using System;

namespace Torq2
{
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		public static void Main(string[] pArgs)
		{
			using (Torq2Game pGame = new Torq2Game())
			{
				pGame.Run();
			}
		}
	}
}

