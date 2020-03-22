namespace GraphVisualization
{
    /// <summary>
    /// Interface for components that fill a Graph visualization on demand.
    /// </summary>
    public interface IGraphGenerator
    {
        /// <summary>
        /// Called upon graph creation.  This should populate the graph with nodes and edges.
        /// </summary>
        /// <param name="graph">Graph component for which to generate nodes and edges.</param>
        void GenerateGraph(Graph graph);
    }
}
