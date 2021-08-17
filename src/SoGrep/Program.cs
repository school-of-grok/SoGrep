using System;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console;

namespace SoGrep
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.Error.WriteLine("Usage: sogrep pattern [file]");
                return 1;
            }

            var pattern = args[0];

            if (args.Length == 2)
            {
                var fileName = args[1];

                if (!File.Exists(fileName))
                {
                    Console.Error.WriteLine($"File '{fileName}' not found.");
                    return 2;
                }

                await FindPatternInFileAsync(pattern, fileName);

                Console.WriteLine();

                return 0;
            }
            else
            {
                var currentDirectory = Environment.CurrentDirectory;

                await FindPatternInDirectoryAsync(pattern, currentDirectory);

                Console.WriteLine();

                return 0;
            }
        }

        private static async Task FindPatternInDirectoryAsync(string pattern, string directory)
        {
            var files = Directory.EnumerateFiles(directory);

            foreach (var file in files)
            {
                await FindPatternInFileAsync(pattern, file, true);
            }

            var subDirectories = Directory.EnumerateDirectories(directory);
            
            foreach (var subDirectory in subDirectories)
            {
                await FindPatternInDirectoryAsync(pattern, subDirectory);
            }
        }

        private static async Task FindPatternInFileAsync(string pattern, string fileName, bool showFileName = false)
        {
            using var reader = File.OpenText(fileName);

            int lineNumber = 0;

            while (!reader.EndOfStream)
            {
                lineNumber++;

                var line = await reader.ReadLineAsync();

                if (IsBinary(line))
                {
                    break;
                }

                if (line.Contains(pattern))
                {
                    if (showFileName)
                    {
                        var displayName = Path.GetRelativePath(Environment.CurrentDirectory, fileName);
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"[deepskyblue1]{displayName}:[/]");
                        showFileName = false;
                    }

                    line = line.Replace("[", "[[")
                        .Replace("]", "]]")
                        .Replace(pattern, $"[red]{pattern}[/]");

                    AnsiConsole.MarkupLine($"[darkseagreen4_1]{lineNumber}:[/]{line}");
                }
            }
        }

        private static bool IsBinary(ReadOnlySpan<char> line)
        {
            while (line.Length > 0)
            {
                // If the character is below Space in the ASCII table
                if (line[0] < SPACE)
                {
                    // Except for Tab, Newline and Carriage Return
                    if (line[0] is not TAB or LF or CR)
                    {
                        // Probably a binary file
                        return true;
                    }
                }
                line = line.Slice(1);
            }
            return false;
        }

        private const char TAB = (char)9;
        private const char LF = (char)10;
        private const char CR = (char)13;
        private const char SPACE = (char)32;
    }
}
