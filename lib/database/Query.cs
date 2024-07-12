using lib.field;
using lib.model;

namespace lib.database;

public class Query
{
    
    /**
     * Transform the domain into a WHERE clause with corresponding LEFT JOIN & arguments, in order:
     * (domain (where), left join, arguments)
     * [('name', '=', "Test")]
     * [('name', '=', "Test"), ('age', '>=', 18)]
     * [('partner_id.name', '=', 'Test'), ('age', '>=', 18)]
     * [('partner_id.name', '=', 'Test'), ('partner_id.age', '>=', 18)]
     * ['|', ('partner_id.name', '=', 'Test'), ('partner_id.age', '>=', 18)]
     */
    public static (List<string>, List<string>, List<string>) DomainToQuery(Model model, List<object> domain)
    {
        // if (domain.Count == 0)
        // {
        //     return ([], [], []);
        // }
        List<string> wheres = [];
        // Left join: (path, table_name, table_alias)
        List<(string, string, string)> leftJoins = [];
        List<string> arguments = [];

        var enumerator = domain.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var result = HandleSingleDomain(enumerator.Current);
            wheres.Add(result.Item1);
            arguments.AddRange(result.Item2);
        }
        
        return (wheres, leftJoins, arguments);
        
        // Handle a single domain.
        // Returns (where, arguments)
        (string, List<string>) HandleSingleDomain(object singleDomain)
        {
            List<string> localArguments = [];
            if (singleDomain is char c)
                singleDomain = c.ToString();
            if (singleDomain is string str)
            {
                string separator;
                if (str == "|")
                    separator = " OR ";
                else if (str == "&")
                    separator = " AND ";
                else
                    throw new InvalidOperationException("Single string should only be \"|\" or \"&\"");
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("Invalid domain!");
                var firstResult = HandleSingleDomain(enumerator.Current);
                localArguments.AddRange(firstResult.Item2);
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("Invalid domain!");
                var secondResult = HandleSingleDomain(enumerator.Current);
                localArguments.AddRange(secondResult.Item2);
                return ($"({firstResult.Item1} {separator} {secondResult.Item2})", localArguments);
            }

            if (singleDomain is List<object> lst)
            {
                if (lst.Count != 3)
                    throw new InvalidOperationException($"Given list should be of length 3, not {lst.Count}. List: {lst}");
                if (lst[0] is not string arg1)
                    throw new InvalidOperationException($"First argument of a domain should be a string, but is {lst[0]}");
                if (lst[1] is not string arg2)
                    throw new InvalidOperationException($"Second argument of a domain should be a string, but is {lst[1]}");

                string[] path = arg1.Split('.');
                var currentModel = model.Env.PluginManager.GetFinalModel(model.ModelName);
                for (int i = 0; i < path.Length - 1; i++)
                {
                    currentModel.Fields.TryGetValue(path[i], out var finalField);
                    if (finalField == null)
                        throw new InvalidOperationException($"Invalid field {path[i]}: Field not found in model {currentModel.Name} for domain {singleDomain}");
                    if (finalField.FieldType is not FieldType.ManyToOne and not FieldType.OneToMany and not FieldType.ManyToMany)
                        throw new InvalidOperationException($"Field {path[i]} of model {currentModel.Name} used in domain {singleDomain} should be of type M2O, O2M or M2M");
                    
                }
            }

            throw new InvalidOperationException($"Invalid node: {singleDomain}. Must be a list, or '|', or '&'");
            // TODO Search on single domain
        }
    }
}
