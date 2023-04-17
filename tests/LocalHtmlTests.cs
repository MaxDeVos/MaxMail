using System.Text;

namespace MaxMail.tests;

public class LocalHtmlTests
{
    void RunLocalHTMLTests(string mailbox, string basePath = @$"C:\Workspace\C#\MaxMail\dataset\html\")
    {
        
        var path = $"{basePath}{mailbox}\\";
        var sumVisibleText = 0;
        var sumTotalText = 0;

        var classNamesDict = new Dictionary<string, int>();
        var idDict = new Dictionary<string, int>();

        var percentVisibleOutPath = $"{basePath}{mailbox}-PercentVis-{DateTime.Now.ToFileTimeUtc()}.csv";
        var writer = File.AppendText(percentVisibleOutPath);

        foreach (var file in Directory.EnumerateFiles($"{path}"))
        {
            var filename = file.Replace(path, "");
            var processor = new HtmlProcessor(file);
        
            classNamesDict = Utils.AddDictionaries(classNamesDict, processor.GetClassFrequencies());
            idDict = Utils.AddDictionaries(idDict, processor.GetIdFrequencies());
            
            sumVisibleText += processor.GetVisibleTextLength();
            sumTotalText += processor.GetTotalTextMinusLinksLength();

            var lineOut = $"{Utils.SanitizeString(filename)},{processor.GetPercentVisibleText()}";
            writer.WriteLine(lineOut);
        
        }

        writer.Flush();
        writer.Close();

        var classNamesOutPath = $"{basePath}{mailbox}-ClassNames-{DateTime.Now.ToFileTimeUtc()}.csv";
        File.WriteAllText(classNamesOutPath, Utils.GetDictionaryTallyFormattedString(classNamesDict), Encoding.UTF8);
    
        var idsOutPath = $"{basePath}{mailbox}-IDs-{DateTime.Now.ToFileTimeUtc()}.csv";
        File.WriteAllText(idsOutPath, Utils.GetDictionaryTallyFormattedString(idDict), Encoding.UTF8);
    }
}