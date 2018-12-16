using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2018.Helpers
{
    public static class InputHelper
    {
        public static List<string> GetInput(int day)
        {
            return File.ReadAllLines($@"{Directory.GetCurrentDirectory()}\Inputs\input{day}.txt").ToList();
        }
    }
}