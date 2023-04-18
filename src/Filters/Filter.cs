using MaxMail.Filters.Conditions;
using MaxMail.Filters.Operations;
using MaxMail.Models;

namespace MaxMail.Filters;

public class Filter
{
    public enum MatchType
    {
        Any, All
    }

    private readonly MatchType _matchType;
    private readonly List<Condition> _conditions;
    private readonly List<Operation> _operations;

    protected Filter(MatchType matchType)
    {
        _matchType = matchType;
        _conditions = new List<Condition>();
        _operations = new List<Operation>();
    }

    public void AddCondition(Condition condition)
    {
        _conditions.Add(condition);
        
    }
    
    public void AddOperation(Operation operation)
    {
        _operations.Add(operation);
    }

    private bool EvaluateMessage(Message message)
    {
        switch (_matchType)
        {
            case MatchType.Any:     // If any condition evaluates as true, return true
                foreach (var condition in _conditions)
                {
                    if (condition.EvaluateMessage(message))
                    {
                        return true;
                    }
                }
                return false;
            
            
            case MatchType.All:     // If any condition evaluates as false, return false
                foreach (var condition in _conditions)
                {
                    if (!condition.EvaluateMessage(message))
                    {
                        return false;
                    }
                }
                return true;
        }

        Console.WriteLine("Illegal State! MatchType isn't ANY or ALL!");
        return true;
    }

    private void ExecuteOperations(Message message)
    {
        foreach (var operation in _operations)
        {
            operation.Execute(message);
        }
    }
}

public class FilterJsonModel
{
    public string match_any_or_all = "";
}