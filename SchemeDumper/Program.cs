using GameRes;
using System;
using System.IO;

namespace SchemeDumper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (Stream stream = File.OpenRead(".\\GameData\\Formats.dat"))
            {
                FormatCatalog.Instance.DeserializeScheme(stream);
            }

            try
            {
                FormatCatalog.Instance.SerializeSchemeJson();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            Console.WriteLine("Deserialize success.");
        }
    }
}
