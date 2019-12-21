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
    public class GraphNode : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public string Label;
        public NodeStyle Style;
        private RectTransform rectTransform;
        private Graph graph;

        public Vector2 Position, PreviousPosition;
        public Vector2 NetForce;
        private Text labelMesh;

        public bool IsBeingDragged;

        public void Initialize(Graph graph, string label, NodeStyle style, Vector2 position)
        {
            this.graph = graph;
            labelMesh = GetComponent<Text>();
            labelMesh.text = label;
            labelMesh.color = style.Color;
            labelMesh.fontSize = style.FontSize;
            if (style.Font != null)
                labelMesh.font = style.Font;
            labelMesh.fontStyle = style.FontStyle;
            rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = PreviousPosition = position;
        }

        public void Update()
        {
            rectTransform.localPosition = Position;
        }

        public void Recolor()
        {
            labelMesh.color = ((graph.SelectedNode == this || graph.SelectedNode == null) ? 1 : graph.GreyOutFactor) * Style.Color;
        }

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
