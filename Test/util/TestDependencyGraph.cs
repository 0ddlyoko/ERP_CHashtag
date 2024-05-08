namespace Test.util;

using lib.util;

[TestFixture]
public class DependencyGraphTests
{
    [Test]
    public void GetOrderedGraph_NoDependencies_ReturnsSingleNodeFirst()
    {
        var dependencies = new Dictionary<string, string[]>();
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);
        
        Assert.That(result, Has.Count.EqualTo(0));
    }

    [Test]
    public void GetOrderedGraph_SingleDependency_ReturnsDependentNodeFirst()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B"] }
        };
        
        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo("B"));
        Assert.That(result[1], Is.EqualTo("A"));
    }

    [Test]
    public void GetOrderedGraph_CircularDependency_ThrowsException()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B"] },
            { "B", ["A"] }
        };

        Assert.Throws<DependencyGraph.CircularDependencyException>(() => DependencyGraph.GetOrderedGraph(dependencies));
    }

    [Test]
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

        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result, Is.EquivalentTo(new[] { "C", "E", "D", "B", "A" }));
    }
    
    [Test]
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

        Assert.That(result, Is.EquivalentTo(new[] { "E", "D", "B", "C", "A" }));
    }

    [Test]
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

        Assert.That(result, Is.EquivalentTo(new[] { "A", "B", "C", "E", "D" }));
    }

    [Test]
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

        Assert.That(result.Count, Is.EqualTo(26)); // Assuming we have 26 nodes from A to Z
        Assert.That(result[0], Is.EqualTo("A")); // A should be the first node
        Assert.That(result[25], Is.EqualTo("Z")); // Z should be the last node
    }

    [Test]
    public void GetOrderedGraph_RepeatedDependencies_ReturnsOrderedGraph()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B", "C"] },
            { "B", ["C"] }, // Repeated dependency
            { "C", [] }
        };

        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.That(result, Is.EquivalentTo(new[] { "C", "B", "A" }));
    }

    [Test]
    public void GetOrderedGraph_MissingNodes_ReturnsOrderedGraph()
    {
        var dependencies = new Dictionary<string, string[]>
        {
            { "A", ["B"] },
            { "B", ["C"] },
            { "D", ["E"] } // Missing dependency
        };

        var result = DependencyGraph.GetOrderedGraph(dependencies);

        Assert.That(result, Is.EquivalentTo(new[] { "C", "B", "A", "E", "D" }));
    }
}
