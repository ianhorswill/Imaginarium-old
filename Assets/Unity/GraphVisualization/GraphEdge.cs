#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphEdge.cs" company="Ian Horswill">
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
    /// Component that drives individual edges in a Graph.
    /// These are created by Graph.AddEdge().  Do not instantiate one yourself.
    /// </summary>
    public class GraphEdge : UIBehaviour
    {
        public GraphNode StartNode;
        public GraphNode EndNode;
        public string Label;
        public EdgeStyle Style;
        private Text text;
        private RectTransform rectTransform;

        /// <summary>
        /// Called by Graph.AddEdge after object creation.
        /// </summary>
        public void Initialize(GraphNode startNode, GraphNode endNode, string label, EdgeStyle style)
        {
            this.StartNode = startNode;
            this.EndNode = endNode;
            this.Label = label;
            this.Style = style;
            text = GetComponent<Text>();
            text.text = label;
            if (style.Font != null)
                text.font = style.Font;
            if (style.FontSize != 0)
                text.fontSize = style.FontSize;
            text.fontStyle = style.FontStyle;
            rectTransform = GetComponent<RectTransform>();
            UpdatePosition();
        }

        /// <summary>
        /// Updates position and rotation of edge text based on positions of endpoints
        /// </summary>
        private void UpdatePosition()
        {
            // Move to midpoint of start and end
            var startPos = StartNode.Position;
            var endPos = EndNode.Position;
            rectTransform.localPosition = (startPos + endPos) * 0.5f;

            // Rotate parallel to line from start to end
            var offset = endPos - startPos;
            var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        }

        public void Update()
        {
            UpdatePosition();
        }

        /// <summary>
        /// Called to adjust color of edge label when selected node in the graph changes.
        /// When there is a selected node, edges not adjacent to it are greyed out.
        /// </summary>
        public void Recolor(Graph g)
        {
            text.color =
                (g.SelectedNode == null || StartNode == g.SelectedNode || EndNode == g.SelectedNode
                    ? 1
                    : g.GreyOutFactor)
                * Style.Color
                ;
        }
    }
}
