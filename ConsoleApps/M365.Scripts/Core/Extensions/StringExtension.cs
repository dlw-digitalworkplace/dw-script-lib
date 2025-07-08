namespace Core.Extensions
{
    public static class StringExtension
    {
        public static int TryParseToInt(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new NullReferenceException("Try parse to int not working on null");
            }

            if(int.TryParse(input, out var result)){
                return result;
            }

            throw new NullReferenceException("No int value found");
        }

        /// <summary>
        /// Try to parse the input string to an int. If not possible a default value will be returned (0 or other value if passed as parameter).
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int TryParseToIntOrDefault(this string? input, int defaultValue = 0)
        {
            return int.TryParse(input, out int result) ? result : defaultValue;
        }

        public static TEnum TryParseToEnum<TEnum>(this string? input) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new NullReferenceException("Try parse to enum not working on null");
            }

            // Try parsing as integer
            if (int.TryParse(input, out int numericValue) && Enum.IsDefined(typeof(TEnum), numericValue))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);
            }

            // Try parsing as enum name
            if (Enum.TryParse<TEnum>(input, true, out var enumValue))
            {
                return enumValue;
            }

            throw new NullReferenceException("No enum value found");
        }
    }
}
