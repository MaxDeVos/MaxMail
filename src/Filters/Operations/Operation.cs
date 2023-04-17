using MaxMail.Filters.Conditions;
using MaxMail.Models;

namespace MaxMail.Filters.Operations;

public abstract class Operation
{

    public abstract void Execute(MessageModel messageModel);

    public abstract void ParseJson(string json);

}