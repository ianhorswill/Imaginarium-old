using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphVisualization
{
    /// <summary>
    /// Strongly typed wrapper for Graph.AddNode and Graph.AddEdge.
    /// </summary>
    /// <typeparam name="NodeType"></typeparam>
    public class GraphBuilder<NodeType>
    {
        public readonly Graph Graph;

        public GraphBuilder(Graph graph)
        {
            Graph = graph;
        }

        /// <summary>
        /// Add a single node to the graph.
        /// </summary>
        /// <param name="node">Node to add</param>
        /// <param name="label">Label to attach to node</param>
        /// <param name="style">Style in which to render node, and apply physics to it.  If null, the first entry in NodeStyles will be used.</param>
        public void AddNode(NodeType node, string label, NodeStyle style = null)
        {
            Graph.AddNode(node, label, style);
        }

        /// <summary>
        /// Add a single edge to the graph.
        /// </summary>
        /// <param name="start">Node from which edge starts.</param>
        /// <param name="end">Node the edge leads to.</param>
        /// <param name="label">Label for the edge</param>
        /// <param name="style">Style in which to render the label.  If null, this will use the style whose name is the same as the label, if any, otherwise the first entry in EdgeStyles.</param>
        public void AddEdge(NodeType start, NodeType end, string label, EdgeStyle style = null)
        {
            Graph.AddEdge(start, end, label, style);
        }
    }
}
