using MaxMail.Filters.Conditions;
using MaxMail.Models;

namespace MaxMail.Filters.Operations;

public abstract class Operation
{

    public abstract void Execute(Message message);

    public abstract void ParseJson(string json);

}