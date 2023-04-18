using MaxMail.Models;

namespace MaxMail.Filters.Conditions;

public abstract class Condition
{
    public abstract bool EvaluateMessage(Message message);

}

public class ConditionJsonModel
{
    private string type = "";
}
