#region Using Statements
using System;
#endregion

namespace Pallopeli
{    
    /// @author Lauri Makkonen
    /// @version v1.2.0 (21.05.2024)
    /// <summary>
    /// Apuluokka joka laittaa pelin käyntiin.
    /// </summary>
    public static class Ohjelma
    {
        [STAThread]
        static void Main()
        {
            using var game = new Pallopeli();
            game.Run();
        }
    }
}