#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Graph.cs" company="Ian Horswill">
// Copyright (C) 2019 Ian Horswill
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace GraphVisualization
{
    /// <summary>
    /// An interactive graph visualization packaged as a Unity UI element
    /// </summary>
    public class Graph : Graphic
    {
        #region Editor-visible fields
        /// <summary>
        /// Styles available for drawing nodes in this graph
        /// </summary>
        [Tooltip("Styles in which to render nodes.")]
        public List<NodeStyle> NodeStyles = new List<NodeStyle>();

        /// <summary>
        /// Styles available for drawing edges in this graph
        /// </summary>
        [Tooltip("Styles in which to render edges.")]
        public List<EdgeStyle> EdgeStyles = new List<EdgeStyle>();

        /// <summary>
        /// The NodeStyle with the specified name, or null
        /// </summary>
        public NodeStyle NodeStyleNamed(string styleName)
        {
            return NodeStyles.Find(s => s.Name == styleName);
        }

        /// <summary>
        /// The EdgeStyle with the specified name, or null
        /// </summary>
        public EdgeStyle EdgeStyleNamed(string styleName)
        {
            return EdgeStyles.Find(s => s.Name == styleName);
        }

        /// <summary>
        /// Prefab to use for making nodes
        /// </summary>
        [Tooltip("Prefab to instantiate to make a new node for this graph.  Prefab should include a Text field.")]
        public GameObject NodePrefab;
        /// <summary>
        /// Prefab to use for making edges
        /// </summary>
        [Tooltip("Prefab to instantiate to make a new edge for this graph.  Prefab should include a Text field.")]
        public GameObject EdgePrefab;
        
        /// <summary>
        /// The strength of the force that moves adjacent nodes together
        /// </summary>
        [Tooltip("The strength of the force that moves adjacent nodes together")]
        public float SpringStiffness = 1f;
        /// <summary>
        /// The strength of the force that moves non-adjacent nodes apart.
        /// </summary>
        [Tooltip("The strength of the force that moves non-adjacent nodes apart.  Set this to 0 to eliminate repulsion calculations.")]
        public float RepulsionGain = 100000;
        /// <summary>
        /// Rate (0-1) at which nodes slow down when no forces are applied to them.
        /// </summary>
        [Tooltip("Rate (0-1) at which nodes slow down when no forces are applied to them.")]
        public float NodeDamping = 0.5f;

        /// <summary>
        /// Degree to which nodes and edges are dimmed when some other node is selected.
        /// 0 = completely dimmed, 1 = not dimmed.
        /// </summary>
        [Tooltip("Degree to which nodes and edges are dimmed when some other node is selected.  0 = completely dimmed, 1 = not dimmed.")]
        public float GreyOutFactor = 0.5f;
        /// <summary>
        /// How far to keep nodes from the edge of the Rect for this UI element.
        /// </summary>
        public float Border = 100;
        /// <summary>
        /// Text object in which to display additional information about a node, or null if no info to be displayed.
        /// </summary>
        [Tooltip("Text object in which to display additional information about a node, or None if no info to be displayed.")]
        public Text ToolTip;
        /// <summary>
        /// Name of the string property of a selected node to be displayed in the ToolTop element.
        /// </summary>
        [Tooltip("Name of the string property of a selected node to be displayed in the ToolTop element.")]
        public string ToolTipProperty;
        #endregion

        #region Node and edge data structures
        /// <summary>
        /// All GraphNode objects in this Graph, one per node/key
        /// </summary>
        private readonly List<GraphNode> nodes = new List<GraphNode>();
        /// <summary>
        /// All GraphEdge objects in this Graph, one per graph edge
        /// </summary>
        private readonly List<GraphEdge> edges = new List<GraphEdge>();

        /// <summary>
        /// Mapping from client-side vertex objects ("keys") to internal GraphNode objects
        /// </summary>
        private readonly Dictionary<object, GraphNode> nodeDict = new Dictionary<object, GraphNode>();
        /// <summary>
        /// Set of pairs of nodes that are adjacent.  This relation is symmetric even when the edge is directed.
        /// Used to determine if nodes should repel one another, and if nodes should be dimmed when another node is selected.
        /// </summary>
        private readonly HashSet<(GraphNode, GraphNode)> adjacency = new HashSet<(GraphNode, GraphNode)>();

        /// <summary>
        /// True if there is an edge from a to be *or* vice-versa.
        /// </summary>
        public bool Adjacent(GraphNode a, GraphNode b)
        {
            return adjacency.Contains((a, b));
        }
        #endregion

        #region Graph creation
        /// <summary>
        /// Remove all existing nodes and edges from graph
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
            nodeDict.Clear();
            adjacency.Clear();

            foreach (Transform child in transform)
            {
                if (child.GetComponent<GraphNode>() != null || child.GetComponent<GraphEdge>() != null)
                    Destroy(child.gameObject);
            }

            RepopulateMesh();
        }

        public void GenerateFrom(IEnumerable keys, NodeFormatter format, EdgeGenerator edgeGenerator)
        {
            void MakeNode(object k)
            {
                var (label, style) = format(k);
                AddNode(k, label, style);
            }

            void WalkGeneration(object k)
            { 
                // Add node
                MakeNode(k);

                // Add edges and recurse
                foreach (var (f, t, l, s) in edgeGenerator(k))
                {
                    bool fWalked = nodeDict.ContainsKey(f);
                    if (!fWalked)
                        MakeNode(f);
                    bool tWalked = nodeDict.ContainsKey(t);
                    if (!tWalked)
                        MakeNode(t);
                    AddEdge(f, t, l, s);
                    if (!fWalked)
                        WalkGeneration(f);
                    if (!tWalked)
                        WalkGeneration(t);
                }
            }

            foreach (var k in keys)
                if (!nodeDict.ContainsKey(k))
                    WalkGeneration(k);
        }

        /// <summary>
        /// A procedure to be used by GenerateFrom() to enumerate edges of a given node
        /// </summary>
        /// <param name="node">Node to generate edges for.</param>
        /// <returns>Enumerated stream of edge information: from-node, to-node, label, and style.</returns>
        public delegate IEnumerable<(object, object, string, EdgeStyle)> EdgeGenerator(object node);
        /// <summary>
        /// A procedure to be used by GenerateFrom() to generate labels and styles for nodes
        /// </summary>
        /// <param name="o">Node</param>
        /// <returns>Label and style for the node</returns>
        public delegate (string, NodeStyle) NodeFormatter(object o);

        /// <summary>
        /// Add a single node to the graph.
        /// </summary>
        /// <param name="node">Node to add</param>
        /// <param name="label">Label to attach to node</param>
        /// <param name="style">Style in which to render node, and apply physics to it.  If null, the first entry in NodeStyles will be used.</param>
        public void AddNode(object node, string label, NodeStyle style = null)
        {
            if (!nodeDict.ContainsKey(node))
            {
                if (label == null)
                    label = node.ToString();
                if (style == null)
                    style = NodeStyles[0];
                var go = Instantiate(NodePrefab, transform);
                go.name = label;
                var rect = rectTransform.rect;
                var internalNode = go.GetComponent<GraphNode>();
                var position = new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
                internalNode.Initialize(this, node, label, style, position);
                nodes.Add(internalNode);
                nodeDict[node] = internalNode;
            }
        }

        /// <summary>
        /// Add a single edge to the graph.
        /// </summary>
        /// <param name="start">Node from which edge starts.</param>
        /// <param name="end">Node the edge leads to.</param>
        /// <param name="label">Label for the edge</param>
        /// <param name="style">Style in which to render the label.  If null, this will use the style whose name is the same as the label, if any, otherwise the first entry in EdgeStyles.</param>
        public void AddEdge(object start, object end, string label, EdgeStyle style = null)
        {
            AddNode(start, null);  // In case it isn't already defined.
            var startNode = nodeDict[start];
            AddNode(end, null);    // In case it isn't already defined.
            var endNode = nodeDict[end];

            if (label == null)
                label = "";
            if (style == null)
                style = EdgeStyleNamed(label)??EdgeStyles[0];
            var go = Instantiate(EdgePrefab, transform);
            go.name = label;
            var edge = go.GetComponent<GraphEdge>();
            edge.Initialize(startNode, endNode, label, style);
            edges.Add(edge);

            adjacency.Add((startNode, endNode));
            adjacency.Add((endNode, startNode));
        }
        #endregion

        #region Unity message handlers
        /// <summary>
        /// Update physics simulation of nodes
        /// </summary>
        public void FixedUpdate()
        {
            UpdatePhysics();
        }

        /// <summary>
        /// Update display of nodes and edges
        /// </summary>
        public void Update()
        {
            if (selectionChanged)
            {
                Recolor();
                selectionChanged = false;
            }
            RepopulateMesh();
        }

        /// <summary>
        /// Call IGraphGenerator in this game object, if any.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            var generator = GetComponent<IGraphGenerator>();
            if (Application.isPlaying && generator != null)
            {
                Clear();
                generator.GenerateGraph(this);
            }
        }
        #endregion

        #region Highlighting and tooltip handling
        /// <summary>
        /// Do not use this directly.
        /// Internal field backing the SelectedNode property
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private GraphNode _selected;
        /// <summary>
        /// True if SelectedNode has changed since the last frame Update.
        /// </summary>
        private bool selectionChanged;
        /// <summary>
        /// Node over which the mouse is currently hovering, if any.  Else null.
        /// </summary>
        public GraphNode SelectedNode
        {
            get => _selected;
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    selectionChanged = true;
                    UpdateToolTip(value);
                }
            }
        }

        /// <summary>
        /// Update the ToolTop UI element, if any, based on the selected node, if any.
        /// </summary>
        /// <param name="node"></param>
        private void UpdateToolTip(GraphNode node)
        {
            if (ToolTip == null)
                return;
            if (node == null)
                ToolTip.text = "";
            else
            {
                var key = node.Key;
                var t = key.GetType();
                var text = (string)t.InvokeMember(ToolTipProperty, BindingFlags.GetProperty, null, key, null);
                ToolTip.text = text;
            }
        }

        /// <summary>
        /// Dim/undim nodes based on selected node.
        /// </summary>
        private void Recolor()
        {
            foreach (var n in nodes)
                n.Recolor();
            foreach (var e in edges)
                e.Recolor(this);
        }
        #endregion
        
        #region Physics update
        /// <summary>
        /// The "ideal" length for edges.
        /// This is the length we'd have if all the nodes were arrayed in a regular grid.
        /// </summary>
        private float targetEdgeLength;

        /// <summary>
        /// Compute forces on nodes and update their positions.
        /// This just updates the internal Position field of the GraphNodes.  The actual
        /// on-screen position is updated once per frame in the Update method.
        /// </summary>
        void UpdatePhysics()
        {
            Rect bounds = rectTransform.rect;
            targetEdgeLength = 1.5f * Mathf.Sqrt(bounds.width * bounds.height / nodes.Count);

            foreach (var n in nodes)
                n.NetForce = Vector2.zero;

            // Pull adjacent nodes together
            foreach (var e in edges) 
                ApplySpringForce(e);

            if (RepulsionGain > 0)
            {
                // Push non-adjacent nodes apart
                foreach (var a in nodes)
                foreach (var b in nodes)
                {
                    if (a != b && !Adjacent(a, b))
                        PushApart(a, b);
                }
            }

            // Keep nodes on screen
            foreach (var n in nodes)
            {
                UpdatePosition(n);
                n.Position = new Vector2(
                    Mathf.Clamp(n.Position.x, bounds.xMin+Border, bounds.xMax-Border),
                    Mathf.Clamp(n.Position.y, bounds.yMin+Border, bounds.yMax-Border));
            }
        }

        /// <summary>
        /// Update position of a single node based on forces already computed.
        /// </summary>
        /// <param name="n"></param>
        private void UpdatePosition(GraphNode n)
        {
            if (n.IsBeingDragged)
                return;
            var saved = n.Position;
            n.Position = (2-NodeDamping) * n.Position - (1-NodeDamping) * n.PreviousPosition + (Time.fixedDeltaTime * Time.fixedDeltaTime) * n.NetForce;
            n.PreviousPosition = saved;
        }
        
        /// <summary>
        /// Apply a repulsive force between two non-adjacent nodes.
        /// </summary>
        private void PushApart(GraphNode a, GraphNode b)
        {
            var offset = (a.Position - b.Position);
            var force = (RepulsionGain / Mathf.Max(1,offset.sqrMagnitude)) * offset;

            a.NetForce += force;
            b.NetForce -= force;
        }

        /// <summary>
        /// Apply a spring force to two adjacent nodes to move them closer to targetEdgeLength.
        /// </summary>
        /// <param name="e">Edge connecting nodes</param>
        private void ApplySpringForce(GraphEdge e)
        {
            var offset = e.EndNode.Position - e.StartNode.Position;
            var len = offset.magnitude;
            if (len > 0.1f)
            {
                var lengthError = targetEdgeLength - len;
                var force = (SpringStiffness * lengthError / len) * offset;

                e.StartNode.NetForce -= force;
                e.EndNode.NetForce += force;
            }
        }
        #endregion

        #region Edge rendering
        /// <summary>
        /// List of triangles to render.  Used as scratch by OnPopulateMesh.
        /// </summary>
        private static readonly List<UIVertex> TriBuffer = new List<UIVertex>();

        /// <summary>
        /// Recompute triangles for lines and arrowheads representing edges
        /// </summary>
        /// <param name="vh">VertexHelper object passed in by Unity</param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // Add a solid-colored tri to TriBuffer.
            void AddTri(Vector2 v1, Vector2 v2, Vector2 v3, float z, Color c)
            {
                var uiv = UIVertex.simpleVert;
                uiv.color = c;
                uiv.position = new Vector3(v1.x, v1.y, z);
                TriBuffer.Add(uiv);
                uiv.position = new Vector3(v2.x, v2.y, z);
                TriBuffer.Add(uiv);
                uiv.position = new Vector3(v3.x, v3.y, z);
                TriBuffer.Add(uiv);
            }

            // A solid-colored quad to TriBuffer
            void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float z, Color c)
            {
                AddTri(v1, v2, v3, z, c);
                AddTri(v1, v3, v4, z, c);
            }

            // Add the representation of an edge (line or arrow) to TriBuffer
            void DrawEdge(Vector2 start, Vector2 end, EdgeStyle style, float z, Color c)
            {
                var offset = end - start;
                var length = offset.magnitude;
                if (length > 1)  // arrows less than one pixel long will disappear
                {
                    // Draw line connecting start and end
                    var unit = offset / length;
                    var perp = new Vector2(unit.y, -unit.x);
                    var halfWidthPerp = (style.LineWidth * 0.5f) * perp;
                    var arrowheadBase = end - (style.ArrowheadLength * style.LineWidth) * unit;

                    AddQuad(start + halfWidthPerp,
                        arrowheadBase + halfWidthPerp,
                        arrowheadBase-halfWidthPerp,
                        start-halfWidthPerp,
                        z,
                        c);

                    // Draw arrowhead if directed edge
                    if (style.IsDirected)
                    {
                        var arrowheadHalfWidthPerp = style.ArrowheadWidth * halfWidthPerp;
                        AddTri(end,
                            arrowheadBase - arrowheadHalfWidthPerp,
                            arrowheadBase + arrowheadHalfWidthPerp,
                            z,
                            c);
                    }
                }
            }

            // Throw away previous geometry
            vh.Clear();
            TriBuffer.Clear();
            // Add edges to TriBuffer
            foreach (var e in edges)
            {
                var foreground = SelectedNode == null || SelectedNode == e.StartNode || SelectedNode == e.EndNode;
                var brightnessFactor = foreground?1:GreyOutFactor;
                DrawEdge(e.StartNode.Position, e.EndNode.Position,
                    e.Style,
                    foreground ? 0 : 1,
                    e.Style.Color * brightnessFactor);
            }
            // Add TriBuffer to vh.
            vh.AddUIVertexTriangleStream(TriBuffer);
        }

        /// <summary>
        /// Tell Unity we need to recompute the mesh.
        /// </summary>
        private void RepopulateMesh()
        {
            SetVerticesDirty();
        }
        #endregion
    }
}
