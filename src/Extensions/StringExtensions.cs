namespace RogueSurvivor.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string s, int maxLength)
        {
            return s.Length <= maxLength ? s : s.Substring(0, maxLength);
        }

        public static string Capitalize(this string text)
        {
            if (text == null)
                return "";
            if (text.Length == 1)
                return string.Format("{0}", char.ToUpper(text[0]));

            return string.Format("{0}{1}", char.ToUpper(text[0]), text.Substring(1));
        }
    }
}
