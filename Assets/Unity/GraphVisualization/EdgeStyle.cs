using System;
using UnityEngine;

namespace GraphVisualization
{
    /// <summary>
    /// Parameters for how to draw a graph edge.
    /// </summary>
    [Serializable]
    public class EdgeStyle
    {
        /// <summary>
        /// Name of the style
        /// </summary>
        public string Name = "default";
        /// <summary>
        /// Color in which to draw the edge.
        /// </summary>
        public Color Color = new Color(1,1,1);
        /// <summary>
        /// Width of the line to draw, in pixels.
        /// </summary>
        public float LineWidth = 1;
        /// <summary>
        /// Whether this is a directed edge.
        /// Directed edges will be drawn as arrows, undirected edges as lines.
        /// </summary>
        public bool IsDirected = true;
        /// <summary>
        /// Length of the arrowhead, if this is a directed edge
        /// </summary>
        public float ArrowheadLength = 6;
        /// <summary>
        /// width of the arrowhead, if this is a directed edge.
        /// </summary>
        public float ArrowheadWidth = 4;
        /// <summary>
        /// Font in which to draw label.
        /// </summary>
        public Font Font;
        /// <summary>
        /// Point size in which to draw label.
        /// </summary>
        public int FontSize = 12;
        /// <summary>
        /// Color in which to draw Label, if different from Color.
        /// </summary>
        public Color? LabelColor = null;
        /// <summary>
        /// The style in which to render the label.
        /// </summary>
        public FontStyle FontStyle = FontStyle.Normal;

        public EdgeStyle Clone()
        {
            return (EdgeStyle)MemberwiseClone();
        }
    }
}
