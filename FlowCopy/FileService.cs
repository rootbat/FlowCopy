using System.Text;
using System.IO;

public class FileService
{
    public Dictionary<string, string> ReadFileAndFillDictionary(string filePath)
    {
        var dictionary = new Dictionary<string, string>();
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            int commaIndex = line.IndexOf(',');
            if (commaIndex != -1)
            {
                string tag = line.Substring(0, commaIndex);
                string content = line.Substring(commaIndex + 1);
                dictionary[tag] = content;
            }
        }
        return dictionary;
    }

    public void SaveDictionaryAsCsv(Dictionary<string, string> dictionary, string filePath)
    {
        StringBuilder csvContent = new StringBuilder();
        foreach (var pair in dictionary)
        {
            csvContent.AppendLine($"{pair.Key},{pair.Value}");
        }
        File.WriteAllText(filePath, csvContent.ToString());
    }
}
