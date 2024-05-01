namespace lib.util;

public static class DependencyGraph
{
    /**
     * Transform given dependencies nodes into a single list containing the node without dependencies at first, and
     * node with the most dependencies at last.
     */
    public static List<string> GetOrderedGraph(IReadOnlyDictionary<string, string[]> dependencies)
    {
        var result = new List<string>();
        var marked = new Dictionary<string, bool>();

        foreach (var key in dependencies.Keys)
            marked[key] = false;

        foreach (var key in dependencies.Keys)
            Visit(result, marked, dependencies, key);

        return result;
    }

    private static void Visit(IList<string> result, IDictionary<string, bool> marked, IReadOnlyDictionary<string, string[]> allDependencies, string currentNode)
    {
        if (!marked.TryGetValue(currentNode, out bool value))
        {
            // Key is not present in the list. Ignore it
            result.Insert(0, currentNode);
            return;
        }
        if (value)
            throw new InvalidOperationException("Recursive node !");

        if (result.Contains(currentNode))
            return;
        marked[currentNode] = true;

        if (allDependencies.TryGetValue(currentNode, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                Visit(result, marked, allDependencies, dependency);
            }
        }

        marked[currentNode] = false;
        result.Insert(0, currentNode);
    }
}
