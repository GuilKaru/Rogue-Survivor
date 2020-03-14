namespace RogueSurvivor.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string s, int maxLength)
        {
            return s.Length <= maxLength ? s : s.Substring(0, maxLength);
        }
    }
}
