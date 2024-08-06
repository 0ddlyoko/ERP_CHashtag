namespace Test.util;

using lib.util;

public class DependencyGraphTests
{
    [Fact]
    public void GetOrderedGraph_NoDependencies_ReturnsSingleNodeFirst()
    {
        var dependencies = new Dictionary<string, string[]>();
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);
        
        Assert.Empty(result);
    }

    [Fact]
    public void GetOrderedGraph_SingleDependency_ReturnsDependentNodeFirst()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B"] }
        };
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.Equal(2, result.Count);
        Assert.Equal("B", result[0]);
        Assert.Equal("A", result[1]);
    }

    [Fact]
    public void GetOrderedGraph_CircularDependency_ThrowsException()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B"] },
            { "B", ["A"] }
        };

        Assert.Throws<DependencyGraph.CircularDependencyException>(() => DependencyGraph.GetOrderedGraph(dependencies));
    }

    [Fact]
    public void GetOrderedGraph_ComplexDependencies_ReturnsOrderedGraph()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B", "C"] },
            { "B", ["C", "D"] },
            { "C", [] },
            { "D", ["E"] },
            { "E", [] }
        };
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.Equal(5, result.Count);
        Assert.Equivalent(new[] { "C", "E", "D", "B", "A" }, result);
    }
    
    [Fact]
    public void GetOrderedGraph_ImbricatedDependencies_ReturnsOrderedGraph()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B", "C"] },
            { "B", ["D"] },
            { "C", ["D", "E"] },
            { "D", ["E"] },
            { "E", [] }
        };
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.Equivalent(new[] { "E", "D", "B", "C", "A" }, result);
    }

    [Fact]
    public void GetOrderedGraph_NodesWithoutDependencies_ReturnsNodesFirst()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", [] },
            { "B", [] },
            { "C", ["A"] },
            { "D", ["B", "C"] },
            { "E", [] }
        };
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.Equivalent(new[] { "A", "B", "C", "E", "D" }, result);
    }

    [Fact]
    public void GetOrderedGraph_LargeNumberOfNodes_ReturnsOrderedGraph()
    {
        var dependencies = new Dictionary<string, string[]>();

        // Create 100 nodes with dependencies
        for (char c = 'A'; c <= 'Z'; c++)
        {
            var nodeName = c.ToString();
            var dependentNodes = new List<string>();
            for (char d = 'A'; d < c; d++)
            {
                dependentNodes.Add(d.ToString());
            }
            dependencies[nodeName] = dependentNodes.ToArray();
        }
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.Equal(26, result.Count); // Assuming we have 26 nodes from A to Z
        Assert.Equal("A", result[0]); // A should be the first node
        Assert.Equal("Z", result[25]); // Z should be the last node
    }

    [Fact]
    public void GetOrderedGraph_RepeatedDependencies_ReturnsOrderedGraph()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B", "C"] },
            { "B", ["C"] }, // Repeated dependency
            { "C", [] }
        };

        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.Equivalent(new[] { "C", "B", "A" }, result);
    }

    [Fact]
    public void GetOrderedGraph_MissingNodes_ReturnsOrderedGraph()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B"] },
            { "B", ["C"] },
            { "D", ["E"] } // Missing dependency
        };

        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.Equivalent(new[] { "C", "B", "A", "E", "D" }, result);
    }
}
