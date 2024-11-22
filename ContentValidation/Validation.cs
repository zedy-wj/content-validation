using System.Text;

namespace UtilityLibraries;
public interface IValidation{
    Task<TResult> Validate(string testLink);
}

public interface IValidationNew{
    Task<TResultNew> Validate(string testLink);
}

public class TResult
{
    public bool Result { get; set; }
    public string? ErrorMsg { get; set; }

    public Dictionary<string, List<Description>> ErrorManger { get; } 
    public void Add(string name, string title, string content)
    {
        if (!ErrorManger.ContainsKey(name))
        {
            ErrorManger[name] = new List<Description>();
        }

        ErrorManger[name].Add(new Description { Title = title, Content = content });
    }
    public string Display()
    {
        var sb = new StringBuilder();
        foreach (var error in ErrorManger)
        {
            sb.AppendLine($"{error.Key}");
            foreach (var desc in error.Value)
            {
                sb.AppendLine($"{"".PadLeft(10)}{desc.Title?.PadRight(15)}:       {desc.Content}");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public bool IsEmpty()
    {
        return ErrorManger.Count == 0;
    }


    public TResult()
    {
        Result = true;
        ErrorMsg = "";
        ErrorManger = new Dictionary<string, List<Description>>();
    }
}

public class TResultNew
{
    public bool Result { get; set; }
    public string? ErrorLink { get; set; }
    public string? ErrorInfo { get; set; }
    public int NumberOfOccurrences { get; set; }
    public List<string> LocationsOfErrors { get; set; }
    public object? AdditionalNotes { get; set; }

    public TResultNew()
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
Number of Occurrences: {NumberOfOccurrences}.
Locations of Errors: 
{string.Join("\n", LocationsOfErrors)}
";
    }
}


public class Description
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}

