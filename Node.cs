using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SharpOpenGraph
{
    /// <summary>
    /// Node class representing a node in the OpenGraph.
    /// Follows BloodHound OpenGraph schema requirements with unique IDs, kinds, and properties.
    /// </summary>
    /// <remarks>
    /// Sources:
    /// - https://bloodhound.specterops.io/opengraph/schema#nodes
    /// - https://bloodhound.specterops.io/opengraph/schema#minimal-working-json
    /// </remarks>
    public class Node
    {
        private List<string> _kinds;
        private Properties _properties;

        /// <summary>
        /// Gets or sets the universally unique identifier for the node.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the list of node types/classes.
        /// The first element is treated as the node's primary kind and is used to determine which icon to display.
        /// Must contain at least 1 and at most 3 kinds.
        /// </summary>
        [JsonPropertyName("kinds")]
        public List<string> Kinds
        {
            get => _kinds;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Kinds cannot be null");
                }
                if (value.Count < 1)
                {
                    throw new ArgumentException("Node must have at least one kind", nameof(value));
                }
                if (value.Count > 3)
                {
                    throw new ArgumentException("Node can have at most 3 kinds", nameof(value));
                }
                _kinds = value;
            }
        }

        /// <summary>
        /// Gets or sets the node properties.
        /// Always has a value (never null), matching Python behavior.
        /// </summary>
        [JsonPropertyName("properties")]
        public Properties Properties
        {
            get => _properties;
            set => _properties = value ?? new Properties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="id">Universally unique identifier for the node.</param>
        /// <param name="kinds">List of node types/classes. Must contain at least one kind.</param>
        /// <param name="properties">Node properties. If null, an empty Properties instance will be created.</param>
        /// <exception cref="ArgumentException">Thrown when id is null or empty, or when kinds is empty or contains more than 3 items.</exception>
        public Node(string id, List<string>? kinds = null, Properties? properties = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Node ID cannot be empty", nameof(id));
            }

            Id = id;
            _kinds = kinds ?? new List<string>();

            if (_kinds.Count < 1)
            {
                throw new ArgumentException("Node must have at least one kind", nameof(kinds));
            }
            if (_kinds.Count > 3)
            {
                throw new ArgumentException("Node can have at most 3 kinds", nameof(kinds));
            }

            _properties = properties ?? new Properties();
        }

        /// <summary>
        /// Adds a kind/type to the node.
        /// </summary>
        /// <param name="kind">Kind/type to add.</param>
        /// <exception cref="ArgumentException">Thrown when the node already has 3 kinds or the kind is null/empty.</exception>
        public void AddKind(string kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
            {
                throw new ArgumentException("Kind cannot be null or empty", nameof(kind));
            }

            if (_kinds.Count >= 3)
            {
                throw new ArgumentException("Node can only have a maximum of 3 kinds");
            }

            if (!_kinds.Contains(kind))
            {
                _kinds.Add(kind);
            }
        }

        /// <summary>
        /// Removes a kind/type from the node.
        /// </summary>
        /// <param name="kind">Kind/type to remove.</param>
        public void RemoveKind(string kind)
        {
            if (_kinds.Contains(kind))
            {
                _kinds.Remove(kind);
            }
        }

        /// <summary>
        /// Checks if the node has a specific kind/type.
        /// </summary>
        /// <param name="kind">Kind/type to check.</param>
        /// <returns>True if the node has the kind, false otherwise.</returns>
        public bool HasKind(string kind)
        {
            return _kinds.Contains(kind);
        }

        /// <summary>
        /// Sets a property on the node.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <param name="value">Property value.</param>
        public void SetProperty(string key, object? value)
        {
            _properties ??= new Properties();
            _properties.SetProperty(key, value);
        }

        /// <summary>
        /// Gets a property from the node.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <param name="defaultValue">Default value if property doesn't exist.</param>
        /// <returns>Property value or default value.</returns>
        public object? GetProperty(string key, object? defaultValue = null)
        {
            return _properties?.GetProperty(key, defaultValue);
        }

        /// <summary>
        /// Removes a property from the node.
        /// </summary>
        /// <param name="key">Property name to remove.</param>
        public void RemoveProperty(string key)
        {
            _properties?.RemoveProperty(key);
        }

        /// <summary>
        /// Validates the node against the BloodHound OpenGraph schema.
        /// </summary>
        /// <returns>A tuple containing validation result (isValid, listOfErrors).</returns>
        public (bool IsValid, List<string> Errors) Validate()
        {
            var errors = new List<string>();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(Id))
            {
                errors.Add("Node ID cannot be empty");
            }

            // Validate kinds
            if (_kinds == null)
            {
                errors.Add("Kinds cannot be null");
            }
            else if (_kinds.Count < 1)
            {
                errors.Add("Node must have at least one kind");
            }
            else if (_kinds.Count > 3)
            {
                errors.Add("Node can have at most 3 kinds");
            }
            else
            {
                for (int i = 0; i < _kinds.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(_kinds[i]))
                    {
                        errors.Add($"Kind at index {i} cannot be null or empty");
                    }
                }
            }

            // Validate properties (always exists, matching Python behavior)
            var (isPropsValid, propErrors) = _properties.Validate();
            if (!isPropsValid)
            {
                errors.AddRange(propErrors);
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Converts the node to a dictionary representation for JSON serialization.
        /// Matches Python's to_dict() method.
        /// </summary>
        /// <returns>Dictionary representation of the node.</returns>
        public Dictionary<string, object?> ToDict()
        {
            var result = new Dictionary<string, object?>
            {
                ["id"] = Id,
                ["kinds"] = new List<string>(_kinds),
                ["properties"] = _properties.ToDict()
            };
            return result;
        }

        /// <summary>
        /// Creates a Node from a dictionary representation.
        /// Matches Python's from_dict() classmethod.
        /// </summary>
        /// <param name="data">Dictionary containing node data with keys: "id", "kinds", "properties".</param>
        /// <returns>A new Node instance, or null if the data is invalid.</returns>
        public static Node? FromDict(Dictionary<string, object?> data)
        {
            try
            {
                if (!data.TryGetValue("id", out var idObj) || idObj is not string id || string.IsNullOrWhiteSpace(id))
                {
                    return null;
                }

                var kinds = new List<string>();
                if (data.TryGetValue("kinds", out var kindsObj) && kindsObj is IEnumerable<object?> kindsList)
                {
                    foreach (var k in kindsList)
                    {
                        if (k is string kindStr)
                        {
                            kinds.Add(kindStr);
                        }
                    }
                }

                Properties? properties = null;
                if (data.TryGetValue("properties", out var propsObj) && propsObj is IDictionary<string, object?> propsDict)
                {
                    properties = new Properties();
                    foreach (var kvp in propsDict)
                    {
                        properties.SetProperty(kvp.Key, kvp.Value);
                    }
                }

                return new Node(id, kinds, properties);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current node.
        /// Two nodes are considered equal if they have the same ID.
        /// </summary>
        /// <param name="obj">The object to compare with the current node.</param>
        /// <returns>True if the specified object is equal to the current node; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is Node other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this node based on its ID.
        /// </summary>
        /// <returns>A hash code for the current node.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the node.
        /// </summary>
        /// <returns>A string that represents the current node.</returns>
        public override string ToString()
        {
            return $"Node(id='{Id}', kinds=[{string.Join(", ", _kinds)}], properties={_properties})";
        }
    }
}
