namespace Eulg.Common.Graph
{
    public interface IProtoEdge
    {
        /// <summary>
        /// Gibt an ob die Kante bereits manifestiert ist.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Gibt an ob die Kante gerichtet ist (wird durch den Graph bestimmt, zu dem die Knoten gehören).
        /// </summary>
        bool IsDirected { get; }

        /// <summary>
        /// Liefert das Objekt, das die Kante beschreibt. Die Kante muss dazu bereits existieren.
        /// </summary>
        IEdge Get();

        /// <summary>
        /// Fügt die Kante zum Graph hinzu.
        /// </summary>
        void Add();

        /// <summary>
        /// Entfernt die Kante aus dem Graph.
        /// </summary>
        bool Remove();
    }

    public interface IProtoEdge<TNode> : IProtoEdge where TNode : class
    {
        /// <summary>
        /// Liefert das Objekt, das die Kante beschreibt. Die Kante muss dazu bereits existieren.
        /// </summary>
        new IEdge<TNode> Get();
    }

    public interface IProtoEdge<TNode, TEdge> : IProtoEdge<TNode> where TNode : class where TEdge : class
    {
        /// <summary>
        /// Liefert das Objekt, das die Kante beschreibt. Die Kante muss dazu bereits existieren.
        /// </summary>
        new IEdge<TNode, TEdge> Get();

        /// <summary>
        /// Fügt die Kante mit dem gegebenen Datenobjekt zum Graph hinzu.
        /// </summary>
        void Add(TEdge data);
    }
}
