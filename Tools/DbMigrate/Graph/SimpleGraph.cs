using System.Collections.Generic;

namespace Eulg.Common.Graph
{
    internal sealed class SimpleGraph<TNode> : IGraph<TNode> where TNode : class
    {
        private readonly IGraph<TNode> _impl;

        public SimpleGraph(IEnumerable<TNode> nodes, bool directed)
        {
            _impl = new Graph<TNode, object>(nodes, directed);
        }

        public bool IsDirected => _impl.IsDirected;

        IEnumerable<INode> IGraph.Nodes => Nodes;
        public IEnumerable<INode<TNode>> Nodes => _impl.Nodes;

        IEnumerable<IEdge> IGraph.Edges => Edges;
        public IEnumerable<IEdge<TNode>> Edges => _impl.Edges;

        public INode<TNode> this[TNode data] => _impl[data];
    }
}
