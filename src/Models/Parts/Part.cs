using System.Text;
using MimeKit;

namespace MaxMail.Models.Parts;

public enum PartType
{
    PlainText,
    Html,
    Css,
    Image,
    MiscAttachment,
    CalendarEvent,
    MiscText,
    Other
}


public abstract class Part
{

    private readonly MimeEntity _part;
    
    protected Part(MimeEntity part)
    {
        _part = part;
    }

    public abstract string Content
    {
        get;
    }

    public abstract string VisibleContent
    {
        get;
    }
    
    public static PartType GetPartType(MimeEntity part)
    {

        switch (part.ContentType.MimeType)
        {
            case "text/html":
                return PartType.Html;
            case "text/css":
                return PartType.Css;
            case "text/plain":
                return PartType.PlainText;
            case "text/calendar":
                return PartType.CalendarEvent;
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (part.ContentType.MediaType)
        {
            case "image":
                return PartType.Image;
            case "application":
                return PartType.MiscAttachment;
            case "text":
                return PartType.MiscText;
        }

        return PartType.Other;

    }

}