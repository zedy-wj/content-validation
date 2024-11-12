namespace UtilityLibraries;

public class AnnotationValidation
{
    public (bool Result, string? ErrorMsg) FindMissingTypeAnnotation(string text)
    {
        var errorList = new List<string>();
        //TODO
        return (true, string.Join(",", errorList));
    }
}
