using System.Collections.Generic;

namespace Eulg.Common.Graph
{
    public interface IGraph
    {
        /// <summary>
        /// Gibt an ob dieser Graph gerichtete oder ungerichtete Kanten enthält.
        /// </summary>
        bool IsDirected { get; }

        /// <summary>
        /// Liste aller Knoten.
        /// </summary>
        IEnumerable<INode> Nodes { get; }

        /// <summary>
        /// Liste aller Kanten.
        /// </summary>
        IEnumerable<IEdge> Edges { get; }
    }

    public interface IGraph<TNode> : IGraph where TNode : class
    {
        /// <summary>
        /// Liste aller Knoten.
        /// </summary>
        new IEnumerable<INode<TNode>> Nodes { get; }

        /// <summary>
        /// Liste aller Kanten.
        /// </summary>
        new IEnumerable<IEdge<TNode>> Edges { get; }

        /// <summary>
        /// Sucht einen Knoten über das zugeordnete Datenobjekt.
        /// </summary>
        INode<TNode> this[TNode data] { get; }
    }

    public interface IGraph<TNode, TEdge> : IGraph<TNode> where TNode : class where TEdge : class
    {
        /// <summary>
        /// Liste aller Knoten.
        /// </summary>
        new IEnumerable<INode<TNode, TEdge>> Nodes { get; }

        /// <summary>
        /// Liste aller Kanten.
        /// </summary>
        new IEnumerable<IEdge<TNode, TEdge>> Edges { get; }

        /// <summary>
        /// Sucht einen Knoten über das zugeordnete Datenobjekt.
        /// </summary>
        new INode<TNode, TEdge> this[TNode data] { get; }
    }

    public interface IMutableGraph : IGraph
    {
        /// <summary>
        /// Fügt einen neuen Knoten zum Graph hinzu.
        /// </summary>
        INode AddNode();

        /// <summary>
        /// Entfernt einen Knoten und alle damit verbundenen Kanten.
        /// </summary>
        void RemoveNode(INode node);
    }

    public interface IMutableGraph<TNode> : IMutableGraph, IGraph<TNode> where TNode : class
    {
        /// <summary>
        /// Fügt einen neuen Knoten zum Graph hinzu.
        /// </summary>
        INode<TNode> AddNode(TNode data);
    }

    public interface IMutableGraph<TNode, TEdge> : IMutableGraph<TNode>, IGraph<TNode, TEdge> where TNode : class where TEdge : class
    {
        /// <summary>
        /// Fügt einen neuen Knoten zum Graph hinzu.
        /// </summary>
        new INode<TNode, TEdge> AddNode(TNode data);
    }
}
