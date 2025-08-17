using System.Text;
using System;
using EvergreenRanch.Models.Common;

namespace EvergreenRanch.Utilities
{
    public static class RandomisorExtension
    {
        private static readonly Random random = new Random();

        public static string GenerateRandom(int length, RandomCharType charTypes, bool addAll = false)
        {
            if (length <= 0 || charTypes == RandomCharType.None)
                throw new ArgumentException("Invalid parameters for GenerateRandom()");

            var charGroups = new Dictionary<RandomCharType, string>();

            if (charTypes.HasFlag(RandomCharType.Uppercase))
                charGroups[RandomCharType.Uppercase] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (charTypes.HasFlag(RandomCharType.Lowercase))
                charGroups[RandomCharType.Lowercase] = "abcdefghijklmnopqrstuvwxyz";

            if (charTypes.HasFlag(RandomCharType.Digit))
                charGroups[RandomCharType.Digit] = "0123456789";

            if (charTypes.HasFlag(RandomCharType.Special))
                charGroups[RandomCharType.Special] = "!@#$%^&*()_-+=<>?";

            if (charGroups.Count == 0)
                throw new InvalidOperationException("No valid character sets selected.");

            var allChars = string.Concat(charGroups.Values);
            var resultChars = new List<char>();

            // Ensure at least one character from each selected type if requested
            if (addAll)
            {
                foreach (var group in charGroups.Values)
                {
                    resultChars.Add(group[random.Next(group.Length)]);
                }
            }

            // Fill the rest of the length
            while (resultChars.Count < length)
            {
                resultChars.Add(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle to avoid predictable order
            return new string(resultChars.OrderBy(_ => random.Next()).ToArray());
        }

        public static int GenerateNumber(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
