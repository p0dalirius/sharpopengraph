using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SharpOpenGraph
{
    /// <summary>
    /// Represents the graph structure containing nodes and edges.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// Gets or sets the list of nodes in the graph.
        /// </summary>
        [JsonPropertyName("nodes")]
        public List<Node> Nodes { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of edges in the graph.
        /// </summary>
        [JsonPropertyName("edges")]
        public List<Edge> Edges { get; set; } = new();
    }
}
