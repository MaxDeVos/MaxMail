using System.Diagnostics;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using MaxMail.Models.Parts;
using MimeKit;
using MimeKit.Text;

namespace MaxMail.Models;


public class Message
{
    
    public MimeMessage message;
    private int _attachmentCount;

    private List<BaseTextPart> _allTextParts;
    private List<BaseTextPart> _plainTextParts;
    private List<HtmlPart> _htmlParts;
    
    public Message(MimeMessage? message)
    {
        this.message = message ?? throw new ArgumentException($"Null Message");
        _htmlParts = new List<HtmlPart>();
        _plainTextParts = new List<BaseTextPart>();
        _allTextParts = new List<BaseTextPart>();
        AnalyzeParts();
    }

    public string BodyContent => _allTextParts.Aggregate("", (current, part) => current + part.Content);

    public string HtmlContent
    {
        get => _htmlParts.Aggregate("", (current, item) => $"{current}\n{item.Content}");
    }
    
    public string PlainTextContent 
    {
        get => _plainTextParts.Aggregate("", (current, item) => $"{current}\n{item.Content}");
    }
    
    public string VisibleText 
    {
        get
        {
            var visibleText = "";
            visibleText += _htmlParts.Aggregate("", (current, htmlPart) => current + htmlPart.VisibleContent);
            visibleText += _plainTextParts.Aggregate(visibleText, (current, textPart) => current + textPart.VisibleContent);
            return visibleText;
        }
    }

    
    public string HeaderValue
    {
        get => message.Headers.Aggregate("", (current, header) => current + $"{header.Value}\n");
    }
    
    public string HeaderBlockContent
    {
        get => message.Headers.Aggregate("", (current, header) => current + $"{header.Field}: {header.Value}\n");
    }

    public string AllContent
    {
        get => $"{HeaderValue}\n{BodyContent}";
    }    
    
    private void AnalyzeParts()
    {
        foreach (var part in message.BodyParts)
        {

            if (part.ContentType.MediaType == "text")
            {
                _allTextParts.Add(BaseTextPart.Create(part));
            }
            
            switch (Part.GetPartType(part))
            {
                case PartType.PlainText:
                    _plainTextParts.Add(BaseTextPart.Create(part));
                    break;
                case PartType.Html:
                    _htmlParts.Add(HtmlPart.Create(part));
                    break;
            }
        }
    }
    
    public bool HasAttachment()
    {
        return message.BodyParts.Any(part => part.IsAttachment);
    }

    private bool HasStyleTag()
    {
        var match1 = Regex.Match(message.Body.ToString(), "<style *= *.*>");
        var match2 = Regex.Match(message.Body.ToString(), "style *= *.*\".*\"");

        return match1.Length > 0 || match2.Length > 0;
    }

    public string SanitizedSubject(bool removeSpaces=false, int maxLength=100)
    {
        var sanitized = Utils.SanitizeString(message.Subject);
        return sanitized.Length > maxLength ? sanitized[..maxLength] : sanitized;
    }

    public string GetFirstFromAddress()
    {
        var rawFromValue = message.From[0].ToString();
        var fromAddressRegex = Regex.Match(rawFromValue, @"<(.*)>");

        return fromAddressRegex.Groups.Count > 1 ? fromAddressRegex.Groups[1].Value : rawFromValue;
    }

    public bool HasReturnToAddress()
    {
        return message.Headers.Contains(HeaderId.ReturnPath);
    }

    public string GetReturnToAddress()
    {
        var header = message.Headers.First(header => header.Field == "Return-Path");
        var returnPath = header.Value.Split("=")[^1];
        return returnPath.Replace("<", "").Replace(">", "");
    }

    public bool DoesMessageReplyToSender()
    {
        if (message.From.Count > 1)
        {
            return true;
        }
        
        if (!HasReturnToAddress())
        {
            return true;
        }

        return GetReturnToAddress() == GetFirstFromAddress();
    }
    
    public double GetSpamScore()
    {

        if (HasAttachment())
        {
            Console.WriteLine("FOUND ATTACHMENT");
            throw new ApplicationException("Cannot process spam score of large attachment");
        }
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "spamc",
                Arguments = "-R -learntype=spam",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var consoleOutput = "";
        
        try
        {
            
            var input = process.StandardInput;
            var output = process.StandardOutput;

            message.WriteTo(input.BaseStream);
            input.Flush();
            input.Close();
            consoleOutput = output.ReadToEnd();

            var isOutputStandard = double.TryParse(consoleOutput.Split("/")[0], out var spamScore);
            return isOutputStandard ? spamScore : 420;
        }
        catch (IOException exception)
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("============================FAILURE==============================");
            Console.WriteLine($"EXCEPTION parsing score of email from {message.From}:\nexception.Message");
            Console.WriteLine(exception.StackTrace);
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("[CONSOLE OUTPUT]");
            Console.WriteLine(consoleOutput);
            Console.WriteLine("=================================================================");
            Console.WriteLine("\n\n");
            
            return double.MaxValue;
        }
        

    }


    public string GetAllContentOfPartType(MessageComponent component)
    {
        switch (component)
        {
            case MessageComponent.HeaderBlock:
                return HeaderBlockContent;
            
            case MessageComponent.HeaderContent:
                return HeaderValue;
            
            case MessageComponent.Subject:
                return message.Subject;
            
            case MessageComponent.To:
                return message.To.Aggregate("", (current, address) => current + (address.Name + "\n"));
            
            case MessageComponent.From:
                return message.From.Aggregate("", (current, address) => current + (address.Name + "\n"));
            
            case MessageComponent.Body:
                return BodyContent;
           
            case MessageComponent.Html:
                return HtmlContent;
            
            case MessageComponent.PlainText:
                return PlainTextContent;
                
            case MessageComponent.VisibleText:
                return VisibleText; 
            
            case MessageComponent.All:
                return AllContent;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(component), component, null);
        }

        return "";

    }
    
    public bool Contains(string str, MessageComponent component = MessageComponent.All)
    {
        return Regex.IsMatch(GetAllContentOfPartType(component), str);
    }
    
    
    public void WritePartsToFiles(string mailbox, string basePath = @"C:\Workspace\C#\MaxMail\dataset\")
    {
        var partNum = 0;

        foreach (var basePart in message.BodyParts)
        {

            if (basePart.ContentType.MediaType != "text")
            {
                continue;   
            }
            
            var part = (TextPart) basePart;
            
            switch (part.Format)
            {
                case TextFormat.Plain:
                case TextFormat.Flowed:
                case TextFormat.Enriched:
                case TextFormat.RichText:
                case TextFormat.CompressedRichText:
                    File.WriteAllText($"{basePath}plaintext\\{mailbox}\\{SanitizedSubject()}-{partNum}.txt",
                        part.GetText(Encoding.UTF8), Encoding.UTF8);
                    break;
                
                case TextFormat.Html:
                    File.WriteAllText($"{basePath}html\\{mailbox}\\{SanitizedSubject()}-{partNum}.html",
                        part.GetText(Encoding.UTF8), Encoding.UTF8);
                    break;

                default:
                    Console.WriteLine($"FUCKED FORMAT ({part.ContentType.MimeType}): " +
                                      $"{SanitizedSubject()}");
                    break;
            }
            
            partNum++;
            
        }
    }

}
