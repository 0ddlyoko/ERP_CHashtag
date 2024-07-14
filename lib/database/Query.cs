using System.Runtime.CompilerServices;
using lib.field;
using lib.model;

namespace lib.database;

public class Query
{
    
    /**
     * Transform the domain into a query, and return it
     * [('Name', '=', "Test")]
     * [('Name', '=', "Test"), ('age', '>=', 18)]
     * [('PartnerId.Name', '=', 'Test'), ('Age', '>=', 18)]
     * [('PartnerId.Name', '=', 'Test'), ('PartnerId.age', '>=', 18)]
     * ['|', ('PartnerId.Name', '=', 'Test'), ('PartnerId.Age', '>=', 18)]
     */
    public static DomainQuery DomainToQuery(FinalModel finalModel, List<object> domain)
    {
        DomainQuery result = new();

        var enumerator = domain.GetEnumerator();
        while (enumerator.MoveNext())
        {
            result.AddWhere(HandleSingleDomain(enumerator.Current));
        }
        
        return result;
        
        // Handle a single domain.
        // Returns "where"
        string HandleSingleDomain(object singleDomain)
        {
            if (singleDomain is char c)
                singleDomain = c.ToString();
            if (singleDomain is string str)
            {
                string separator;
                if (str == "|")
                    separator = "OR";
                else if (str == "&")
                    separator = "AND";
                else
                    throw new InvalidOperationException("Single string should only be \"|\" or \"&\"");
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("Invalid domain!");
                var firstResult = HandleSingleDomain(enumerator.Current);
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("Invalid domain!");
                var secondResult = HandleSingleDomain(enumerator.Current);
                return $"({firstResult} {separator} {secondResult})";
            }
            
            if (singleDomain is ITuple tuple)
            {
                if (tuple.Length != 3)
                    throw new InvalidOperationException($"Given domain should be of length 3, not {tuple.Length}. Domain: {tuple}");
                if (tuple[0] is not string arg1)
                    throw new InvalidOperationException($"First argument of a domain should be a string, but is {tuple[0]}");
                if (tuple[1] is not string arg2)
                    throw new InvalidOperationException($"Second argument of a domain should be a string, but is {tuple[1]}");

                var currentModel = finalModel;
                var currentModelPath = finalModel.Name;
                List<string> paths = [currentModelPath];
                var split = arg1.Split('.');
                var useId = false;
                for (int i = 0; i < split.Length; i++)
                {
                    var currentPath = split[i];
                    paths.Add(currentPath);
                    currentModel.Fields.TryGetValue(currentPath, out var currentField);
                    if (currentField == null)
                        throw new InvalidOperationException($"Invalid field {currentPath}: Field not found in model {currentModel.Name} for domain {singleDomain}");
                    if (i == split.Length - 1)
                    {
                        if (currentField.FieldType is FieldType.OneToMany or FieldType.ManyToMany)
                            useId = true;
                        else
                            break;
                    }
                    if (currentField.FieldType is not FieldType.ManyToOne and not FieldType.OneToMany and not FieldType.ManyToMany)
                        throw new InvalidOperationException($"Field {currentPath} of model {currentModel.Name} used in domain {singleDomain} should be of type M2O, O2M or M2M");
                    var targetModel = currentField.TargetFinalModel;
                    if (targetModel == null)
                        throw new InvalidOperationException($"Field {currentPath} of model {currentModel.Name} used in domain {singleDomain} should target a model, but target nothing");

                    var fullPath = string.Join('.', paths);
                    if (result.HasLeftJoin(fullPath))
                    {
                        currentModel = targetModel;
                        currentModelPath = fullPath;
                        continue;
                    }
                    // ManyToOne: currentModel contains the link to targetModel
                    // OneToMany: targetModel contains the link to currentModel
                    // ManyToMany: currentModel & targetModel contains a link to an intermediate table
                    if (currentField.FieldType is FieldType.ManyToOne)
                    {
                        // LEFT JOIN "targetModel" AS "alias" ON "currentModel"."finalField" = "alias"."id"
                        result.AddLeftJoin(fullPath, $"LEFT JOIN \"{targetModel.Name}\" AS \"{fullPath}\" ON \"{currentModelPath}\".\"{currentField.FieldName}\" = \"{fullPath}\".\"id\"");
                    }
                    else if (currentField.FieldType is FieldType.OneToMany)
                    {
                        // LEFT JOIN "targetModel" as "alias" ON "currentModel"."id" = "alias"."finalField"
                        if (currentField.TargetField == null)
                            throw new InvalidOperationException($"Field {currentPath} of model {currentModel.Name} used in domain {singleDomain} should target a field, but target nothing");
                        result.AddLeftJoin(fullPath, $"LEFT JOIN \"{targetModel.Name}\" AS \"{fullPath}\" ON \"{currentModelPath}\".\"id\" = \"{fullPath}\".\"{currentField.TargetField}\"");
                    }
                    else if (currentField.FieldType is FieldType.ManyToMany)
                    {
                        // LEFT JOIN "targetModel" as "alias"
                        // TODO Handle ManyToMany
                    }
                    currentModel = targetModel;
                    currentModelPath = fullPath;
                }
                
                // Add domain
                var finalFieldName = useId ? "Id" : currentModel.Fields[arg1.Split('.').Last()].FieldName;
                var operation = arg2 switch
                {
                    "=" => "=",
                    "!=" => "!=",
                    ">" => ">",
                    "<" => "<",
                    ">=" => ">=",
                    "<=" => "<=",
                    "like" => "LIKE",
                    "ilike" => "ILIKE",
                    "in" => "IN",
                    _ => throw new InvalidOperationException($"Invalid operator: {arg2}")
                };
                var nbrOfArgs = result.Arguments.Count + 1;
                result.AddArgument(tuple[2]);

                return $"(\"{currentModelPath}\".\"{finalFieldName}\" {operation} ${nbrOfArgs})";
            }

            throw new InvalidOperationException($"Invalid node: {singleDomain}. Must be a list, or '|', or '&'");
        }
    }

    public class DomainQuery
    {
        public string Where => $"({string.Join(" AND ", _where)})";
        private readonly List<string> _where = [];
        public readonly List<object> Arguments = [];
        // Left join: query
        public readonly List<string> LeftJoins = [];
        // Left join: path
        private readonly List<string> _leftJoins = [];

        public void AddWhere(string where)
        {
            _where.Add(where);
        }

        public void AddArgument(object arg)
        {
            Arguments.Add(arg);
        }

        public bool HasLeftJoin(string path) => _leftJoins.Contains(path);

        public void AddLeftJoin(string path, string query)
        {
            _leftJoins.Add(path);
            LeftJoins.Add(query);
        }
    }
}
