using MaxMail.Models;

namespace MaxMail.Filters.Conditions;

public abstract class Condition
{
    public abstract bool EvaluateMessage(MessageModel messageModel);

}

public class ConditionJsonModel
{
    private string type = "";
}
