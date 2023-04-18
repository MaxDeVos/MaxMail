using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MaxMail.Models;
using MaxMail.Models.Parts;
using Org.BouncyCastle.Crypto.Engines;

namespace MaxMail.Filters.Conditions;

public class RegexCondition: Condition
{
    
    [JsonIgnore]
    private MessageComponent _part;
    
    [JsonIgnore] 
    private bool _parsed;

    private string Name;
    private string Pattern;
    private string Part;
    
    
    public override bool EvaluateMessage(Message message)
    {
        if (!_parsed)
        {
            Console.WriteLine($"Skipping Rule {Name} because it wasn't parsed");
            return false;
        }

        throw new NotImplementedException("try again next week");

    }

    public ParseResult ParseJsonModel(int lineNum)
    {
        var success = MessageComponent.TryParse(Part, true, out _part);
        if (!success)
        {
            return new ParseResult(false, "Unable to parse part type", Part, lineNum);
        }

        try
        {
            var isMatch = Regex.IsMatch("", Pattern);

            if (isMatch)
            {
                return new ParseResult(false,"Regex Pattern Always Matches!", Pattern, lineNum);
            }
            
        }
        catch (ArgumentException)
        {
            return new ParseResult(false,"Invalid Regex Pattern", Pattern, lineNum);
        }

        _parsed = true;
        return new ParseResult(true);

    }
}

