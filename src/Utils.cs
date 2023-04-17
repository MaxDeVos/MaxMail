using System.Text;
using System.Text.RegularExpressions;
using MailKit;
using MimeKit;

namespace MaxMail;

public class Utils
{
    public static void PrintFolders(IEnumerable<IMailFolder> folders)
    {
        foreach (var folder in folders)
        {
            Console.WriteLine ("[folder] {0}", folder.FullName);
            if (folder.GetSubfolders().Count > 0)
            {
                PrintFolders(folder.GetSubfolders());
            }
        }
    }
    
    public static string TrimString(string value, int maxLength)
    {
        return value[..Math.Min(value.Length, maxLength)];
    }
    
    public static void PrintParts(IEnumerable<MimeEntity> parts)
    {
        foreach (var part in parts)
        {
        
            if (part.ContentType.IsMimeType("multipart", "*"))
            {
                Console.WriteLine(part.ContentType);
                Console.WriteLine(part.ToString());
            }
        
            Console.WriteLine(part.ContentType);
        }
    }

    public static string SanitizeString(string subject, bool removeSpaces=false)
    {
        subject = Regex.Replace(subject, @"[^a-zA-Z 0-9]", "");

        if (removeSpaces)
        {
            subject = subject.Replace(" ", "");
        }

        return subject;

    }
    
    public static string GetDictionaryTallyFormattedString(Dictionary<string, int> frequencies)
    {
        var outLine = new StringBuilder();
        
        foreach (var pair in frequencies)
        {
            outLine.AppendLine($"{pair.Key},{pair.Value}");
        }

        return outLine.ToString();
    }

    public static Dictionary<string, int> AddDictionaries(Dictionary<string, int> a, Dictionary<string, int> b)
    {
        foreach (var pair in a)
        {
            if (b.ContainsKey(pair.Key))
            {
                b[pair.Key] += pair.Value;
            }
            else
            {
                b[pair.Key] = pair.Value;
            }
        }

        return b;
    }

}