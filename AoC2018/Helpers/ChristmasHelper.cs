using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using AoC2018.Extensions;

namespace AoC2018.Helpers
{
    public static class ChristmasHelper
    {
        private static IList<ConsoleColor> GoodColors
        {
            get
            {
                return new List<ConsoleColor>
                {
                    ConsoleColor.Red,
                    ConsoleColor.Blue,
                    ConsoleColor.Cyan,
                    ConsoleColor.Magenta,
                    ConsoleColor.Yellow,
                    ConsoleColor.Green
                };
            }
        }
        // from here: https://asciiart.website//index.php?art=holiday/christmas/other

        public static IList<string> GetWreathLines()
        {
            return new List<string>
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
        }

        public static void DrawWreathRibbonToConsole(int left, int top, int padding = 5)
        {
            var lenghtPerWreath = GetWreathLines().Max(f => f.Length) + padding;

            var wreathCount = Math.Abs(Console.BufferWidth / lenghtPerWreath);

            for (var i = 0; i < wreathCount; i++) DrawWreathToConsole(i * lenghtPerWreath, top);
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