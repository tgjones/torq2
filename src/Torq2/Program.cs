using System;

namespace Torq2
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
					using (Torq2Game game = new Torq2Game())
            {
                game.Run();
            }
        }
    }
#endif
}

