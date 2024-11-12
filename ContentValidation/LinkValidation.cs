namespace UtilityLibraries;

public class LinkValidation
{
    public (bool Result, string? ErrorMsg) CheckBrokenLink(string text)
    {
        var errorList = new List<string>();
        //TODO
        return (true, string.Join(",", errorList));
    }

        public (bool Result, string? ErrorMsg) CheckCrossLink(string text)
    {
        var errorList = new List<string>();
        //TODO
        return (true, string.Join(",", errorList));
    }
}
