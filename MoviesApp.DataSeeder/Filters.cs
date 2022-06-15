using System.Text.RegularExpressions;

namespace MoviesApp.DataSeeder
{
    public static class Filters
    {
        public static readonly Regex synopsisFilter = new Regex(@"[^0-9a-zA-Z.:;?!,'() -]");
        public static readonly Regex movieNameFilter = new Regex(@"[^A-Za-z0-9'(): -]");
        public static readonly Regex nameFilter = new Regex("[^A-Za-z'. ]");
        public static readonly Regex singleSpace = new Regex(@"\s+");
        public static readonly Regex year = new Regex(@"[^0-9]");
        public static readonly Regex rating = new Regex(@"[^0-9.]");
        public static readonly Regex genre = new Regex(@"[^a-zA-Z]");
    }
}