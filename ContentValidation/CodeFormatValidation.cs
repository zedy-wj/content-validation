namespace UtilityLibraries;

public class CodeFormatValidation
{
    public (bool Result, string? ErrorMsg) CheckJsonFormat(string text)
    {
        var errorList = new List<string>();
        //TODO
        return (true, string.Join(",", errorList));
    }
}
