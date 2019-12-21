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
    public class GraphEdge : UIBehaviour
    {
        public GraphNode StartNode;
        public GraphNode EndNode;
        public string Label;
        public EdgeStyle Style;
        public float EquilibriumLength;
        private Text text;
        private RectTransform rectTransform;

        public void Initialize(GraphNode startNode, GraphNode endNode, string label, float length, EdgeStyle style)
        {
            this.StartNode = startNode;
            this.EndNode = endNode;
            this.Label = label;
            this.Style = style;
            this.EquilibriumLength = length;
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
    }
}
