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
        [Tooltip("Name of the style.  This will be matched against the label of an edge to select the style.")]
        public string Name = "default";
        /// <summary>
        /// Color in which to draw the edge.
        /// </summary>
        [Tooltip("Color in which to render the edge")]
        public Color Color = new Color(1,1,1);
        /// <summary>
        /// Width of the line to draw, in pixels.
        /// </summary>
        [Tooltip("Width of the line, in pixels, to use to render the edge.")]
        public float LineWidth = 1;
        /// <summary>
        /// Whether this is a directed edge.
        /// Directed edges will be drawn as arrows, undirected edges as lines.
        /// </summary>
        [Tooltip("If true, the edge will be drawn as an arrow, otherwise a plain line.")]
        public bool IsDirected = true;
        /// <summary>
        /// Length of the arrowhead, if this is a directed edge
        /// </summary>
        [Tooltip("Length of the arrowhead, in multiples of line width.")]
        public float ArrowheadLength = 6;
        /// <summary>
        /// width of the arrowhead, if this is a directed edge.
        /// </summary>
        [Tooltip("Width of the arrowhead, in multiples of line width")]
        public float ArrowheadWidth = 4;
        /// <summary>
        /// Font in which to draw label.
        /// </summary>
        [Tooltip("Font for label")]
        public Font Font;
        /// <summary>
        /// Point size in which to draw label.
        /// </summary>
        [Tooltip("Side for label, in pixels")]
        public int FontSize = 12;
        /// <summary>
        /// Color in which to draw Label, if different from Color.
        /// </summary>
        [Tooltip("Color for label, if different from color for arrow.")]
        public Color? LabelColor = null;
        /// <summary>
        /// The style in which to render the label.
        /// </summary>
        [Tooltip("FontStyle (e.g. italic) in which to render the label.")]
        public FontStyle FontStyle = FontStyle.Normal;
        
        /// <summary>
        /// Copy the style
        /// </summary>
        public EdgeStyle Clone()
        {
            return (EdgeStyle)MemberwiseClone();
        }
    }
}
