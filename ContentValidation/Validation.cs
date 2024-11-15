namespace UtilityLibraries;
public interface IValidation{
    Task<TResult> Validate(string testLink);
}

public class TResult
{
    public bool Result { get; set; }
    public string? ErrorMsg { get; set; }

    public TResult(){
        Result = true;
        ErrorMsg = "";
    }
}