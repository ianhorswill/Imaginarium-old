using UnityEngine;

namespace GraphVisualization
{
    /// <summary>
    /// Interface for components that fill a Graph visualization on demand.
    /// </summary>
    public interface INodeDriver
    {
        /// <summary>
        /// Called from Graph.AddNode after instantiation of the prefab for this node.
        /// </summary>
        /// <param name="g">The Graph to which this node belongs</param>
        /// <param name="nodeKey">The object the client identified as the node</param>
        /// <param name="label">The label attached to this node</param>
        /// <param name="style">The style in which to render this node</param>
        /// <param name="position">The position in which to render this node</param>
        void Initialize(Graph g, object nodeKey, string label, NodeStyle style, Vector2 position);

        /// <summary>
        /// Called when the mouse hovers over a new node
        /// </summary>
        /// <param name="graph">Graph to which this edge belongs</param>
        /// <param name="selectedNode">GraphNode over which the mouse is hovering</param>
        void SelectionChanged(Graph graph, GraphNode selectedNode);
    }
}