namespace Eulg.Common.Graph
{
    public interface IEdge
    {
        /// <summary>
        /// Der Knoten am einen Ende der Kante. Bei gerichteten Graphen der Knoten, von dem die Kante ausgeht.
        /// </summary>
        INode Node0 { get; }

        /// <summary>
        /// Der Knoten am anderen Ende der Kante. Ber gerichteten Graphen der Knoten, zu dem die Kante hingeht.
        /// </summary>
        INode Node1 { get; }

        /// <summary>
        /// Gibt an ob die Kante gerichtet oder ungerichtet ist.
        /// </summary>
        bool IsDirected { get; }

        /// <summary>
        /// Kante aus dem Graph entfernen.
        /// </summary>
        void Remove();
    }

    public interface IEdge<TNode> : IEdge where TNode : class
    {
        /// <summary>
        /// Der Knoten am einen Ende der Kante. Bei gerichteten Graphen der Knoten, von dem die Kante ausgeht.
        /// </summary>
        new INode<TNode> Node0 { get; }

        /// <summary>
        /// Der Knoten am anderen Ende der Kante. Ber gerichteten Graphen der Knoten, zu dem die Kante hingeht.
        /// </summary>
        new INode<TNode> Node1 { get; }
    }

    public interface IEdge<TNode, TEdge> : IEdge<TNode> where TNode : class where TEdge : class
    {
        /// <summary>
        /// Mit der Kante verbundenes Datenobjekt.
        /// </summary>
        TEdge Data { get; }

        /// <summary>
        /// Der Knoten am einen Ende der Kante. Bei gerichteten Graphen der Knoten, von dem die Kante ausgeht.
        /// </summary>
        new INode<TNode, TEdge> Node0 { get; }

        /// <summary>
        /// Der Knoten am anderen Ende der Kante. Ber gerichteten Graphen der Knoten, zu dem die Kante hingeht.
        /// </summary>
        new INode<TNode, TEdge> Node1 { get; }
    }
}
