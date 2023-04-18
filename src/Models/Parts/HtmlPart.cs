using System.Reflection.Metadata;
using System.Text;
using MaxMail.Models.Parts.Html;
using MimeKit;

namespace MaxMail.Models.Parts;

public class HtmlPart : BaseTextPart
{

    protected TextPart _textPart;
    private HtmlProcessor _htmlProcessor;
    
    public HtmlPart(MimeEntity part) : base(part)
    {
        _textPart = (TextPart)part;
        _htmlProcessor = new HtmlProcessor(Content);
    }

    public override string VisibleContent => _htmlProcessor.GetVisibleText();

    public override string Content
    {
        get
        {
            var encoding = _textPart.ContentType.CharsetEncoding ?? Encoding.Default;
            return _textPart.GetText(encoding);
        }
    }
    
    public static HtmlPart Create(MimeEntity mimePart)
    {
        return new HtmlPart(mimePart);
    }
}