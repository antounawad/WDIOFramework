using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eulg.Common.Graph
{
    internal sealed class Graph<TNode, TEdge> : IMutableGraph<TNode, TEdge> where TNode : class where TEdge : class
    {
        #region Class: Node

        [DebuggerDisplay("{" + nameof(Data) + "}")]
        private class Node : INode<TNode, TEdge>
        {
            private readonly WeakReference<Graph<TNode, TEdge>> _graphRef;
            private readonly Func<uint, uint, ProtoEdge> _edgeGetter;

            public Node(TNode data, uint id, Func<uint, uint, ProtoEdge> edgeGetter, Graph<TNode, TEdge> owner)
            {
                Index = id;
                Data = data;
                _edgeGetter = edgeGetter;
                _graphRef = new WeakReference<Graph<TNode, TEdge>>(owner);
            }

            public uint Index { get; }

            public TNode Data { get; }

            private Graph<TNode, TEdge> Owner
            {
                get
                {
                    if (_graphRef.TryGetTarget(out var owner))
                    {
                        return owner;
                    }

                    throw new ObjectDisposedException("Graph", "The graph that this node belongs to has been finalized");
                }
            }

            IEnumerable<INode> INode.Predecessors => Predecessors;
            IEnumerable<INode<TNode>> INode<TNode>.Predecessors => Predecessors;
            public IEnumerable<INode<TNode, TEdge>> Predecessors => Edges.Where(e => !e.IsDirected || e.Node1 == this).Select(e => e.Node0 == this ? e.Node1 : e.Node0);

            IEnumerable<INode> INode.Successors => Successors;
            IEnumerable<INode<TNode>> INode<TNode>.Successors => Successors;
            public IEnumerable<INode<TNode, TEdge>> Successors => Edges.Where(e => !e.IsDirected || e.Node0 == this).Select(e => e.Node0 == this ? e.Node1 : e.Node0);

            IProtoEdge<TNode> INode<TNode>.this[TNode adjacent] => _edgeGetter(Index, Owner._nodeMap[adjacent]);
            IProtoEdge<TNode, TEdge> INode<TNode, TEdge>.this[TNode adjacent] => _edgeGetter(Index, Owner._nodeMap[adjacent]);

            public IProtoEdge this[INode adjacent] => GetEdgeByNode(adjacent);
            IProtoEdge<TNode> INode<TNode>.this[INode adjacent] => GetEdgeByNode(adjacent);
            IProtoEdge<TNode, TEdge> INode<TNode, TEdge>.this[INode adjacent] => GetEdgeByNode(adjacent);

            IEnumerable<INode> INode.Adjacent => Adjacent;
            IEnumerable<INode<TNode>> INode<TNode>.Adjacent => Adjacent;
            public IEnumerable<INode<TNode, TEdge>> Adjacent
            {
                get
                {
                    var owner = Owner;
                    var nodes = owner._nodes;
                    return owner._edges.Keys.Select(GetConnectedNodeIndex).OfType<uint>().Select(i => nodes[i]);
                }
            }

            IEnumerable<IEdge> INode.Edges => Edges;
            IEnumerable<IEdge<TNode>> INode<TNode>.Edges => Edges;
            public IEnumerable<IEdge<TNode, TEdge>> Edges => Owner._edges.Keys.Select(GetEdgeIfConnected).Where(e => e != null);

            IEnumerable<IEdge> INode.Incoming => Incoming;
            IEnumerable<IEdge<TNode>> INode<TNode>.Incoming => Incoming;
            public IEnumerable<IEdge<TNode, TEdge>> Incoming => Edges.Where(e => !e.IsDirected || e.Node1 == this);

            IEnumerable<IEdge> INode.Outgoing => Outgoing;
            IEnumerable<IEdge<TNode>> INode<TNode>.Outgoing => Outgoing;
            public IEnumerable<IEdge<TNode, TEdge>> Outgoing => Edges.Where(e => !e.IsDirected || e.Node0 == this);

            private IProtoEdge<TNode, TEdge> GetEdgeByNode(INode node)
            {
                return _edgeGetter(Index, ((Node)node).Index);
            }

            private uint? GetConnectedNodeIndex(ulong edgeKey)
            {
                var n0 = (uint)(edgeKey >> 32);
                var n1 = (uint)(edgeKey & uint.MaxValue);

                return n0 == Index
                    ? n1
                    : n1 == Index
                        ? n0
                        : (uint?)null;
            }

            private IEdge<TNode, TEdge> GetEdgeIfConnected(ulong edgeKey)
            {
                var n0 = (uint)(edgeKey >> 32);
                var n1 = (uint)(edgeKey & uint.MaxValue);

                return n0 == Index || n1 == Index ? Owner._edges[edgeKey] : null;
            }
        }

        #endregion

        #region Class: Edge

        [DebuggerDisplay("{" + nameof(DebugString) + "}")]
        private class Edge : IEdge<TNode, TEdge>
        {
            private readonly uint _id0, _id1;
            private readonly Func<uint, Node> _getter;

            public Edge(uint n0, uint n1, TEdge data, Func<uint, Node> getter, bool isDirected)
            {
                Data = data;
                _id0 = n0;
                _id1 = n1;
                _getter = getter;
                IsDirected = isDirected;
            }

            public TEdge Data { get; }
            public bool IsDirected { get; }

            INode IEdge.Node0 => Node0;
            INode<TNode> IEdge<TNode>.Node0 => Node0;
            public INode<TNode, TEdge> Node0 => _getter(_id0);

            INode IEdge.Node1 => Node1;
            INode<TNode> IEdge<TNode>.Node1 => Node1;
            public INode<TNode, TEdge> Node1 => _getter(_id1);

            public void Remove()
            {
                Node0[Node1].Remove();
            }

            private string DebugString
            {
                get { return Data is IEnumerable enumerable ? "[" + string.Join(", ", enumerable.Cast<object>().Select(_ => _.ToString())) + "]" : Data.ToString(); }
            }
        }

        #endregion

        #region Class: ProtoEdge

        private class ProtoEdge : IProtoEdge<TNode, TEdge>
        {
            private readonly uint _node0, _node1;
            private readonly IDictionary<ulong, Edge> _edges;
            private readonly Func<uint, Node> _nodes;

            public ProtoEdge(uint n0, uint n1, IDictionary<ulong, Edge> edges, Func<uint, Node> nodes, bool directed)
            {
                _node0 = n0;
                _node1 = n1;
                _edges = edges;
                _nodes = nodes;
                IsDirected = directed;
            }

            public bool Exists => _edges.ContainsKey(GetEdgeKey(_node0, _node1, IsDirected));
            public bool IsDirected { get; }

            IEdge IProtoEdge.Get() => Get();

            IEdge<TNode> IProtoEdge<TNode>.Get() => Get();

            public IEdge<TNode, TEdge> Get()
            {
                return _edges[GetEdgeKey(_node0, _node1, IsDirected)];
            }

            public void Add()
            {
                Add(default(TEdge));
            }

            public void Add(TEdge data)
            {
                if(Exists) throw new InvalidOperationException();
                _edges.Add(GetEdgeKey(_node0, _node1, IsDirected), new Edge(_node0, _node1, data, _nodes, IsDirected));
            }

            public bool Remove()
            {
                return _edges.Remove(GetEdgeKey(_node0, _node1, IsDirected));
            }
        }

        #endregion

        private readonly IDictionary<ulong, Edge> _edges;
        private readonly IDictionary<TNode, uint> _nodeMap;
        private readonly SortedList<uint, Node> _nodes;
        private uint _nextNodeId;

        public Graph(IEnumerable<TNode> nodes, bool directed)
        {
            _edges = new Dictionary<ulong, Edge>();
            _nodes = new SortedList<uint, Node>(nodes.Select(item => new Node(item, ++_nextNodeId, GetProtoEdge, this)).ToDictionary(n => n.Index, n => n));
            _nodeMap = _nodes.ToDictionary(n => n.Value.Data, n => n.Key);
            IsDirected = directed;
        }

        public bool IsDirected { get; }

        IEnumerable<INode> IGraph.Nodes => Nodes;
        IEnumerable<INode<TNode>> IGraph<TNode>.Nodes => Nodes;
        public IEnumerable<INode<TNode, TEdge>> Nodes => _nodes.Values;

        IEnumerable<IEdge> IGraph.Edges => Edges;
        IEnumerable<IEdge<TNode>> IGraph<TNode>.Edges => Edges;
        public IEnumerable<IEdge<TNode, TEdge>> Edges => _edges.Values;

        INode<TNode> IGraph<TNode>.this[TNode data] => _nodes[_nodeMap[data]];
        public INode<TNode, TEdge> this[TNode data] => _nodes[_nodeMap[data]];

        INode IMutableGraph.AddNode() => AddNode(null);
        INode<TNode> IMutableGraph<TNode>.AddNode(TNode data) => AddNode(data);
        public INode<TNode, TEdge> AddNode(TNode data)
        {
            var node = new Node(data, ++_nextNodeId, GetProtoEdge, this);
            _nodes.Add(node.Index, node);
            _nodeMap.Add(data, node.Index);
            return node;
        }

        public void RemoveNode(INode node)
        {
            if(!ReferenceEquals(_nodes[node.Index], node)) throw new ArgumentException();

            foreach(var e in node.Edges.ToList()) e.Remove();
            _nodeMap.Remove(((Node)node).Data);
            _nodes.Remove(node.Index);
        }

        private ProtoEdge GetProtoEdge(uint n0, uint n1)
        {
            return new ProtoEdge(n0, n1, _edges, id => _nodes[id], IsDirected);
        }

        private static ulong GetEdgeKey(uint n0, uint n1, bool directed)
        {
            return !directed && n0 > n1
                ? n1 | ((ulong)n0 << 32)
                : n0 | ((ulong)n1 << 32);
        }
    }
}
