namespace UtilityLibraries;
public interface IValidation{
    Task<TResult> Validate(string testLink);
}

public class TResult
{
    public bool Result { get; set; }
    public string? ErrorLink { get; set; }
    public string? ErrorInfo { get; set; }
    public int NumberOfOccurrences { get; set; }
    public List<string> LocationsOfErrors { get; set; }
    public object? AdditionalNotes { get; set; }

    public TResult()
    {
        Result = true;
        ErrorLink = "";
        ErrorInfo = "";
        NumberOfOccurrences = 0;
        LocationsOfErrors = new List<string>();
    }

    public string FormatErrorMessage(){
        return $@"
    Error Link: {ErrorLink}.
    Error Info: {ErrorInfo}.
    Number of Occurrences: {NumberOfOccurrences}." + ((LocationsOfErrors.Count == 0) ? "\n" : $@"
    Locations of Errors: 
    {string.Join("\n\t", LocationsOfErrors)}
");
    }
}
