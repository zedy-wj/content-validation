using System.Text;

namespace UtilityLibraries;
public interface IValidation{
    Task<TResult> Validate(string testLink);
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


public class Description
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}

