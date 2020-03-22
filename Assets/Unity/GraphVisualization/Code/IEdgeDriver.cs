using UnityEngine;
using UnityEngine.UI;

namespace GraphVisualization
{
    /// <summary>
    /// Interface for components that fill a Graph visualization on demand.
    /// </summary>
    public interface IEdgeDriver
    {
        /// <summary>
        /// Called by Graph.AddEdge after object creation.
        /// </summary>
        /// <param name="g">The Graph to which this edge belongs</param>
        /// <param name="startNode">The GraphNode from which this edge originates</param>
        /// <param name="endNode">The GraphNode at which this edge terminates</param>
        /// <param name="label">Label of the edge</param>
        /// <param name="style">Style in which to render the edge</param>
        void Initialize(Graph g, GraphNode startNode, GraphNode endNode, string label, EdgeStyle style);

        /// <summary>
        /// Called when the mouse hovers over a new node
        /// </summary>
        /// <param name="graph">Graph to which this edge belongs</param>
        /// <param name="selectedNode">GraphNode over which the mouse is hovering</param>
        void SelectionChanged(Graph graph, GraphNode selectedNode);
    }
}