using System.Text.Json.Serialization;

namespace SharpOpenGraph
{
    /// <summary>
    /// Represents metadata about the graph, including the source kind.
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Gets or sets the source kind for all nodes in the graph.
        /// </summary>
        [JsonPropertyName("source_kind")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SourceKind { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class.
        /// </summary>
        /// <param name="sourceKind">The source kind for all nodes in the graph.</param>
        public Metadata(string? sourceKind = null)
        {
            SourceKind = sourceKind;
        }
    }
}
