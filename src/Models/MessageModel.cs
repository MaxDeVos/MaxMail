using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using MimeKit;
using MimeKit.Text;

namespace MaxMail.Models;


public enum MessageComponent
{
    All,
    AllHeaders,
    Subject,
    To,
    From,
    ReplyTo,
    Body,
    Html,
    PlainText
}

public class MessageModel
{

    public MimeMessage message;
    private int _attachmentCount;

    public MessageModel(MimeMessage? message)
    {
        this.message = message ?? throw new ArgumentException($"Null Message");
    }

    private void AnalyzeParts()
    {
        foreach (var part in message.BodyParts)
        {
            Console.WriteLine(part.ToString());
            
            if (part.IsAttachment)
            {
                _attachmentCount++;
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

    public bool GetDoesReturnPathEqualFrom()
    {
        if (message.From.Count > 1)
        {
            return true;
        }
        
        if (message.Subject.Contains("unsubscribe", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.WriteLine($"FOUND UNSUBSCRIBE: {message.Subject}");
            return false;
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

    public enum ContentType
    {
        Plaintext, Html, Any
    }

    public bool Contains(string str, bool headers = false)
    {
        if (GetDecodedBodyContents().Contains(str))
        {
            return true;
        }

        if (!headers)
        {
            return false;
        }

        return message.Headers.Any(header => header.Value.Contains(str));
    }
    
    public string GetDecodedBodyContents(ContentType contentType = ContentType.Any)
    {

        bool doPlaintext = contentType != ContentType.Html;
        bool doHtml = contentType != ContentType.Plaintext;

        var output = "";
        
        foreach (var basePart in message.BodyParts)
        {

            if (basePart.ContentType.MediaType != "text")
            {
                continue;   
            }

            var part = (TextPart) basePart;
            var encoding = part.ContentType.CharsetEncoding ?? Encoding.Default;


            switch (part.Format)
            {
                case TextFormat.Plain:
                case TextFormat.Flowed:
                case TextFormat.Enriched:
                    if (doPlaintext)
                    {
                        output += part.GetText(encoding);
                    }
                    break;
                
                case TextFormat.Html:
                case TextFormat.RichText:
                case TextFormat.CompressedRichText:
                    if (doHtml)
                    {
                        output += part.GetText(encoding);
                    }
                    break;
                
                default:
                    continue;
            }

        }

        return output;

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