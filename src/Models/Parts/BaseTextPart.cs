using System.Text;
using MimeKit;

namespace MaxMail.Models.Parts;

public class BaseTextPart : Part
{

    protected TextPart _textPart;

    public BaseTextPart(MimeEntity part) : base(part)
    {
        _textPart = (TextPart)part;
    }

    public override string VisibleContent => Content;

    public override string Content
    {
        get
        {
            var encoding = _textPart.ContentType.CharsetEncoding ?? Encoding.Default;
            return _textPart.GetText(encoding);
        }
    }
    
    public static BaseTextPart Create(MimeEntity mimePart)
    {
        return new BaseTextPart(mimePart);
    }

}