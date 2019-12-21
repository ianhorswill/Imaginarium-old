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

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace GraphVisualization
{
    public class Graph : Graphic
    {
        /// <summary>
        /// Styles available for drawing nodes in this graph
        /// </summary>
        public List<NodeStyle> NodeStyles = new List<NodeStyle>();
        /// <summary>
        /// Styles available for drawing edges in this graph
        /// </summary>
        public List<EdgeStyle> EdgeStyles = new List<EdgeStyle>();
        /// <summary>
        /// Prefab to use for making nodes
        /// </summary>
        public GameObject NodePrefab;
        /// <summary>
        /// Prefab to use for making edges
        /// </summary>
        public GameObject EdgePrefab;

        public float SpringStiffness = 1f;
        public float NodeDamping = 0.5f;

        public float GreyOutFactor = 0.5f;

        private readonly List<GraphNode> nodes = new List<GraphNode>();
        private readonly List<GraphEdge> edges = new List<GraphEdge>();

        private readonly Dictionary<object, GraphNode> nodeDict = new Dictionary<object, GraphNode>();
        GraphNode _selected;
        private bool selectionChanged;
        public GraphNode SelectedNode
        {
            get => _selected;
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    selectionChanged = true;
                }
            }
        }

        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
            nodeDict.Clear();

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            RepopulateMesh();
        }

        public GraphNode AddNode(object key, string label, NodeStyle style = null)
        {
            if (!nodeDict.ContainsKey(key))
            {
                if (label == null)
                    label = key.ToString();
                if (style == null)
                    style = NodeStyles[0];
                var go = Instantiate<GameObject>(NodePrefab, transform);
                go.name = label;
                var rect = rectTransform.rect;
                var node = go.GetComponent<GraphNode>();
                var position = new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
                node.Initialize(this, label, style, position);
                nodes.Add(node);
                nodeDict[key] = node;
            }

            return nodeDict[key];
        }

        public GraphEdge AddEdge(object start, object end, string label, float length, EdgeStyle style = null)
        {
            var startNode = AddNode(start, null);
            var endNode = AddNode(end, null);

            if (label == null)
                label = "";
            if (style == null)
                style = EdgeStyles[0];
            var go = Instantiate<GameObject>(EdgePrefab, transform);
            go.name = label;
            var rect = rectTransform.rect;
            var edge = go.GetComponent<GraphEdge>();
            edge.Initialize(startNode, endNode, label, length, style);
            edges.Add(edge);

            return edge;
        }

        //protected override void Start()
        //{
        //    base.Start();
        //    if (Application.isPlaying)
        //    {
        //        AddEdge("a", "b", "edge", 1000);
        //        AddEdge("a", "c", "edge", 1000);
        //        AddEdge("c", "b", "edge", 1000);
        //    }
        //}

        public void FixedUpdate()
        {
            UpdatePhysics();
        }

        public void Update()
        {
            if (selectionChanged)
            {
                RecolorNodes();
                selectionChanged = false;
            }
            RepopulateMesh();
        }

        private void RecolorNodes()
        {
            foreach (var n in nodes)
                n.Recolor();
        }

        #region Physics update
        void UpdatePhysics()
        {
            Rect bounds = rectTransform.rect;
            foreach (var n in nodes)
                n.NetForce = Vector2.zero;;
            foreach (var e in edges) 
                ApplySpringForce(e);
            foreach (var n in nodes)
            {
                UpdatePosition(n);
                n.Position = new Vector2(Mathf.Clamp(n.Position.x, bounds.xMin, bounds.xMax),
                    Mathf.Clamp(n.Position.y, bounds.yMin, bounds.yMax));
            }
        }

        private void UpdatePosition(GraphNode n)
        {
            if (n.IsBeingDragged)
                return;
            var saved = n.Position;
            n.Position = (2-NodeDamping) * n.Position - (1-NodeDamping) * n.PreviousPosition + (Time.fixedDeltaTime * Time.fixedDeltaTime) * n.NetForce;
            n.PreviousPosition = saved;
        }

        private void ApplySpringForce(GraphEdge e)
        {
            var offset = e.EndNode.Position - e.StartNode.Position;
            var len = offset.magnitude;
            if (len > 0.1f)
            {
                var lengthError = e.EquilibriumLength - len;
                var force = (SpringStiffness * lengthError / len) * offset;
                e.StartNode.NetForce -= force;
                e.EndNode.NetForce += force;
            }
        }

        #endregion

        #region Primitive rendering
        private void RepopulateMesh()
        {
            SetVerticesDirty();
        }

        private static readonly List<UIVertex> TriBuffer = new List<UIVertex>();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            void AddTri(Vector3 v1, Vector3 v2, Vector3 v3, Color c)
            {
                var uiv = UIVertex.simpleVert;
                uiv.color = c;
                uiv.position = v1;
                TriBuffer.Add(uiv);
                uiv.position = v2;
                TriBuffer.Add(uiv);
                uiv.position = v3;
                TriBuffer.Add(uiv);
            }

            void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c)
            {
                AddTri(v1, v2, v3, c);
                AddTri(v1, v3, v4, c);
            }

            void DrawEdge(Vector2 start, Vector2 end, EdgeStyle style, Color c)
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
                        c);

                    // Draw arrowhead if directed edge
                    if (style.IsDirected)
                    {
                        var arrowheadHalfWidthPerp = style.ArrowheadWidth * halfWidthPerp;
                        AddTri(end,
                            arrowheadBase - arrowheadHalfWidthPerp,
                            arrowheadBase + arrowheadHalfWidthPerp,
                            c);
                    }
                }
            }

            vh.Clear();
            TriBuffer.Clear();
            foreach (var e in edges)
            {
                var brightnessFactor = ((SelectedNode == null || SelectedNode == e.StartNode || SelectedNode == e.EndNode)?1:GreyOutFactor);
                DrawEdge(e.StartNode.Position, e.EndNode.Position,
                    e.Style,
                    e.Style.Color * brightnessFactor);
            }
            //for (var end = -1000; end <= 1000; end += 100)
            //    DrawArrow(new Vector2(0,0), new Vector2(end, 1000), 10, Color.red);
            //AddQuad(new Vector3(0, 0, 0), new Vector3(100, 0, 0), new Vector3(100, 100, 0), new Vector3(0, 100, 0), Color.white);
            vh.AddUIVertexTriangleStream(TriBuffer);
        }
        #endregion
    }
}
