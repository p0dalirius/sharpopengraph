using System;
using System.Text.Json.Serialization;

namespace SharpOpenGraph
{
    /// <summary>
    /// Represents a reference to a node in the graph, specifying how to match the node.
    /// </summary>
    public class NodeReference
    {
        private string _matchBy = "id";

        /// <summary>
        /// Gets or sets the value used for matching — either an object ID or a name, depending on match_by.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets how to match the node: "id" (by unique object ID) or "name" (by name property).
        /// Defaults to "id".
        /// </summary>
        [JsonPropertyName("match_by")]
        public string MatchBy
        {
            get => _matchBy;
            set
            {
                if (value != "id" && value != "name")
                {
                    throw new ArgumentException("MatchBy must be either 'id' or 'name'", nameof(value));
                }
                _matchBy = value;
            }
        }

        /// <summary>
        /// Gets or sets an optional kind filter; the referenced node must have this kind.
        /// </summary>
        [JsonPropertyName("kind")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Kind { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeReference"/> class.
        /// </summary>
        /// <param name="value">The value used for matching — either an object ID or a name.</param>
        /// <param name="matchBy">How to match the node: "id" or "name". Defaults to "id".</param>
        /// <param name="kind">Optional kind filter; the referenced node must have this kind.</param>
        /// <exception cref="ArgumentException">Thrown when value is null or empty, or matchBy is not "id" or "name".</exception>
        public NodeReference(string value, string matchBy = "id", string? kind = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(value));
            }

            Value = value;
            MatchBy = matchBy;
            Kind = kind;
        }

        /// <summary>
        /// Returns a string representation of the node reference.
        /// </summary>
        public override string ToString()
        {
            var kindStr = Kind != null ? $", kind='{Kind}'" : "";
            return $"NodeReference(value='{Value}', match_by='{MatchBy}'{kindStr})";
        }
    }
}
