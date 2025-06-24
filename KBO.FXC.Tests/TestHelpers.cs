using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBO.FXC.Tests
{
    internal static class TestHelpers
    {
        private static string fxcPath;
        public static string GetFXC()
        {
            if (fxcPath != null)
                return fxcPath;
            try
            {
                foreach (var line in File.ReadAllLines("fxcpath.txt"))
                {
                    if (File.Exists(line))
                        return fxcPath = line;
                }
            }
            catch (FileNotFoundException) { }
            Assert.Ignore("fxc path was not specified for tests");
            return null!;
        }
        public static void EnsureASCII(string filePath)
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!char.IsAscii((char)fs.ReadByte()))
            {
                fs.Position = 0;
                byte[] asciiContents;
                using (StreamReader reader = new StreamReader(fs, detectEncodingFromByteOrderMarks: true, leaveOpen: false))
                    asciiContents = Encoding.ASCII.GetBytes(reader.ReadToEnd());
                File.WriteAllBytes(filePath, asciiContents);
            }
        }
    }
}
