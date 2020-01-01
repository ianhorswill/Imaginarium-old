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
        [Tooltip("Name of the style.")]
        public string Name = "default";
        /// <summary>
        /// Color in which to draw the node.
        /// </summary>
        [Tooltip("Color in which to render the node")]
        public Color Color = new Color(1,1,1);
        /// <summary>
        /// Font in which to draw label.
        /// </summary>
        [Tooltip("Font in which to render the node")]
        public Font Font;
        /// <summary>
        /// Point size in which to draw label.
        /// </summary>
        [Tooltip("Size in which to render the node label")]
        public int FontSize = 12;
        /// <summary>
        /// The style in which to render the node's label.
        /// </summary>
        [Tooltip("Font style (e.g. italic) in which to render the node label.")]
        public FontStyle FontStyle = FontStyle.Normal;

        /// <summary>
        /// Prefab to use for making nodes
        /// </summary>
        [Tooltip("Prefab to instantiate to make a new node for this graph.  If None, use the default listed in the Graph.")]
        public GameObject Prefab;
    }
}
