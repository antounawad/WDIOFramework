using System.Collections.Generic;

namespace Eulg.Common.Graph
{
    public interface INode
    {
        /// <summary>
        /// Eindeutiger Index des Knoten innerhalb des Graphs.
        /// </summary>
        uint Index { get; }

        /// <summary>
        /// Alle Knoten, die direkt mit diesem Knoten durch eine Kante verbunden sind.
        /// </summary>
        IEnumerable<INode> Adjacent { get; }

        /// <summary>
        /// Alle Kanten, die mindestens ein Ende in diesem Knoten haben.
        /// </summary>
        IEnumerable<IEdge> Edges { get; }

        /// <summary>
        /// Alle gerichteten Kanten, die in diesem Knoten enden, sowie alle ungerichteten Kanten, die mit diesem Knoten verbunden sind.
        /// </summary>
        IEnumerable<IEdge> Incoming { get; }

        /// <summary>
        /// Alle gerichteten Kanten, die in diesem Knoten beginnen, sowie alle ungerichteten Kanten, die mit diesem Knoten verbunden sind.
        /// </summary>
        IEnumerable<IEdge> Outgoing { get; }

        /// <summary>
        /// Alle Knoten, die ausgehende bzw. ungerichtete Kanten zu diesem Knoten haben.
        /// </summary>
        IEnumerable<INode> Predecessors { get; }

        /// <summary>
        /// Alle Knoten, zu denen dieser Knoten ausgehende bzw. ungerichtete Kanten hat.
        /// </summary>
        IEnumerable<INode> Successors { get; }

        /// <summary>
        /// Liefert ein Proxy-Objekt, mit dem die Kante zwischen diesem und dem gegebenen Knoten manipuliert werden kann.
        /// </summary>
        IProtoEdge this[INode adjacent] { get; }
    }

    public interface INode<TNode> : INode where TNode : class
    {
        /// <summary>
        /// Mit dem Knoten verknüpftes Datenobjekt.
        /// </summary>
        TNode Data { get; }

        /// <summary>
        /// Alle Knoten, die direkt mit diesem Knoten durch eine Kante verbunden sind.
        /// </summary>
        new IEnumerable<INode<TNode>> Adjacent { get; }

        /// <summary>
        /// Alle Kanten, die mindestens ein Ende in diesem Knoten haben.
        /// </summary>
        new IEnumerable<IEdge<TNode>> Edges { get; }

        /// <summary>
        /// Alle gerichteten Kanten, die in diesem Knoten enden, sowie alle ungerichteten Kanten, die mit diesem Knoten verbunden sind.
        /// </summary>
        new IEnumerable<IEdge<TNode>> Incoming { get; }

        /// <summary>
        /// Alle gerichteten Kanten, die in diesem Knoten beginnen, sowie alle ungerichteten Kanten, die mit diesem Knoten verbunden sind.
        /// </summary>
        new IEnumerable<IEdge<TNode>> Outgoing { get; }

        /// <summary>
        /// Alle Knoten, die ausgehende bzw. ungerichtete Kanten zu diesem Knoten haben.
        /// </summary>
        new IEnumerable<INode<TNode>> Predecessors { get; }

        /// <summary>
        /// Alle Knoten, zu denen dieser Knoten ausgehende bzw. ungerichtete Kanten hat.
        /// </summary>
        new IEnumerable<INode<TNode>> Successors { get; }

        /// <summary>
        /// Liefert ein Proxy-Objekt, mit dem die Kante zwischen diesem und einem Knoten, der durch das Datenobjekt identifiziert wird, manipuliert werden kann.
        /// </summary>
        IProtoEdge<TNode> this[TNode adjacent] { get; }

        /// <summary>
        /// Liefert ein Proxy-Objekt, mit dem die Kante zwischen diesem und dem gegebenen Knoten manipuliert werden kann.
        /// </summary>
        new IProtoEdge<TNode> this[INode adjacent] { get; }
    }

    public interface INode<TNode, TEdge> : INode<TNode> where TNode : class where TEdge : class
    {
        /// <summary>
        /// Alle Knoten, die direkt mit diesem Knoten durch eine Kante verbunden sind.
        /// </summary>
        new IEnumerable<INode<TNode, TEdge>> Adjacent { get; }

        /// <summary>
        /// Alle Kanten, die mindestens ein Ende in diesem Knoten haben.
        /// </summary>
        new IEnumerable<IEdge<TNode, TEdge>> Edges { get; }

        /// <summary>
        /// Alle gerichteten Kanten, die in diesem Knoten enden, sowie alle ungerichteten Kanten, die mit diesem Knoten verbunden sind.
        /// </summary>
        new IEnumerable<IEdge<TNode, TEdge>> Incoming { get; }

        /// <summary>
        /// Alle gerichteten Kanten, die in diesem Knoten beginnen, sowie alle ungerichteten Kanten, die mit diesem Knoten verbunden sind.
        /// </summary>
        new IEnumerable<IEdge<TNode, TEdge>> Outgoing { get; }

        /// <summary>
        /// Alle Knoten, die ausgehende bzw. ungerichtete Kanten zu diesem Knoten haben.
        /// </summary>
        new IEnumerable<INode<TNode, TEdge>> Predecessors { get; }

        /// <summary>
        /// Alle Knoten, zu denen dieser Knoten ausgehende bzw. ungerichtete Kanten hat.
        /// </summary>
        new IEnumerable<INode<TNode, TEdge>> Successors { get; }

        /// <summary>
        /// Liefert ein Proxy-Objekt, mit dem die Kante zwischen diesem und einem Knoten, der durch das Datenobjekt identifiziert wird, manipuliert werden kann.
        /// </summary>
        new IProtoEdge<TNode, TEdge> this[TNode adjacent] { get; }

        /// <summary>
        /// Liefert ein Proxy-Objekt, mit dem die Kante zwischen diesem und dem gegebenen Knoten manipuliert werden kann.
        /// </summary>
        new IProtoEdge<TNode, TEdge> this[INode adjacent] { get; }
    }
}
