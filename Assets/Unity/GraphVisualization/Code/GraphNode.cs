#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphNode.cs" company="Ian Horswill">
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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GraphVisualization
{
    /// <summary>
    /// Component that drives individual nodes in a Graph visualization.
    /// These are created by Graph.AddNode().  Do not instantiate one yourself.
    /// </summary>
    public class GraphNode : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, INodeDriver
    {
        /// <summary>
        /// The client-side object associated with this node.
        /// </summary>
        public object Key;
        public string Label;
        public NodeStyle Style;
        private RectTransform rectTransform;
        private Graph graph;
        private Text labelMesh;

        /// <summary>
        /// The current position as computed by the spring physics system in Graph.cs
        /// </summary>
        internal Vector2 Position;
        /// <summary>
        /// The previous position as computed by the spring physics system in Graph.cs
        /// </summary>
        internal Vector2 PreviousPosition;
        /// <summary>
        /// Net force applied to this node by the various springs
        /// </summary>
        public Vector2 NetForce;

        /// <summary>
        /// True if this node is in the process of being dragged by the mouse
        /// </summary>
        public bool IsBeingDragged;

        /// <summary>
        /// Called from Graph.AddNode after instantiation of the prefab for this node.
        /// </summary>
        public void Initialize(Graph g, object key, string label, NodeStyle style, Vector2 position)
        {
            graph = g;
            Key = key;
            Label = label;
            Style = style;
            labelMesh = GetComponent<Text>();
            if (labelMesh != null)
            {
                labelMesh.text = label;
                labelMesh.color = style.Color;
                labelMesh.fontSize = style.FontSize;
                if (style.Font != null)
                    labelMesh.font = style.Font;
                labelMesh.fontStyle = style.FontStyle;
            }

            rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = PreviousPosition = position;
        }

        public void Update()
        {
            // Set gameObject's position to that computed by spring physics
            Vector3 p = Position;
            p.z = Foreground ? -1 : 1;
            rectTransform.localPosition = p;
        }

        /// <summary>
        /// Update color of node text, based on whether it has been selected by the user.
        /// Called when node selected by mouse changes
        /// </summary>
        public void SelectionChanged(Graph g, GraphNode selected)
        {
            if (labelMesh != null)
                labelMesh.color = (Foreground ? 1 : graph.GreyOutFactor) * Style.Color;
        }

        /// <summary>
        /// True if this node is in the foreground.
        /// Nodes are in the foreground unless some node they aren't adjacent to has been selected.
        /// </summary>
        private bool Foreground =>
            graph.SelectedNode == this || graph.SelectedNode == null ||
            graph.Adjacent(this, graph.SelectedNode);

        public void OnPointerEnter(PointerEventData eventData)
        {
            graph.SelectedNode = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (graph.SelectedNode == this)
                graph.SelectedNode = null;
        }

        public void OnDrag(PointerEventData data)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, data.position, null, out var p))
                PreviousPosition = Position = p;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsBeingDragged = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsBeingDragged = false;
        }
    }
}
