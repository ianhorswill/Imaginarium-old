using System;
using UnityEngine;

namespace GraphVisualization
{
    /// <summary>
    /// Parameters for how to draw a graph node.
    /// </summary>
    [Serializable]
    public class NodeStyle
    {
        /// <summary>
        /// Name of the style
        /// </summary>
        public string Name = "default";
        /// <summary>
        /// Color in which to draw the node.
        /// </summary>
        public Color Color = new Color(1,1,1);
        /// <summary>
        /// Font in which to draw label.
        /// </summary>
        public Font Font;
        /// <summary>
        /// Point size in which to draw label.
        /// </summary>
        public int FontSize = 12;
        /// <summary>
        /// The style in which to render the node's label.
        /// </summary>
        public FontStyle FontStyle = FontStyle.Normal;
    }
}
