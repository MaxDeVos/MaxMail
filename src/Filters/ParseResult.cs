namespace MaxMail.Filters;

public class ParseResult
{
    
    public bool WasSuccess;
    public int? ErrorLineNumber;
    public string? ErrorLineContents;

    public ParseResult(bool wasSuccess, string? message = "", string? errorLineContents = "", int? errorLineNumber = -1)
    {
        WasSuccess = wasSuccess;

        if (!WasSuccess)
        {
            ErrorLineContents = errorLineContents;
            ErrorLineNumber = errorLineNumber;
        }
        
    }
}