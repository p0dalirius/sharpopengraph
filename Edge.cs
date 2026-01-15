using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpOpenGraph
{
    /// <summary>
    /// Custom JSON converter for Edge Properties that only serializes non-empty properties.
    /// </summary>
    internal class EdgePropertiesConverter : JsonConverter<Properties?>
    {
        public override Properties? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var properties = new Properties();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string key = reader.GetString() ?? "";
                        reader.Read();
                        object? value = JsonSerializer.Deserialize<object>(ref reader, options);
                        properties.SetProperty(key, value);
                    }
                }
                return properties;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Properties? value, JsonSerializerOptions options)
        {
            // Only write if properties exist and are not empty (matching Python behavior)
            // Return null for empty properties so JsonIgnore can skip them
            if (value != null && value.Count > 0)
            {
                // Write the properties as a JSON object
                writer.WriteStartObject();
                foreach (var kvp in value)
                {
                    writer.WritePropertyName(kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
            }
            else
            {
                // Write null so JsonIgnore can skip it
                writer.WriteNullValue();
            }
        }
    }
    /// <summary>
    /// Edge class representing a directed edge in the OpenGraph.
    /// Follows BloodHound OpenGraph schema requirements with start/end nodes, kind, and properties.
    /// All edges are directed and one-way as per BloodHound requirements.
    /// </summary>
    /// <remarks>
    /// Sources:
    /// - https://bloodhound.specterops.io/opengraph/schema#edges
    /// - https://bloodhound.specterops.io/opengraph/schema#minimal-working-json
    /// </remarks>
    public class Edge
    {
        private Properties? _properties;

        /// <summary>
        /// Gets or sets the type/class of the edge relationship.
        /// </summary>
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the reference to the start (source) node.
        /// </summary>
        [JsonPropertyName("start")]
        public NodeReference Start { get; set; }

        /// <summary>
        /// Gets or sets the reference to the end (destination) node.
        /// </summary>
        [JsonPropertyName("end")]
        public NodeReference End { get; set; }

        /// <summary>
        /// Gets or sets the edge properties.
        /// Only serialized if not null and not empty (matching Python behavior).
        /// </summary>
        [JsonPropertyName("properties")]
        [JsonConverter(typeof(EdgePropertiesConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Properties? Properties
        {
            get => _properties;
            set => _properties = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge"/> class.
        /// </summary>
        /// <param name="startNode">ID or name of the source node.</param>
        /// <param name="endNode">ID or name of the destination node.</param>
        /// <param name="kind">Type/class of the edge relationship.</param>
        /// <param name="properties">Edge properties. If null, an empty Properties instance will be created.</param>
        /// <param name="startMatchBy">How to match the start node: "id" or "name". Defaults to "id".</param>
        /// <param name="endMatchBy">How to match the end node: "id" or "name". Defaults to "id".</param>
        /// <param name="startKind">Optional kind filter for the start node.</param>
        /// <param name="endKind">Optional kind filter for the end node.</param>
        /// <exception cref="ArgumentException">Thrown when any required parameter is null or empty.</exception>
        public Edge(
            string startNode,
            string endNode,
            string kind,
            Properties? properties = null,
            string startMatchBy = "id",
            string endMatchBy = "id",
            string? startKind = null,
            string? endKind = null)
        {
            if (string.IsNullOrWhiteSpace(startNode))
            {
                throw new ArgumentException("Start node cannot be empty", nameof(startNode));
            }
            if (string.IsNullOrWhiteSpace(endNode))
            {
                throw new ArgumentException("End node cannot be empty", nameof(endNode));
            }
            if (string.IsNullOrWhiteSpace(kind))
            {
                throw new ArgumentException("Edge kind cannot be empty", nameof(kind));
            }

            Kind = kind;
            Start = new NodeReference(startNode, startMatchBy, startKind);
            End = new NodeReference(endNode, endMatchBy, endKind);
            _properties = properties ?? new Properties();
        }

        /// <summary>
        /// Sets a property on the edge.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <param name="value">Property value.</param>
        public void SetProperty(string key, object? value)
        {
            _properties ??= new Properties();
            _properties.SetProperty(key, value);
        }

        /// <summary>
        /// Gets a property from the edge.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <param name="defaultValue">Default value if property doesn't exist.</param>
        /// <returns>Property value or default value.</returns>
        public object? GetProperty(string key, object? defaultValue = null)
        {
            return _properties?.GetProperty(key, defaultValue);
        }

        /// <summary>
        /// Removes a property from the edge.
        /// </summary>
        /// <param name="key">Property name to remove.</param>
        public void RemoveProperty(string key)
        {
            _properties?.RemoveProperty(key);
        }

        /// <summary>
        /// Gets the start node value (ID or name).
        /// </summary>
        /// <returns>Start node value.</returns>
        public string GetStartNode()
        {
            return Start.Value;
        }

        /// <summary>
        /// Gets the end node value (ID or name).
        /// </summary>
        /// <returns>End node value.</returns>
        public string GetEndNode()
        {
            return End.Value;
        }

        /// <summary>
        /// Gets the edge kind/type.
        /// </summary>
        /// <returns>Edge kind.</returns>
        public string GetKind()
        {
            return Kind;
        }

        /// <summary>
        /// Gets a unique ID for the edge.
        /// </summary>
        /// <returns>Unique ID for the edge.</returns>
        public string GetUniqueId()
        {
            return $"[{Start.MatchBy}:{Start.Value}]-({Kind})->[{End.MatchBy}:{End.Value}]";
        }

        /// <summary>
        /// Validates the edge against the BloodHound OpenGraph schema.
        /// </summary>
        /// <returns>A tuple containing validation result (isValid, listOfErrors).</returns>
        public (bool IsValid, List<string> Errors) Validate()
        {
            var errors = new List<string>();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(Kind))
            {
                errors.Add("Edge kind cannot be empty");
            }

            if (Start == null)
            {
                errors.Add("Start node reference cannot be null");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Start.Value))
                {
                    errors.Add("Start node value cannot be empty");
                }
                if (Start.MatchBy != "id" && Start.MatchBy != "name")
                {
                    errors.Add("Start match_by must be either 'id' or 'name'");
                }
            }

            if (End == null)
            {
                errors.Add("End node reference cannot be null");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(End.Value))
                {
                    errors.Add("End node value cannot be empty");
                }
                if (End.MatchBy != "id" && End.MatchBy != "name")
                {
                    errors.Add("End match_by must be either 'id' or 'name'");
                }
            }

            // Validate properties if they exist
            if (_properties != null)
            {
                var (isPropsValid, propErrors) = _properties.Validate();
                if (!isPropsValid)
                {
                    errors.AddRange(propErrors);
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current edge.
        /// Two edges are considered equal if they have the same start, end, and kind.
        /// </summary>
        /// <param name="obj">The object to compare with the current edge.</param>
        /// <returns>True if the specified object is equal to the current edge; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is Edge other)
            {
                return Start.Value == other.Start.Value
                    && End.Value == other.End.Value
                    && Kind == other.Kind;
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this edge based on start, end, and kind.
        /// </summary>
        /// <returns>A hash code for the current edge.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Start.Value, End.Value, Kind);
        }

        /// <summary>
        /// Returns a string representation of the edge.
        /// </summary>
        /// <returns>A string that represents the current edge.</returns>
        public override string ToString()
        {
            return $"Edge(start='{Start.Value}', end='{End.Value}', kind='{Kind}', properties={_properties})";
        }
    }
}
