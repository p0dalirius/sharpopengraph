using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpOpenGraph;

namespace SharpOpenGraph.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the OpenGraph classes
    /// to recreate the minimal working JSON from BloodHound documentation.
    /// </summary>
    /// <remarks>
    /// This example recreates the minimal working example from BloodHound OpenGraph documentation.
    /// https://bloodhound.specterops.io/opengraph/schema#minimal-working-json
    /// </remarks>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create the minimal working example from BloodHound OpenGraph documentation.
            // https://bloodhound.specterops.io/opengraph/schema#minimal-working-json

            // Create an OpenGraph instance
            var graph = new OpenGraph(sourceKind: "Base");

            // Create nodes
            var bobProperties = new Properties
            {
                ["displayname"] = "bob",
                ["property"] = "a",
                ["objectid"] = "123",
                ["name"] = "BOB"
            };

            var bobNode = new Node(
                id: "123",
                kinds: new List<string> { "Person", "Base" },
                properties: bobProperties
            );

            var aliceProperties = new Properties
            {
                ["displayname"] = "alice",
                ["property"] = "b",
                ["objectid"] = "234",
                ["name"] = "ALICE"
            };

            var aliceNode = new Node(
                id: "234",
                kinds: new List<string> { "Person", "Base" },
                properties: aliceProperties
            );

            // Add nodes to graph
            graph.AddNode(bobNode);
            graph.AddNode(aliceNode);

            // Create edge: Bob knows Alice
            var knowsEdge = new Edge(
                startNode: bobNode.Id,      // Bob is the start
                endNode: aliceNode.Id,      // Alice is the end
                kind: "Knows"
            );

            // Add edge to graph
            graph.AddEdge(knowsEdge);

            // Export to file
            await graph.ExportToFileAsync("minimal_working_json.json");

            Console.WriteLine("Graph exported successfully to minimal_working_json.json");
            Console.WriteLine($"Graph contains {graph.GetNodeCount()} nodes and {graph.GetEdgeCount()} edges");
        }
    }
}
