using GameRes;
using GameRes.Formats.KiriKiri;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SchemeTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Load database
            using (Stream stream = File.OpenRead(".\\GameData\\Formats.dat"))
            {
                FormatCatalog.Instance.DeserializeScheme(stream);
            }

            Xp3Opener format = FormatCatalog.Instance.ArcFormats
                .FirstOrDefault(a => a is Xp3Opener) as Xp3Opener;

            if (format != null)
            {
                Xp3Scheme scheme = format.Scheme as Xp3Scheme;

                // Add scheme information here

#if false
                byte[] cb = File.ReadAllBytes(@"MEM_10014628_00001000.mem");
                var cb2 = MemoryMarshal.Cast<byte, uint>(cb);
                for (int i = 0; i < cb2.Length; i++)
                    cb2[i] = ~cb2[i];
                var cs = new GameRes.Formats.KiriKiri.CxScheme
                {
                    Mask = 0x000,
                    Offset = 0x000,
                    PrologOrder = new byte[] { 0, 1, 2 },
                    OddBranchOrder = new byte[] { 0, 1, 2, 3, 4, 5 },
                    EvenBranchOrder = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                    ControlBlock = cb2.ToArray()
                };
                GameRes.Formats.KiriKiri.ICrypt crypt = new GameRes.Formats.KiriKiri.CxEncryption(cs);
#else
                ICrypt crypt = new XorCrypt(0x00);
#endif

                scheme.KnownSchemes.Add("game title", crypt);
            }

            var gameMap = typeof(FormatCatalog).GetField("m_game_map", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(FormatCatalog.Instance) as Dictionary<string, string>;

            if (gameMap != null)
            {
                // Add file name here
                gameMap.Add("game.exe", "game title");
            }

            // Save database
            using (Stream stream = File.Create(".\\GameData\\Formats.dat"))
            {
                FormatCatalog.Instance.SerializeScheme(stream);
            }

            FormatCatalog.Instance.SerializeSchemeJson();

        }
    }
}
