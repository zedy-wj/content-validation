using System.Text.Json;

public class LocalDataItem
{
    public required string Type { get; set; }
    public required List<Rule> Rules { get; set; }

    public LocalDataItem(string type, List<Rule> rules)
    {
        Type = type;
        Rules = rules;
    }
}

public class Rule
{
    public required string RuleName { get; set; }
    public required int Count { get; set; }
    public required string LocalPath { get; set; }
    public string? FileUri { get; set; }

    public Rule(string ruleName, int count, string localPath)
    {
        RuleName = ruleName;
        Count = count;
        LocalPath = localPath;
        string htmlFilePath = Path.GetFullPath(localPath); 
        FileUri = new Uri(htmlFilePath).AbsoluteUri;
    }
}
public class LocalData
{
    public static List<LocalDataItem> Items { get; set; }

    static LocalData()
    {
        string filePath = "../../../localdata.json";

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        string jsonContent = File.ReadAllText(filePath);

        Items = JsonSerializer.Deserialize<List<LocalDataItem>>(jsonContent) ?? new List<LocalDataItem>();

        // foreach (var item in Items)
        // {
        //     Console.WriteLine(item.Type);
        //     foreach (var rule in item.Rules)
        //     {
        //         Console.WriteLine(rule.RuleName);
        //         Console.WriteLine(rule.Count);
        //         Console.WriteLine(rule.LocalPath);
        //         Console.WriteLine(rule.FileUri);
        //     }
        // }

    }
}