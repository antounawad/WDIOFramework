using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Eulg.Common.Graph
{
    public enum ETopSortOrder
    {
        SourceToSink,
        SinkToSource
    }

    public static class GraphExtensions
    {
        public static IGraph<TNode> CreateGraph<TNode>(this IEnumerable<TNode> nodes, bool directed) where TNode : class
        {
            return new SimpleGraph<TNode>(nodes, directed);
        }

        public static IGraph<TNode> CreateUndirectedGraph<TNode>(this IEnumerable<TNode> nodes) where TNode : class
        {
            return new SimpleGraph<TNode>(nodes, false);
        }

        public static IGraph<TNode> CreateDirectedGraph<TNode>(this IEnumerable<TNode> nodes) where TNode : class
        {
            return new SimpleGraph<TNode>(nodes, true);
        }

        public static IGraph<TNode, TEdge> CreateGraph<TNode, TEdge>(this IEnumerable<TNode> nodes, bool directed) where TNode : class where TEdge : class
        {
            return new Graph<TNode, TEdge>(nodes, directed);
        }

        public static IGraph<TNode, TEdge> CreateUndirectedGraph<TNode, TEdge>(this IEnumerable<TNode> nodes) where TNode : class where TEdge : class
        {
            return new Graph<TNode, TEdge>(nodes, false);
        }

        public static IGraph<TNode, TEdge> CreateDirectedGraph<TNode, TEdge>(this IEnumerable<TNode> nodes) where TNode : class where TEdge : class
        {
            return new Graph<TNode, TEdge>(nodes, true);
        }

        public static IGraph<TNode> CreateDependencyGraph<TNode>(this IEnumerable<TNode> nodes, Func<TNode, TNode, bool> isDependencyFunc) where TNode : class
        {
            var graph = CreateDirectedGraph(nodes);

            foreach (var n0 in graph.Nodes)
            {
                foreach (var n1 in graph.Nodes)
                {
                    if (n0 == n1) continue;

                    if (isDependencyFunc(n0.Data, n1.Data))
                    {
                        n0[n1].Add();
                    }
                }
            }

            return graph;
        }

        public static IGraph<TNode> EvaluateEdgeData<TNode, TEdge>(this IGraph<TNode, TEdge> graph, Func<TEdge, bool> evaluator) where TNode : class where TEdge : class
        {
            var nodes = graph.Nodes.Select(_ => _.Data);
            var result = nodes.CreateGraph(graph.IsDirected);

            foreach (var edge in graph.Edges)
            {
                if (evaluator(edge.Data))
                {
                    var proto = result[edge.Node0.Data][edge.Node1.Data];
                    if (!proto.Exists)
                    {
                        proto.Add();
                    }
                }
            }

            return result;
        }

        public static IGraph<TNode, TEdge> GraphFromEdges<TNode, TEdge>(this IEnumerable<IEdge<TNode, TEdge>> edges, bool directed) where TNode : class where TEdge : class
        {
            var edgeList = edges.ToList();
            var graph = edgeList.SelectMany(e => new[] { e.Node0.Data, e.Node1.Data }).Distinct().CreateGraph<TNode, TEdge>(directed);

            foreach (var edge in edgeList)
            {
                graph[edge.Node0.Data][edge.Node1.Data].Add(edge.Data);
            }

            return graph;
        }

        public static IGraph<TNode, TEdge> CreateConnectedSubGraph<TNode, TEdge>(this IGraph<TNode, TEdge> graph, IEnumerable<INode<TNode, TEdge>> seed, Func<IEdge<TNode, TEdge>, bool> edgeCondition, Func<IEdge<TNode, TEdge>, TEdge> edgeSelector, int maxDistance = int.MaxValue) where TNode : class where TEdge : class
        {
            return InternalCreateConnectedSubGraph(graph.IsDirected, seed, EDirection.Either, edgeCondition, edgeSelector, maxDistance);
        }

        public static IGraph<TNode, TEdge> CreateSuccessorSubGraph<TNode, TEdge>(this IGraph<TNode, TEdge> graph, IEnumerable<INode<TNode, TEdge>> seed, Func<IEdge<TNode, TEdge>, bool> edgeCondition, Func<IEdge<TNode, TEdge>, TEdge> edgeSelector, int maxDistance = int.MaxValue) where TNode : class where TEdge : class
        {
            if(!graph.IsDirected) throw new ArgumentException("This method can only be used on directed graphs");
            return InternalCreateConnectedSubGraph(true, seed, EDirection.Outgoing, edgeCondition, edgeSelector, maxDistance);
        }

        public static IGraph<TNode, TEdge> CreatePredecessorSubGraph<TNode, TEdge>(this IGraph<TNode, TEdge> graph, IEnumerable<INode<TNode, TEdge>> seed, Func<IEdge<TNode, TEdge>, bool> edgeCondition, Func<IEdge<TNode, TEdge>, TEdge> edgeSelector, int maxDistance = int.MaxValue) where TNode : class where TEdge : class
        {
            if(!graph.IsDirected) throw new ArgumentException("This method can only be used on directed graphs");
            return InternalCreateConnectedSubGraph(true, seed, EDirection.Incoming, edgeCondition, edgeSelector, maxDistance);
        }

        public static IEdge<TNode, TEdge> GetOrAdd<TNode, TEdge>(this IProtoEdge<TNode, TEdge> edge) where TNode : class where TEdge : class, new()
        {
            if (!edge.Exists)
            {
                edge.Add(new TEdge());
            }

            return edge.Get();
        }

        public static IEdge<TNode, TEdge> GetOrAdd<TNode, TEdge>(this IProtoEdge<TNode, TEdge> edge, Func<TEdge> factory) where TNode : class where TEdge : class
        {
            if(!edge.Exists)
            {
                edge.Add(factory());
            }

            return edge.Get();
        }

        public static IGraph<TNode, TEdge> TrimNodes<TNode, TEdge>(this IGraph<TNode, TEdge> graph, Func<TNode, bool> removeIf) where TNode : class where TEdge : class
        {
            var nodes = new HashSet<INode<TNode, TEdge>>(graph.Nodes.Where(n => !removeIf(n.Data)));
            var result = nodes.Select(n => n.Data).CreateGraph<TNode, TEdge>(graph.IsDirected);

            foreach (var edge in graph.Edges)
            {
                if (nodes.Contains(edge.Node0) && nodes.Contains(edge.Node1))
                {
                    result[edge.Node0.Data][edge.Node1.Data].Add(edge.Data);
                }
            }

            return result;
        }

        public static IGraph<TNodeOut, TEdgeOut> ConvertData<TNodeIn, TNodeOut, TEdgeIn, TEdgeOut>(this IGraph<TNodeIn, TEdgeIn> graph, Func<TNodeIn, TNodeOut> nodeSelector, Func<TEdgeIn, TEdgeOut> edgeSelector) where TNodeIn : class where TNodeOut : class where TEdgeIn : class where TEdgeOut : class
        {
            var nodeMap = graph.Nodes.ToDictionary(n => n, n => nodeSelector(n.Data));
            var result = graph.Nodes.Select(n => nodeMap[n]).CreateGraph<TNodeOut, TEdgeOut>(graph.IsDirected);

            foreach (var edge in graph.Edges)
            {
                result[nodeMap[edge.Node0]][nodeMap[edge.Node1]].Add(edgeSelector(edge.Data));
            }

            return result;
        }

        #region Graph To XGML

        public static void GraphToXml<TNode, TEdge>(this IGraph<TNode, TEdge> graph, TextWriter output,
            Func<TNode, string> nodeFormatter, Func<TEdge, string> edgeFormatter,
            Func<TNode, string> nodeColor = null, Func<TEdge, (string, int)> edgeColor = null) where TNode : class where TEdge : class
        {
            var xml = XgmlNode("section", ("name", "gxml"));
            xml.Add(XgmlAttribute("2.14", ("key", "Version"), ("type", "String")));

            var xmlGraph = XgmlNode("section", ("name", "graph"));

            xmlGraph.Add(
                XgmlAttribute("1", ("key", "hierarchic"), ("type", "int")),
                XgmlAttribute("", ("key", "label"), ("type", "String")),
                XgmlAttribute("1", ("key", "directed"), ("type", "int"))
            );

            var idMap = graph.Nodes.Select((n, i) => new KeyValuePair<INode<TNode, TEdge>, int>(n, i)).ToDictionary(x => x.Key, x => x.Value);
            foreach (var node in graph.Nodes)
            {
                var label = nodeFormatter(node.Data);
                var xmlNode = XgmlNode("section", ("name", "node"));

                var nodeGraphics = XgmlNode("section", ("name", "graphics"));

                nodeGraphics.Add(
                    XgmlAttribute("rectangle3d", ("key", "type"), ("type", "String")),
                    XgmlAttribute(nodeColor == null ? "#A0A0A0" : nodeColor(node.Data), ("key", "fill"), ("type", "String"))
                );

                xmlNode.Add(
                    XgmlAttribute(idMap[node], ("key", "id"), ("type", "int")),
                    XgmlAttribute(label, ("key", "label"), ("type", "String")),
                    nodeGraphics
                );

                var nodeLabelGraphics = XgmlNode("section", ("name", "LabelGraphics"));

                nodeLabelGraphics.Add(
                    XgmlAttribute(label, ("key", "text"), ("type", "String")),
                    XgmlAttribute(12, ("key", "fontSize"), ("type", "int")),
                    XgmlAttribute("Segoe UI Light", ("key", "fontName"), ("type", "String")),
                    XgmlAttribute(null, ("key", "Model"))
                );

                xmlNode.Add(nodeLabelGraphics);
                xmlGraph.Add(xmlNode);
            }
            foreach (var edge in graph.Edges)
            {
                var xmlEdge = XgmlNode("section", ("name", "edge"));

                var edgeGraphics = XgmlNode("section", ("name", "graphics"));
                var labelGraphics = XgmlNode("section", ("name", "LabelGraphics"));

                edgeGraphics.Add(
                    XgmlAttribute(edgeColor?.Invoke(edge.Data).Item2.ToString() ?? "1", ("key", "width"), ("type", "int")),
                    XgmlAttribute(edgeColor == null ? "#000000" : edgeColor(edge.Data).Item1, ("key", "fill"), ("type", "String")),
                    XgmlAttribute("standard", ("key", "targetArrow"), ("type", "String"))
                );

                labelGraphics.Add(
                    XgmlAttribute(10, ("key", "fontSize"), ("type", "int")),
                    XgmlAttribute("Segoe UI Light", ("key", "fontName"), ("type", "String"))
                );

                xmlEdge.Add(
                    XgmlAttribute(idMap[edge.Node0], ("key", "source"), ("type", "int")),
                    XgmlAttribute(idMap[edge.Node1], ("key", "target"), ("type", "int")),
                    XgmlAttribute(edgeFormatter(edge.Data), ("key", "label"), ("type", "String")),
                    edgeGraphics,
                    labelGraphics
                );

                xmlGraph.Add(xmlEdge);
            }

            xml.Add(xmlGraph);
            xml.Save(output);
        }

        private static XElement XgmlNode(string name, params (string key, string value)[] attributes)
        {
            var element = new XElement(name);
            foreach (var attr in attributes)
            {
                element.SetAttributeValue(attr.key, attr.value);
            }
            return element;
        }

        private static XElement XgmlAttribute(object value, params (string key, string value)[] attributes)
        {
            var element = new XElement("attribute", value);
            foreach (var attr in attributes)
            {
                element.SetAttributeValue(attr.key, attr.value);
            }
            return element;
        }

        #endregion

        public static bool GeneratesTree(this INode startNode)
        {
            var path = new HashSet<INode>();
            return GeneratesTreeInternal(startNode, path);
        }

        private static bool GeneratesTreeInternal(INode node, ISet<INode> path)
        {
            return path.Add(node) && node.Outgoing.All(e => e.IsDirected && GeneratesTreeInternal(e.Node1, path));
        }


        #region Topological Sort

        public static IEnumerable<TNode> TopologicalSort<TNode>(this IGraph<TNode> graph, ETopSortOrder order) where TNode : class
        {
            if (!graph.IsDirected) throw new ArgumentException("Topological sort can only be performed on directed graphs", nameof(graph));

            IEnumerable<TNode> topologicalOrder;

            var remainingEdges = new HashSet<IEdge<TNode>>(graph.Edges);
            switch (order)
            {
                case ETopSortOrder.SourceToSink:
                    topologicalOrder = TopologicalSortSourceToSink(graph, remainingEdges).ToList();
                    break;
                case ETopSortOrder.SinkToSource:
                    topologicalOrder = TopologicalSortSinkToSource(graph, remainingEdges).ToList();
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            if (remainingEdges.Any())
            {
                throw new InvalidOperationException("Graph contains cycles");
            }

            return topologicalOrder;
        }

        private static IEnumerable<TNode> TopologicalSortSourceToSink<TNode>(IGraph<TNode> graph, ICollection<IEdge<TNode>> remainingEdges) where TNode : class
        {
            var sourceNodes = new HashSet<INode<TNode>>(graph.Nodes.Where(n => !n.Incoming.Any()));
            while (sourceNodes.Count > 0)
            {
                var node = sourceNodes.First();
                sourceNodes.Remove(node);

                yield return node.Data;

                foreach (var edge in node.Outgoing.Where(remainingEdges.Contains))
                {
                    remainingEdges.Remove(edge);

                    if (!edge.Node1.Incoming.Any(remainingEdges.Contains))
                    {
                        sourceNodes.Add(edge.Node1);
                    }
                }
            }
        }

        private static IEnumerable<TNode> TopologicalSortSinkToSource<TNode>(IGraph<TNode> graph, ICollection<IEdge<TNode>> remainingEdges) where TNode : class
        {
            var sourceNodes = new HashSet<INode<TNode>>(graph.Nodes.Where(n => !n.Outgoing.Any()));
            while (sourceNodes.Count > 0)
            {
                var node = sourceNodes.First();
                sourceNodes.Remove(node);

                yield return node.Data;

                foreach (var edge in node.Incoming.Where(remainingEdges.Contains))
                {
                    remainingEdges.Remove(edge);

                    if (!edge.Node0.Outgoing.Any(remainingEdges.Contains))
                    {
                        sourceNodes.Add(edge.Node0);
                    }
                }
            }
        }

        #endregion

        #region Connected Subgraph

        private enum EDirection
        {
            Incoming,
            Outgoing,
            Either
        }

        private static IGraph<TNode, TEdge> InternalCreateConnectedSubGraph<TNode, TEdge>(bool directed, IEnumerable<INode<TNode, TEdge>> seed, EDirection direction, Func<IEdge<TNode, TEdge>, bool> edgeCondition, Func<IEdge<TNode, TEdge>, TEdge> edgeSelector, int maxDistance) where TNode : class where TEdge : class
        {
            var nodes = new HashSet<INode<TNode, TEdge>>();
            var edges = new HashSet<IEdge<TNode, TEdge>>();
            var queue = new Queue<INode<TNode, TEdge>>();
            var distanceMap = new Dictionary<INode<TNode, TEdge>, int>();

            void TryEnqueue(INode<TNode, TEdge> head, IEdge<TNode, TEdge> edge, INode<TNode, TEdge> node)
            {
                if(!distanceMap.TryGetValue(node, out var distance) || distanceMap[head] + 1 < distance)
                {
                    distance = distanceMap[head] + 1;
                }
                if(distance <= maxDistance && edges.Add(edge))
                {
                    distanceMap[node] = distance;
                    queue.Enqueue(node);
                }
            }

            foreach(var s in seed)
            {
                queue.Enqueue(s);
                distanceMap.Add(s, 0);
            }

            while(queue.Count > 0)
            {
                var head = queue.Dequeue();
                if(!nodes.Add(head)) continue;

                IEnumerable<IEdge<TNode, TEdge>> candidates;
                switch (direction)
                {
                    case EDirection.Incoming:
                        candidates = head.Incoming;
                        break;
                    case EDirection.Outgoing:
                        candidates = head.Outgoing;
                        break;
                    case EDirection.Either:
                        candidates = head.Edges;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }

                foreach(var edge in candidates.Where(edgeCondition))
                {
                    switch (direction)
                    {
                        case EDirection.Incoming:
                            TryEnqueue(head, edge, edge.Node0);
                            break;
                        case EDirection.Outgoing:
                            TryEnqueue(head, edge, edge.Node1);
                            break;
                        case EDirection.Either:
                            TryEnqueue(head, edge, head == edge.Node0 ? edge.Node1 : edge.Node0);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                }
            }

            var result = nodes.Select(n => n.Data).CreateGraph<TNode, TEdge>(directed);
            foreach(var edge in edges)
            {
                result[edge.Node0.Data][edge.Node1.Data].Add(edgeSelector(edge));
            }

            return result;
        }

        #endregion

        #region Find Path

        public static IEdge[] FindPath(this IGraph graph, INode origin, INode target)
        {
            if (!graph.IsDirected) throw new NotSupportedException("This method is not supported on undirected paths");

            return FindDirectedPath(origin, target, graph.Edges)?.ToArray();
        }

        public static IEdge<TNode>[] FindPath<TNode>(this IGraph<TNode> graph, INode<TNode> origin, INode<TNode> target, Func<IEdge<TNode>, bool> predicate) where TNode : class
        {
            if (!graph.IsDirected) throw new NotSupportedException("This method is not supported on undirected paths");

            return FindDirectedPath(origin, target, graph.Edges.Where(predicate))?.Cast<IEdge<TNode>>().ToArray();
        }

        public static IEdge<TNode, TEdge>[] FindPath<TNode, TEdge>(this IGraph<TNode, TEdge> graph, INode<TNode, TEdge> origin, INode<TNode, TEdge> target, Func<IEdge<TNode, TEdge>, bool> predicate) where TNode : class where TEdge : class
        {
            if (!graph.IsDirected) throw new NotSupportedException("This method is not supported on undirected paths");

            return FindDirectedPath(origin, target, graph.Edges.Where(predicate))?.Cast<IEdge<TNode, TEdge>>().ToArray();
        }

        private static IEnumerable<IEdge> FindDirectedPath(INode origin, INode target, IEnumerable<IEdge> availableEdges)
        {
            if (origin == target) return Enumerable.Empty<IEdge>();

            var edges = new HashSet<IEdge>(availableEdges);

            bool steady;
            do
            {
                steady = true;

                var haveOriginEdge = false;
                var haveTargetEdge = false;
                var deleted = new HashSet<IEdge>();

                foreach (var edge in edges)
                {
                    if (edge.Node0 == origin)
                    {
                        haveOriginEdge = true;

                        if(edge.Node1 == target)
                        {
                            return new[] { edge };
                        }

                        continue;
                    }

                    if (edge.Node1 == target)
                    {
                        haveTargetEdge = true;
                        continue;
                    }

                    if (edge.Node0.Incoming.Intersect(edges).Any() && edge.Node1.Outgoing.Intersect(edges).Any())
                    {
                        continue;
                    }

                    steady = false;
                    deleted.Add(edge);
                }

                if (!haveOriginEdge || !haveTargetEdge)
                {
                    return null;
                }

                edges.RemoveWhere(e => deleted.Contains(e));
            }
            while (!steady);

            var stack = new Stack<KeyValuePair<int, IEdge>>();
            var result = new List<IEdge>();

            foreach (var edge in origin.Outgoing.Intersect(edges))
            {
                stack.Push(new KeyValuePair<int, IEdge>(0, edge));
            }

            while (stack.Count != 0)
            {
                var step = stack.Pop();

                result.RemoveRange(step.Key, result.Count - step.Key);
                if (result.Contains(step.Value))
                {
                    // Cycle
                    continue;
                }
                result.Add(step.Value);

                if (step.Value.Node1 == target)
                {
                    return result;
                }

                foreach (var edge in step.Value.Node1.Outgoing.Intersect(edges))
                {
                    stack.Push(new KeyValuePair<int, IEdge>(step.Key + 1, edge));
                }
            }

            return null;
        }

        #endregion
    }
}
