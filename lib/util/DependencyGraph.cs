namespace lib.util;

public static class DependencyGraph
{
    /**
     * Transform given dependencies nodes into a single list containing the node without dependencies at first, and
     * node with the most dependencies at last.
     * If a dependency is not in the list, it will be added.
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
            // Key is not present in the list, but we still add it to the result
            result.Add(currentNode);
            return;
        }
        if (value)
            throw new CircularDependencyException($"Circular dependency detected involving node: {currentNode}");

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
        result.Add(currentNode);
    }
    
    public class CircularDependencyException : Exception
    {
        public CircularDependencyException() { }
        public CircularDependencyException(string message) : base(message) { }
        public CircularDependencyException(string message, Exception inner) : base(message, inner) { }
    }
}
