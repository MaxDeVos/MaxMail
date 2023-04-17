using MimeKit;

namespace MaxMail.Models;

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


public class Part
{

    protected MimeEntity _part;
    private string _content;
    
    public Part(MimeEntity part)
    {
        _part = part;
    }

    public PartType GetPartType()
    {

        switch (_part.ContentType.MimeType)
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
        switch (_part.ContentType.MediaType)
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