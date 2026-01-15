using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SharpOpenGraph
{
    /// <summary>
    /// Properties class for storing arbitrary key-value pairs for nodes and edges.
    /// Follows BloodHound OpenGraph schema requirements where properties must be primitive types.
    /// </summary>
    /// <remarks>
    /// Property values must be primitive types: string, int, float, bool, null, or arrays of primitive types.
    /// Arrays must be homogeneous (all items must be of the same type).
    /// </remarks>
    public class Properties : IDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="Properties"/> class.
        /// </summary>
        public Properties()
        {
            _properties = new Dictionary<string, object?>();
        }

        /// <summary>
        /// Gets the number of properties.
        /// </summary>
        public int Count => _properties.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a collection containing the keys of the properties.
        /// </summary>
        public ICollection<string> Keys => _properties.Keys;

        /// <summary>
        /// Gets a collection containing the values of the properties.
        /// </summary>
        public ICollection<object?> Values => _properties.Values;

        /// <summary>
        /// Gets or sets the property value with the specified key.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <returns>The property value.</returns>
        public object? this[string key]
        {
            get => _properties[key];
            set => SetProperty(key, value);
        }

        /// <summary>
        /// Sets a property value. Only primitive types are allowed.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <param name="value">Property value (must be primitive type: string, int, float, bool, null, or array of primitives).</param>
        /// <exception cref="ArgumentException">Thrown when the value is not a valid primitive type.</exception>
        public void SetProperty(string key, object? value)
        {
            if (!IsValidPropertyValue(value))
            {
                throw new ArgumentException(
                    $"Property value must be a primitive type (string, int, float, bool, null, or array of primitives), got {value?.GetType().Name ?? "null"}",
                    nameof(value));
            }

            _properties[key] = value;
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <param name="defaultValue">Default value if key doesn't exist.</param>
        /// <returns>Property value or default value.</returns>
        public object? GetProperty(string key, object? defaultValue = null)
        {
            return _properties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Removes a property.
        /// </summary>
        /// <param name="key">Property name to remove.</param>
        public void RemoveProperty(string key)
        {
            if (_properties.ContainsKey(key))
            {
                _properties.Remove(key);
            }
        }

        /// <summary>
        /// Checks if a property exists.
        /// </summary>
        /// <param name="key">Property name to check.</param>
        /// <returns>True if property exists, false otherwise.</returns>
        public bool HasProperty(string key)
        {
            return _properties.ContainsKey(key);
        }

        /// <summary>
        /// Gets all properties as a dictionary.
        /// </summary>
        /// <returns>Copy of all properties.</returns>
        public Dictionary<string, object?> GetAllProperties()
        {
            return new Dictionary<string, object?>(_properties);
        }

        /// <summary>
        /// Converts properties to dictionary for JSON serialization.
        /// Matches Python's to_dict() method.
        /// </summary>
        /// <returns>Copy of all properties as dictionary.</returns>
        public Dictionary<string, object?> ToDict()
        {
            return new Dictionary<string, object?>(_properties);
        }

        /// <summary>
        /// Clears all properties.
        /// </summary>
        public void Clear()
        {
            _properties.Clear();
        }

        /// <summary>
        /// Validates all properties according to OpenGraph schema rules.
        /// </summary>
        /// <returns>A tuple containing validation result (isValid, listOfErrors).</returns>
        public (bool IsValid, List<string> Errors) Validate()
        {
            var errors = new List<string>();

            foreach (var kvp in _properties)
            {
                if (!IsValidPropertyValue(kvp.Value))
                {
                    errors.Add(
                        $"Property '{kvp.Key}' has invalid value type '{kvp.Value?.GetType().Name ?? "null"}' not in (str, int, float, bool, None, list)");
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates a single property value according to OpenGraph schema rules.
        /// </summary>
        /// <param name="value">The property value to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValidPropertyValue(object? value)
        {
            // Check if value is None (allowed)
            if (value == null)
            {
                return true;
            }

            // Check if value is a primitive type
            if (value is string || value is int || value is long || value is float || value is double || value is bool)
            {
                return true;
            }

            // Check if value is an array
            if (value is IEnumerable enumerable && value is not string)
            {
                var list = enumerable.Cast<object?>().ToList();

                if (list.Count == 0) // Empty array is valid
                {
                    return true;
                }

                // Check if all items are of the same primitive type
                var firstItem = list[0];
                if (firstItem == null)
                {
                    // Array of nulls is valid
                    return list.All(item => item == null);
                }

                var firstType = firstItem.GetType();

                // First item must be a primitive type
                if (firstType != typeof(string) &&
                    firstType != typeof(int) &&
                    firstType != typeof(long) &&
                    firstType != typeof(float) &&
                    firstType != typeof(double) &&
                    firstType != typeof(bool))
                {
                    return false;
                }

                // Check that all items are the same type and not objects/arrays
                // Python logic: if not isinstance(item, first_type) or isinstance(item, (dict, list)): return False
                foreach (var item in list)
                {
                    if (item == null)
                    {
                        continue; // null items are allowed
                    }

                    var itemType = item.GetType();

                    // Check: item is not the same type as first_type OR item is a dict/list
                    if (itemType != firstType || item is IDictionary || (item is IEnumerable && item is not string))
                    {
                        return false;
                    }
                }

                return true;
            }

            // Objects are not allowed
            return false;
        }

        #region IDictionary Implementation

        /// <summary>
        /// Adds a property to the collection.
        /// </summary>
        public void Add(string key, object? value)
        {
            SetProperty(key, value);
        }

        /// <summary>
        /// Adds a key-value pair to the collection.
        /// </summary>
        public void Add(KeyValuePair<string, object?> item)
        {
            SetProperty(item.Key, item.Value);
        }

        /// <summary>
        /// Determines whether the collection contains a property with the specified key.
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _properties.ContainsKey(key);
        }

        /// <summary>
        /// Removes the property with the specified key.
        /// </summary>
        public bool Remove(string key)
        {
            if (_properties.ContainsKey(key))
            {
                _properties.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the first occurrence of a specific key-value pair from the collection.
        /// </summary>
        public bool Remove(KeyValuePair<string, object?> item)
        {
            if (_properties.TryGetValue(item.Key, out var value) && Equals(value, item.Value))
            {
                return _properties.Remove(item.Key);
            }
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        public bool TryGetValue(string key, out object? value)
        {
            return _properties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the properties.
        /// </summary>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the properties.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Copies the properties to an array, starting at a particular array index.
        /// </summary>
        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object?>>)_properties).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determines whether the collection contains a specific key-value pair.
        /// </summary>
        public bool Contains(KeyValuePair<string, object?> item)
        {
            return _properties.TryGetValue(item.Key, out var value) && Equals(value, item.Value);
        }

        #endregion

        /// <summary>
        /// Returns a string representation of the properties.
        /// Matches Python's __repr__ format.
        /// </summary>
        public override string ToString()
        {
            // Python: f"Properties({self._properties})"
            // Format the dictionary contents
            var items = string.Join(", ", _properties.Select(kvp =>
                $"{kvp.Key}={FormatValue(kvp.Value)}"));
            return $"Properties({{{items}}})";
        }

        /// <summary>
        /// Formats a value for string representation.
        /// </summary>
        private static string FormatValue(object? value)
        {
            if (value == null)
                return "None";
            if (value is string str)
                return $"'{str}'";
            if (value is IEnumerable enumerable && value is not string)
            {
                var items = enumerable.Cast<object?>().Select(FormatValue);
                return $"[{string.Join(", ", items)}]";
            }
            return value.ToString() ?? "null";
        }
    }
}
