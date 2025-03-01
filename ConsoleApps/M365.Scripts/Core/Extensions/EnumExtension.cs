using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Core.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// Generate a menu of all enum values available using the Display -> Name Datanotation to display the option for each enum (or the enum text value as fallback).
        /// Navigation within the menu can be done using the arrow keys or a number
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="menuTitle"></param>
        /// <returns></returns>
        public static T PrintMenu<T>(string menuTitle) where T : Enum
        {
            var enumValues = (T[])Enum.GetValues(typeof(T));
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine(menuTitle);
                Console.WriteLine("Use the arrow keys to navigate or type a number and press enter to jump. Press Enter to confirm the selected item.");

                for (int i = 0; i < enumValues.Length; i++)
                {
                    string displayName = GetEnumDisplayName(enumValues[i]);

                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                    }

                    Console.WriteLine($"{i}: {displayName}");

                    Console.ResetColor();
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex == 0) ? enumValues.Length - 1 : selectedIndex - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex == enumValues.Length - 1) ? 0 : selectedIndex + 1;
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    string input = keyInfo.KeyChar.ToString();
                    Console.Write(input); // Show the typed number

                    while (true)
                    {
                        keyInfo = Console.ReadKey(true);

                        if (keyInfo.Key == ConsoleKey.Enter) break; // Finish input on Enter

                        if (char.IsDigit(keyInfo.KeyChar))
                        {
                            input += keyInfo.KeyChar;
                            Console.Write(keyInfo.KeyChar); // Show additional typed number
                        }
                        else if (keyInfo.Key == ConsoleKey.Backspace && input.Length > 0)
                        {
                            input = input[..^1]; // Remove last character
                            Console.Write("\b \b"); // Remove from console display
                        }
                    }

                    if (int.TryParse(input, out int enteredIndex) && enteredIndex >= 0 && enteredIndex < enumValues.Length)
                    {
                        selectedIndex = enteredIndex;
                    }
                }

            } while (key != ConsoleKey.Enter);

            Console.Clear();
            Console.WriteLine($"You selected: {GetEnumDisplayName(enumValues[selectedIndex])}");

            return enumValues[selectedIndex];
        }



        private static string GetEnumDisplayName<T>(T enumValue) where T : Enum
        {
            var fieldInfo = typeof(T).GetField(enumValue.ToString());
            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
