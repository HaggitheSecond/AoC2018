using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace AoC2018
{
    public static class Helpers
    {
        public static List<string> GetInput(int day)
        {
            return File.ReadAllLines($@"C:\Users\Yannik\Desktop\Advent Of Code\input{day}.txt").ToList();
        }

        public static List<char> AllLowercaseLetters => new List<char>("abcdefghijklmnopqrstuvwxyz");

        public static int GetBetterRandom(this RandomNumberGenerator generator, int maxValue)
        {
            var bytesNeeded = (int)Math.Ceiling((decimal)maxValue / 255);
            var numbers = new byte[bytesNeeded];

            generator.GetBytes(numbers);

            var number = numbers.Sum(f => f);

            return number >= maxValue ? generator.GetBetterRandom(maxValue) : number;
        }

        public static string FormatForDisplay(this int self)
        {
            return self >= 1000 ? self.ToString() :
                self >= 100 ? " " + self :
                self >= 10 ? "  " + self
                : "   " + self;
        }
    }

    public static class ChristmasHelper
    {
        // from here: https://asciiart.website//index.php?art=holiday/christmas/other

        public static IList<string> GetWreathLines() => new List<string>
        {
            "      ,....,",
            "   ,;;:o;;;o;;,",
            " ,;;o;'''''';;;;,",
            ",;;;;        ;;o;,",
            ";o;;          ;;;;",
            ";;o;          ;;o;",
            "';;;,  _  _  ,;;;'",
            " \';o;;/_\\/_\\;;o;\'",
            "   \';;\\_\\/_/;;\'",
            "      '//\\\\'",
            "      //  \\\\",
            "     |/    \\|"
        };

        private static IList<ConsoleColor> GoodColors => new List<ConsoleColor>
        {
            ConsoleColor.Red,
            ConsoleColor.Blue,
            ConsoleColor.Cyan,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.Green
        };

        public static void DrawWreathRibbonToConsole(int left, int top, int padding = 5)
        {
            var lenghtPerWreath = GetWreathLines().Max(f => f.Length) + padding;

            var wreathCount = (int)Math.Abs(Console.BufferWidth / lenghtPerWreath);

            for (int i = 0; i < wreathCount; i++)
            {
                DrawWreathToConsole(i * lenghtPerWreath, top);
            }
        }

        public static void DrawWreathToConsole(int left, int top)
        {
            var wreathLines = GetWreathLines();

            Console.SetCursorPosition(left, top);
            var generator = RandomNumberGenerator.Create();

            var ribbonColor = GoodColors[generator.GetBetterRandom(GoodColors.Count)];

            for (var i = 0; i < wreathLines.Count; i++)
            {
                var currentWreathLine = wreathLines[i];

                Console.SetCursorPosition(left, top + i);


                foreach (var currentChar in currentWreathLine)
                {
                    switch (currentChar)
                    {
                        case ',':
                        case ';':
                        case '.':
                        case '\'':
                            Console.ForegroundColor = generator.GetBetterRandom(255) > 128 ? ConsoleColor.DarkGreen : ConsoleColor.Green;
                            break;
                        case '/':
                        case '\\':
                        case '-':
                        case '_':
                        case '|':
                            Console.ForegroundColor = ribbonColor;
                            break;
                        case 'o':
                            Console.ForegroundColor = GoodColors[generator.GetBetterRandom(GoodColors.Count)];
                            break;
                    }

                    Console.Write(currentChar);
                    Console.ResetColor();
                }
            }
        }
    }
}