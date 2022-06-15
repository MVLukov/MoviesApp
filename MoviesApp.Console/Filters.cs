using System.Text.RegularExpressions;

namespace MoviesApp.Console;

public static class Filter
{
    public static readonly Regex name = new Regex(@"[^A-Za-z'. ]");  //name filter
    public static readonly Regex lettersOnly = new Regex("[^A-Za-z]"); //letters filter
    public static readonly Regex numbersOnly = new Regex("[^0-9]");    //numbers filter
    public static readonly Regex rating = new Regex("[^0-9.]");    //rating filter
    public static readonly Regex singleSpace = new Regex(@"\s+");  //all available commands
    public static readonly Regex movieSynopsis = new Regex(@"[^0-9a-zA-Z.:;?!,'() -]"); //synopsis filter
    public static readonly Regex movieName = new Regex(@"[^A-Za-z0-9'(): -]");    //name filter
}