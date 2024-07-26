using System.Xml;
using NUnit.Engine;

namespace Test;

public class DefaultTestEventListener: ITestEventListener
{
    public void OnTestEvent(string report)
    {
        using (var stringReader = new StringReader(report))
        using (var reader = XmlReader.Create(stringReader))
        {
            // Go to starting point
            reader.MoveToContent();

            if (reader.NodeType != XmlNodeType.Element)
                throw new InvalidOperationException("Expected to find root element");

            var name = reader.Name;
            switch (name)
            {
                // case "start-test":
                //     Console.WriteLine("start-test!");
                //     break;
                // case "start-suite":
                //     Console.WriteLine("start-suite!");
                //     break;
                // case "test-case":
                //     Console.WriteLine("test-case!");
                //     break;
                // case "test-suite":
                //     Console.WriteLine("test-suite!");
                //     break;
                default:
                    Console.WriteLine($"!{name}!");
                    break;
            }
        }
    }
}
