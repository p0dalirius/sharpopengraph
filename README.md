# sharpopengraph: A C# library to create BloodHound OpenGraphs 

<p align="center">
  A C# library to create BloodHound OpenGraphs easily
  <br>
  <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/p0dalirius/sharpopengraph">
  <a href="https://twitter.com/intent/follow?screen_name=podalirius_" title="Follow"><img src="https://img.shields.io/twitter/follow/podalirius_?label=Podalirius&style=social"></a>
  <a href="https://www.youtube.com/c/Podalirius_?sub_confirmation=1" title="Subscribe"><img alt="YouTube Channel Subscribers" src="https://img.shields.io/youtube/channel/subscribers/UCF_x5O7CSfr82AfNVTKOv_A?style=social"></a>
  <br>
  <img height=21px src="https://img.shields.io/badge/Get bloodhound:-191646"> <a href="https://specterops.io/bloodhound-enterprise/" title="Get BloodHound Enterprise"><img alt="Get BloodHound Enterprise" height=21px src="https://mintlify.s3.us-west-1.amazonaws.com/specterops/assets/enterprise-edition-pill-tag.svg"></a>
  <a href="https://specterops.io/bloodhound-community-edition/" title="Get BloodHound Community"><img alt="Get BloodHound Community" height=21px src="https://mintlify.s3.us-west-1.amazonaws.com/specterops/assets/community-edition-pill-tag.svg"></a>
  <br>
  <br>
  This library also exists in: <a href="https://github.com/p0dalirius/sharpopengraph">C#</a> | <a href="https://github.com/TheManticoreProject/gopengraph">Go</a> | <a href="https://github.com/p0dalirius/bhopengraph">Python</a>
</p>

## Features

This module provides C# classes for creating and managing graph structures that are compatible with BloodHound OpenGraph. The classes follow the [BloodHound OpenGraph schema](https://bloodhound.specterops.io/opengraph/schema) and [best practices](https://bloodhound.specterops.io/opengraph/best-practices).

If you don't know about BloodHound OpenGraph yet, a great introduction can be found here: [https://bloodhound.specterops.io/opengraph/best-practices](https://bloodhound.specterops.io/opengraph/best-practices)

The complete documentation of this library can be found here: https://sharpopengraph.readthedocs.io/en/latest/ 

## Examples

Here is an example of a C# program using the [sharpopengraph](https://github.com/p0dalirius/sharpopengraph) C# library to model the [Minimal Working JSON](https://bloodhound.specterops.io/opengraph/schema#minimal-working-json) from the OpenGraph Schema documentation:

```C#
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
```

This gives us the following [Minimal Working JSON](https://bloodhound.specterops.io/opengraph/schema#minimal-working-json) as per the documentation:

```json
{
  "graph": {
    "nodes": [
      {
        "id": "123",
        "kinds": [
          "Person",
          "Base"
        ],
        "properties": {
          "displayname": "bob",
          "property": "a",
          "objectid": "123",
          "name": "BOB"
        }
      },
      {
        "id": "234",
        "kinds": [
          "Person",
          "Base"
        ],
        "properties": {
          "displayname": "alice",
          "property": "b",
          "objectid": "234",
          "name": "ALICE"
        }
      }
    ],
    "edges": [
      {
        "kind": "Knows",
        "start": {
          "value": "123",
          "match_by": "id"
        },
        "end": {
          "value": "234",
          "match_by": "id"
        }
      }
    ]
  },
  "metadata": {
    "source_kind": "Base"
  }
}
```

## Contributing

Pull requests are welcome. Feel free to open an issue if you want to add other features.

## References

- [BloodHound OpenGraph Best Practices](https://bloodhound.specterops.io/opengraph/best-practices)
- [BloodHound OpenGraph Schema](https://bloodhound.specterops.io/opengraph/schema)
- [BloodHound OpenGraph API](https://bloodhound.specterops.io/opengraph/api)
- [BloodHound OpenGraph Custom Icons](https://bloodhound.specterops.io/opengraph/custom-icons)
