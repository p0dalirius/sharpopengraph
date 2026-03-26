using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharpOpenGraph
{
    /// <summary>
    /// OpenGraph class for managing a graph structure compatible with BloodHound OpenGraph.
    /// Follows BloodHound OpenGraph schema requirements and best practices.
    /// </summary>
    /// <remarks>
    /// Sources:
    /// - https://bloodhound.specterops.io/opengraph/schema#opengraph
    /// - https://bloodhound.specterops.io/opengraph/schema#minimal-working-json
    /// - https://bloodhound.specterops.io/opengraph/best-practices
    /// </remarks>
    public class OpenGraph
    {
        private readonly Dictionary<string, Node> _nodes;
        private readonly Dictionary<string, Edge> _edges;

        /// <summary>
        /// Gets the source kind for all nodes in the graph.
        /// </summary>
        [JsonPropertyName("source_kind")]
        [JsonIgnore]
        public string? SourceKind { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenGraph"/> class.
        /// </summary>
        /// <param name="sourceKind">Optional source kind for all nodes in the graph.</param>
        public OpenGraph(string? sourceKind = null)
        {
            _nodes = new Dictionary<string, Node>();
            _edges = new Dictionary<string, Edge>();
            SourceKind = sourceKind;
        }

        #region Edge Methods

        /// <summary>
        /// Generates a unique key for an edge based on start node, end node, and kind.
        /// </summary>
        /// <param name="edge">Edge to generate key for.</param>
        /// <returns>Unique key for the edge.</returns>
        private static string GetEdgeKey(Edge edge)
        {
            return $"{edge.Start.Value}|{edge.End.Value}|{edge.Kind}";
        }

        /// <summary>
        /// Adds an edge to the graph if it doesn't already exist and if the start and end nodes exist.
        /// </summary>
        /// <param name="edge">Edge to add.</param>
        /// <returns>True if edge was added, False if start or end node doesn't exist or edge already exists.</returns>
        public bool AddEdge(Edge edge)
        {
            if (edge == null)
            {
                return false;
            }

            // Python: edge.start_node not in self.nodes or edge.end_node not in self.nodes
            // In Python, start_node and end_node are always ID strings
            if (!_nodes.ContainsKey(edge.Start.Value))
            {
                return false;
            }
            if (!_nodes.ContainsKey(edge.End.Value))
            {
                return false;
            }

            string edgeKey = GetEdgeKey(edge);
            if (_edges.ContainsKey(edgeKey))
            {
                return false;
            }

            _edges[edgeKey] = edge;
            return true;
        }


        /// <summary>
        /// Adds a list of edges to the graph.
        /// </summary>
        /// <param name="edges">List of edges to add.</param>
        /// <returns>True if all edges were added successfully, False if any failed.</returns>
        public bool AddEdges(IEnumerable<Edge> edges)
        {
            if (edges == null)
            {
                return false;
            }

            bool success = true;
            foreach (var edge in edges)
            {
                if (!AddEdge(edge))
                {
                    success = false;
                }
            }
            return success;
        }

        /// <summary>
        /// Adds an edge to the graph without validation. If an edge with the same key already exists, it will be overwritten.
        /// </summary>
        /// <param name="edge">Edge to add.</param>
        /// <returns>True if edge was added, False if edge is invalid.</returns>
        public bool AddEdgeWithoutValidation(Edge edge)
        {
            if (edge == null)
            {
                return false;
            }

            string edgeKey = GetEdgeKey(edge);
            _edges[edgeKey] = edge;
            return true;
        }

        /// <summary>
        /// Adds a list of edges to the graph without validation.
        /// </summary>
        /// <param name="edges">List of edges to add.</param>
        /// <returns>True if edges were added successfully.</returns>
        public bool AddEdgesWithoutValidation(IEnumerable<Edge> edges)
        {
            if (edges == null)
            {
                return false;
            }

            foreach (var edge in edges)
            {
                AddEdgeWithoutValidation(edge);
            }
            return true;
        }

        /// <summary>
        /// Gets all edges of a specific kind.
        /// </summary>
        /// <param name="kind">Kind/type to filter by.</param>
        /// <returns>List of edges with the specified kind.</returns>
        public List<Edge> GetEdgesByKind(string kind)
        {
            return _edges.Values.Where(e => e.Kind == kind).ToList();
        }

        /// <summary>
        /// Gets all edges starting from a specific node.
        /// </summary>
        /// <param name="nodeId">ID of the source node.</param>
        /// <returns>List of edges starting from the specified node.</returns>
        public List<Edge> GetEdgesFromNode(string nodeId)
        {
            // Python: edge.start_node == node_id
            // In Python, start_node is always an ID string, so we check Start.Value
            return _edges.Values.Where(e => e.Start.Value == nodeId).ToList();
        }

        /// <summary>
        /// Gets all edges ending at a specific node.
        /// </summary>
        /// <param name="nodeId">ID of the destination node.</param>
        /// <returns>List of edges ending at the specified node.</returns>
        public List<Edge> GetEdgesToNode(string nodeId)
        {
            // Python: edge.end_node == node_id
            // In Python, end_node is always an ID string, so we check End.Value
            return _edges.Values.Where(e => e.End.Value == nodeId).ToList();
        }

        /// <summary>
        /// Gets all edges that have no start or end node.
        /// These are edges that are not connected to any other nodes in the graph.
        /// </summary>
        /// <returns>List of edges with no start or end node.</returns>
        public List<Edge> GetIsolatedEdges()
        {
            // Python: edge.start_node not in self.nodes or edge.end_node not in self.nodes
            // In Python, start_node and end_node are ID strings
            return _edges.Values.Where(edge =>
                !_nodes.ContainsKey(edge.Start.Value) || !_nodes.ContainsKey(edge.End.Value)).ToList();
        }

        /// <summary>
        /// Gets the total number of isolated edges in the graph.
        /// </summary>
        /// <returns>Number of isolated edges.</returns>
        public int GetIsolatedEdgesCount()
        {
            return GetIsolatedEdges().Count;
        }

        /// <summary>
        /// Gets the total number of edges in the graph.
        /// </summary>
        /// <returns>Number of edges.</returns>
        public int GetEdgeCount()
        {
            return _edges.Count;
        }

        #endregion

        #region Node Methods

        /// <summary>
        /// Adds a node to the graph.
        /// </summary>
        /// <param name="node">Node to add.</param>
        /// <returns>True if node was added, False if node with same ID already exists.</returns>
        public bool AddNode(Node node)
        {
            if (node == null)
            {
                return false;
            }

            if (_nodes.ContainsKey(node.Id))
            {
                return false;
            }

            // Add source_kind to node kinds if specified
            // Python: if self.source_kind and self.source_kind not in node.kinds: node.add_kind(self.source_kind)
            // Python's add_kind raises ValueError if node has 3 kinds, but we just continue
            if (!string.IsNullOrWhiteSpace(SourceKind) && !node.HasKind(SourceKind))
            {
                try
                {
                    node.AddKind(SourceKind);
                }
                catch (ArgumentException)
                {
                    // Node already has 3 kinds, skip adding source_kind (matches Python behavior)
                }
            }

            _nodes[node.Id] = node;
            return true;
        }

        /// <summary>
        /// Adds a list of nodes to the graph.
        /// </summary>
        /// <param name="nodes">List of nodes to add.</param>
        /// <returns>True if all nodes were added successfully.</returns>
        public bool AddNodes(IEnumerable<Node> nodes)
        {
            if (nodes == null)
            {
                return false;
            }

            foreach (var node in nodes)
            {
                AddNode(node);
            }
            return true;
        }

        /// <summary>
        /// Adds a node to the graph without validation.
        /// </summary>
        /// <param name="node">Node to add.</param>
        /// <returns>True if node was added, False if node is invalid.</returns>
        public bool AddNodeWithoutValidation(Node node)
        {
            if (node == null)
            {
                return false;
            }

            _nodes[node.Id] = node;
            return true;
        }

        /// <summary>
        /// Adds a list of nodes to the graph without validation.
        /// </summary>
        /// <param name="nodes">List of nodes to add.</param>
        /// <returns>True if nodes were added successfully.</returns>
        public bool AddNodesWithoutValidation(IEnumerable<Node> nodes)
        {
            if (nodes == null)
            {
                return false;
            }

            foreach (var node in nodes)
            {
                AddNodeWithoutValidation(node);
            }
            return true;
        }

        /// <summary>
        /// Gets a node by ID.
        /// </summary>
        /// <param name="id">ID of the node to retrieve.</param>
        /// <returns>The node if found, null otherwise.</returns>
        public Node? GetNodeById(string id)
        {
            return _nodes.TryGetValue(id, out var node) ? node : null;
        }

        /// <summary>
        /// Gets all nodes of a specific kind.
        /// </summary>
        /// <param name="kind">Kind/type to filter by.</param>
        /// <returns>List of nodes with the specified kind.</returns>
        public List<Node> GetNodesByKind(string kind)
        {
            return _nodes.Values.Where(n => n.HasKind(kind)).ToList();
        }

        /// <summary>
        /// Gets the total number of nodes in the graph.
        /// </summary>
        /// <returns>Number of nodes.</returns>
        public int GetNodeCount()
        {
            return _nodes.Count;
        }

        /// <summary>
        /// Gets all nodes that have no edges.
        /// These are nodes that are not connected to any other nodes in the graph.
        /// </summary>
        /// <returns>List of nodes with no edges.</returns>
        public List<Node> GetIsolatedNodes()
        {
            return _nodes.Values.Where(node =>
                GetEdgesFromNode(node.Id).Count == 0 &&
                GetEdgesToNode(node.Id).Count == 0).ToList();
        }

        /// <summary>
        /// Gets the total number of isolated nodes in the graph.
        /// </summary>
        /// <returns>Number of isolated nodes.</returns>
        public int GetIsolatedNodesCount()
        {
            return GetIsolatedNodes().Count;
        }

        /// <summary>
        /// Removes a node and all its associated edges from the graph.
        /// </summary>
        /// <param name="id">ID of the node to remove.</param>
        /// <returns>True if node was removed, False if node doesn't exist.</returns>
        public bool RemoveNodeById(string id)
        {
            if (!_nodes.ContainsKey(id))
            {
                return false;
            }

            // Remove the node
            _nodes.Remove(id);

            // Remove all edges that reference this node
            var edgesToRemove = _edges
                .Where(kvp => kvp.Value.Start.Value == id || kvp.Value.End.Value == id)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in edgesToRemove)
            {
                _edges.Remove(key);
            }

            return true;
        }

        #endregion

        #region Path Methods

        /// <summary>
        /// Finds all paths between two nodes using BFS.
        /// </summary>
        /// <param name="startId">Starting node ID.</param>
        /// <param name="endId">Target node ID.</param>
        /// <param name="maxDepth">Maximum path length to search. Default is 10.</param>
        /// <returns>List of paths, where each path is a list of node IDs.</returns>
        public List<List<string>> FindPaths(string startId, string endId, int maxDepth = 10)
        {
            if (!_nodes.ContainsKey(startId) || !_nodes.ContainsKey(endId))
            {
                return new List<List<string>>();
            }

            if (startId == endId)
            {
                return new List<List<string>> { new List<string> { startId } };
            }

            var paths = new List<List<string>>();
            var queue = new Queue<(string currentId, List<string> path)>();
            queue.Enqueue((startId, new List<string> { startId }));

            // Python: while queue and len(queue[0][1]) <= max_depth:
            // We check depth after dequeueing, which is equivalent
            while (queue.Count > 0)
            {
                var (currentId, path) = queue.Dequeue();
                int currentDepth = path.Count;

                // Only explore if we haven't reached max depth
                // Python: if current_depth >= max_depth: continue
                if (currentDepth >= maxDepth)
                {
                    continue;
                }

                foreach (var edge in GetEdgesFromNode(currentId))
                {
                    // Python: next_id = edge.end_node
                    string nextId = edge.End.Value;
                    // Check if next_id is not already in the current path (prevents cycles)
                    if (!path.Contains(nextId))
                    {
                        var newPath = new List<string>(path) { nextId };
                        if (nextId == endId)
                        {
                            paths.Add(newPath);
                        }
                        else
                        {
                            queue.Enqueue((nextId, newPath));
                        }
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Finds all connected components in the graph.
        /// </summary>
        /// <returns>List of connected component sets (as lists of node IDs).</returns>
        public List<HashSet<string>> GetConnectedComponents()
        {
            var visited = new HashSet<string>();
            var components = new List<HashSet<string>>();

            foreach (var nodeId in _nodes.Keys)
            {
                if (!visited.Contains(nodeId))
                {
                    var component = new HashSet<string>();
                    var stack = new Stack<string>();
                    stack.Push(nodeId);

                    while (stack.Count > 0)
                    {
                        string current = stack.Pop();
                        if (!visited.Contains(current))
                        {
                            visited.Add(current);
                            component.Add(current);

                            // Add all adjacent nodes
                            // Python: edge.end_node and edge.start_node
                            foreach (var edge in GetEdgesFromNode(current))
                            {
                                string endNode = edge.End.Value;
                                if (!visited.Contains(endNode))
                                {
                                    stack.Push(endNode);
                                }
                            }
                            foreach (var edge in GetEdgesToNode(current))
                            {
                                string startNode = edge.Start.Value;
                                if (!visited.Contains(startNode))
                                {
                                    stack.Push(startNode);
                                }
                            }
                        }
                    }

                    components.Add(component);
                }
            }

            return components;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates the graph for common issues including node and edge validation.
        /// </summary>
        /// <returns>A tuple containing validation result (isValid, listOfErrors).</returns>
        public (bool IsValid, List<string> Errors) ValidateGraph()
        {
            var errors = new List<string>();

            // Validate all nodes
            foreach (var (nodeId, node) in _nodes)
            {
                var (isNodeValid, nodeErrors) = node.Validate();
                if (!isNodeValid)
                {
                    foreach (var error in nodeErrors)
                    {
                        errors.Add($"Node '{nodeId}': {error}");
                    }
                }
            }

            // Validate all edges
            foreach (var (edgeKey, edge) in _edges)
            {
                var (isEdgeValid, edgeErrors) = edge.Validate();
                if (!isEdgeValid)
                {
                    foreach (var error in edgeErrors)
                    {
                        errors.Add($"Edge {edgeKey} ({edge.Start.Value}->{edge.End.Value}): {error}");
                    }
                }
            }

            // Check for graph structure issues
            var startNodeEdges = new Dictionary<string, List<Edge>>();
            var endNodeEdges = new Dictionary<string, List<Edge>>();

            // Build edge mappings and check for isolated edges
            // Python: edge.start_node not in self.nodes or edge.end_node not in self.nodes
            foreach (var (edgeKey, edge) in _edges)
            {
                // Check for isolated edges (edges referencing non-existent nodes)
                string startId = edge.Start.Value;
                string endId = edge.End.Value;

                if (!_nodes.ContainsKey(startId))
                {
                    errors.Add(
                        $"Edge {edgeKey} ({startId}->{endId}): Start node '{startId}' does not exist");
                }
                else
                {
                    // Build start node mapping
                    if (!startNodeEdges.ContainsKey(startId))
                    {
                        startNodeEdges[startId] = new List<Edge>();
                    }
                    startNodeEdges[startId].Add(edge);
                }

                if (!_nodes.ContainsKey(endId))
                {
                    errors.Add(
                        $"Edge {edgeKey} ({startId}->{endId}): End node '{endId}' does not exist");
                }
                else
                {
                    // Build end node mapping
                    if (!endNodeEdges.ContainsKey(endId))
                    {
                        endNodeEdges[endId] = new List<Edge>();
                    }
                    endNodeEdges[endId].Add(edge);
                }
            }

            // Check for isolated nodes using pre-computed mappings
            foreach (var nodeId in _nodes.Keys)
            {
                bool hasOutgoing = startNodeEdges.ContainsKey(nodeId);
                bool hasIncoming = endNodeEdges.ContainsKey(nodeId);

                if (!hasOutgoing && !hasIncoming)
                {
                    errors.Add($"Node '{nodeId}' is isolated (no incoming or outgoing edges)");
                }
            }

            return (errors.Count == 0, errors);
        }

        #endregion

        #region Export Methods

        /// <summary>
        /// Exports the graph to JSON format compatible with BloodHound OpenGraph.
        /// </summary>
        /// <param name="includeMetadata">Whether to include metadata in the export.</param>
        /// <param name="indent">Indentation level (None/null for no indentation, or number of spaces). Default is null.</param>
        /// <returns>JSON string representation of the graph.</returns>
        public string ExportJson(bool includeMetadata = true, int? indent = null)
        {
            var graphData = new OpenGraphExport
            {
                Graph = new GraphExport
                {
                    Nodes = _nodes.Values.ToList(),
                    Edges = _edges.Values.ToList()
                },
                Metadata = includeMetadata && !string.IsNullOrWhiteSpace(SourceKind)
                    ? new MetadataExport { SourceKind = SourceKind }
                    : null
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = indent.HasValue,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = JsonSerializer.Serialize(graphData, options);

            // Python's json.dumps with indent uses the specified number of spaces
            // System.Text.Json uses 2 spaces by default when WriteIndented=true
            // For now, we'll use the default 2-space indentation when indent is specified
            // (Full control would require custom formatting, which is more complex)
            return json;
        }

        // Helper classes for JSON export
        private class OpenGraphExport
        {
            [JsonPropertyName("graph")]
            public GraphExport Graph { get; set; } = null!;

            [JsonPropertyName("metadata")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public MetadataExport? Metadata { get; set; }
        }

        private class GraphExport
        {
            [JsonPropertyName("nodes")]
            public List<Node> Nodes { get; set; } = new();

            [JsonPropertyName("edges")]
            public List<Edge> Edges { get; set; } = new();
        }

        private class MetadataExport
        {
            [JsonPropertyName("source_kind")]
            public string? SourceKind { get; set; }
        }

        /// <summary>
        /// Exports the graph to a JSON file.
        /// </summary>
        /// <param name="filename">Name of the file to write.</param>
        /// <param name="includeMetadata">Whether to include metadata in the export.</param>
        /// <param name="indent">Indentation level (None/null for no indentation, or number of spaces). Default is null.</param>
        /// <returns>True if export was successful, False otherwise.</returns>
        public bool ExportToFile(string filename, bool includeMetadata = true, int? indent = null)
        {
            try
            {
                string jsonData = ExportJson(includeMetadata, indent);
                File.WriteAllText(filename, jsonData);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Exports the graph to a JSON file asynchronously.
        /// </summary>
        /// <param name="filename">Name of the file to write.</param>
        /// <param name="includeMetadata">Whether to include metadata in the export.</param>
        /// <param name="indent">Indentation level (None/null for no indentation, or number of spaces). Default is null.</param>
        /// <returns>True if export was successful, False otherwise.</returns>
        public async Task<bool> ExportToFileAsync(string filename, bool includeMetadata = true, int? indent = null)
        {
            try
            {
                string jsonData = ExportJson(includeMetadata, indent);
                await File.WriteAllTextAsync(filename, jsonData);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Exports the graph to a dictionary.
        /// </summary>
        /// <returns>Dictionary representation of the graph.</returns>
        public Dictionary<string, object> ExportToDict()
        {
            // Python: always includes metadata, even if source_kind is None
            return new Dictionary<string, object>
            {
                ["graph"] = new Dictionary<string, object>
                {
                    ["nodes"] = _nodes.Values.ToList(),
                    ["edges"] = _edges.Values.ToList()
                },
                ["metadata"] = new Dictionary<string, object?>
                {
                    ["source_kind"] = SourceKind ?? null
                }
            };
        }

        #endregion

        #region Import Methods

        /// <summary>
        /// Loads graph data from a JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing graph data.</param>
        /// <returns>True if load was successful, False otherwise.</returns>
        public bool ImportFromJson(string jsonData)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                return ImportFromJsonElement(doc.RootElement);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads graph data from a JSON file.
        /// </summary>
        /// <param name="filename">Name of the file to read.</param>
        /// <returns>True if load was successful, False otherwise.</returns>
        public bool ImportFromFile(string filename)
        {
            try
            {
                string jsonData = File.ReadAllText(filename);
                return ImportFromJson(jsonData);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads graph data from a JSON file asynchronously.
        /// </summary>
        /// <param name="filename">Name of the file to read.</param>
        /// <returns>True if load was successful, False otherwise.</returns>
        public async Task<bool> ImportFromFileAsync(string filename)
        {
            try
            {
                string jsonData = await File.ReadAllTextAsync(filename);
                return ImportFromJson(jsonData);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads graph data from a dictionary.
        /// Matches Python's import_from_dict() method.
        /// </summary>
        /// <param name="data">Dictionary containing graph data with "graph" and optional "metadata" keys.</param>
        /// <returns>True if load was successful, False otherwise.</returns>
        public bool ImportFromDict(Dictionary<string, object> data)
        {
            try
            {
                // Convert to JSON and re-parse through the JsonElement path for consistency
                string json = JsonSerializer.Serialize(data);
                return ImportFromJson(json);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads graph data from a JsonElement (typically from JSON).
        /// </summary>
        /// <param name="data">JsonElement containing graph data.</param>
        /// <returns>True if load was successful, False otherwise.</returns>
        private bool ImportFromJsonElement(JsonElement data)
        {
            try
            {
                if (!data.TryGetProperty("graph", out var graphElement))
                {
                    return false;
                }

                // Load nodes
                if (graphElement.TryGetProperty("nodes", out var nodesElement) && nodesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var nodeElement in nodesElement.EnumerateArray())
                    {
                        var node = NodeFromJson(nodeElement);
                        if (node != null)
                        {
                            _nodes[node.Id] = node;
                        }
                    }
                }

                // Load edges
                if (graphElement.TryGetProperty("edges", out var edgesElement) && edgesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var edgeElement in edgesElement.EnumerateArray())
                    {
                        var edge = EdgeFromJson(edgeElement);
                        if (edge != null)
                        {
                            string edgeKey = GetEdgeKey(edge);
                            _edges[edgeKey] = edge;
                        }
                    }
                }

                // Load metadata
                if (data.TryGetProperty("metadata", out var metadataElement))
                {
                    if (metadataElement.TryGetProperty("source_kind", out var sourceKindElement))
                    {
                        SourceKind = sourceKindElement.GetString();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a Node from a JsonElement.
        /// </summary>
        private Node? NodeFromJson(JsonElement nodeElement)
        {
            try
            {
                if (!nodeElement.TryGetProperty("id", out var idElement))
                {
                    return null;
                }

                string id = idElement.GetString() ?? "";
                var kinds = new List<string>();
                if (nodeElement.TryGetProperty("kinds", out var kindsElement) && kindsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var kindElement in kindsElement.EnumerateArray())
                    {
                        if (kindElement.ValueKind == JsonValueKind.String)
                        {
                            kinds.Add(kindElement.GetString() ?? "");
                        }
                    }
                }

                Properties? properties = null;
                if (nodeElement.TryGetProperty("properties", out var propsElement) && propsElement.ValueKind == JsonValueKind.Object)
                {
                    properties = new Properties();
                    foreach (var prop in propsElement.EnumerateObject())
                    {
                        properties.SetProperty(prop.Name, JsonValueToObject(prop.Value));
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
        /// Creates an Edge from a JsonElement.
        /// </summary>
        private Edge? EdgeFromJson(JsonElement edgeElement)
        {
            try
            {
                if (!edgeElement.TryGetProperty("kind", out var kindElement))
                {
                    return null;
                }

                string kind = kindElement.GetString() ?? "";

                if (!edgeElement.TryGetProperty("start", out var startElement) ||
                    !edgeElement.TryGetProperty("end", out var endElement))
                {
                    return null;
                }

                string startValue = startElement.TryGetProperty("value", out var startValueElement)
                    ? startValueElement.GetString() ?? ""
                    : "";
                string startMatchBy = startElement.TryGetProperty("match_by", out var startMatchByElement)
                    ? startMatchByElement.GetString() ?? "id"
                    : "id";
                string? startKind = startElement.TryGetProperty("kind", out var startKindElement)
                    ? startKindElement.GetString()
                    : null;

                string endValue = endElement.TryGetProperty("value", out var endValueElement)
                    ? endValueElement.GetString() ?? ""
                    : "";
                string endMatchBy = endElement.TryGetProperty("match_by", out var endMatchByElement)
                    ? endMatchByElement.GetString() ?? "id"
                    : "id";
                string? endKind = endElement.TryGetProperty("kind", out var endKindElement)
                    ? endKindElement.GetString()
                    : null;

                Properties? properties = null;
                if (edgeElement.TryGetProperty("properties", out var propsElement) && propsElement.ValueKind == JsonValueKind.Object)
                {
                    properties = new Properties();
                    foreach (var prop in propsElement.EnumerateObject())
                    {
                        properties.SetProperty(prop.Name, JsonValueToObject(prop.Value));
                    }
                }

                return new Edge(startValue, endValue, kind, properties, startMatchBy, endMatchBy, startKind, endKind);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a JsonElement value to an object.
        /// </summary>
        private object? JsonValueToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var intVal) ? intVal : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(JsonValueToObject).ToList(),
                _ => element.GetRawText()
            };
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Clears all nodes and edges from the graph.
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }

        /// <summary>
        /// Gets the total number of nodes and edges.
        /// </summary>
        /// <returns>Total number of nodes and edges.</returns>
        public int Count => _nodes.Count + _edges.Count;

        /// <summary>
        /// Returns a string representation of the graph.
        /// </summary>
        public override string ToString()
        {
            return $"OpenGraph(nodes={_nodes.Count}, edges={_edges.Count}, source_kind='{SourceKind}')";
        }

        #endregion
    }
}
