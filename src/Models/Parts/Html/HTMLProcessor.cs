using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace MaxMail.Models.Parts.Html;

public class HtmlProcessor
{

    private string _rawHtml;
    private HtmlDocument _document;
    
    public HtmlProcessor(string htmlContent)
    {
        _document = new HtmlDocument();
        _document.Load(htmlContent);
        _rawHtml = _document.Text;
    }

    public IEnumerable<string> GetVisibleTextObjects()
    {
        return _document.DocumentNode.Descendants().
            Where(n =>
                n.NodeType == HtmlNodeType.Text &&
                n.ParentNode.Name != "script" &&
                n.ParentNode.Name != "style"
            ).Select(n => n.InnerText.ToLower());
    }

    public string GetVisibleText()
    {
        return GetVisibleTextObjects().Aggregate((s, s1) => $"{s} {s1}");
    }

    public int GetVisibleTextLength()
    {
        try
        {
            var allText = GetVisibleTextObjects().Aggregate((a,b) => $"{a} {b}");
            return allText.Length;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    public int GetTotalTextMinusLinksLength()
    {
        var temp = _rawHtml;
        temp = Regex.Replace(temp, "href=[\'\"].*[\"\']", "href=\"\"");
        temp = Regex.Replace(temp, "src=[\'\"].*[\"\']", "src=\"\"");
        return temp.Length;
    }

    public int GetTotalTextLength()
    {
        return _rawHtml.Length;
    }

    public double GetPercentVisibleText()
    {
        return GetVisibleTextLength() / (double) GetTotalTextMinusLinksLength();
    }
    
    public Dictionary<string, int> GetClassFrequencies()
    {
        var classes = new Dictionary<string, int>();
        foreach (var node in _document.DocumentNode.Descendants())
        {
            foreach (var className in node.GetClasses())
            {
                if (classes.ContainsKey(className))
                {
                    classes[className] += 1;
                }
                else
                {
                    classes[className] = 1;
                }
                    
            }
        }

        return classes;
    }
    
    public Dictionary<string, int> GetIdFrequencies()
    {
        var classes = new Dictionary<string, int>();
        foreach (var node in _document.DocumentNode.Descendants())
        {
            if (classes.ContainsKey(node.Id))
            {
                classes[node.Id] += 1;
            }
            else
            {
                classes[node.Id] = 1;
            }
        }

        return classes;
    }
    
}