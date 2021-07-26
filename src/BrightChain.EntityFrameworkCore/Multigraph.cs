// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;

#nullable enable

namespace BrightChain.EntityFrameworkCore.Utilities
{
    internal class Multigraph<TVertex, TEdge> : Graph<TVertex>
        where TVertex : notnull
    {
        private readonly HashSet<TVertex> _vertices = new();
        private readonly Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>> _successorMap = new();
        private readonly Dictionary<TVertex, HashSet<TVertex>> _predecessorMap = new();

        public IEnumerable<TEdge> Edges
            => this._successorMap.Values.SelectMany(s => s.Values).SelectMany(e => e).Distinct();

        public IEnumerable<TEdge> GetEdges(TVertex from, TVertex to)
        {
            if (this._successorMap.TryGetValue(from, out var successorSet))
            {
                if (successorSet.TryGetValue(to, out var edgeList))
                {
                    return edgeList;
                }
            }

            return Enumerable.Empty<TEdge>();
        }

        public void AddVertex(TVertex vertex)
        {
            this._vertices.Add(vertex);
        }

        public void AddVertices(IEnumerable<TVertex> vertices)
        {
            this._vertices.UnionWith(vertices);
        }

        public void AddEdge(TVertex from, TVertex to, TEdge edge)
        {
#if DEBUG
            if (!this._vertices.Contains(from))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(from));
            }

            if (!this._vertices.Contains(to))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(to));
            }
#endif

            if (!this._successorMap.TryGetValue(from, out var successorEdges))
            {
                successorEdges = new Dictionary<TVertex, List<TEdge>>();
                this._successorMap.Add(from, successorEdges);
            }

            if (!successorEdges.TryGetValue(to, out var edgeList))
            {
                edgeList = new List<TEdge>();
                successorEdges.Add(to, edgeList);
            }

            edgeList.Add(edge);

            if (!this._predecessorMap.TryGetValue(to, out var predecessors))
            {
                predecessors = new HashSet<TVertex>();
                this._predecessorMap.Add(to, predecessors);
            }

            predecessors.Add(from);
        }

        public void AddEdges(TVertex from, TVertex to, IEnumerable<TEdge> edges)
        {
#if DEBUG
            if (!this._vertices.Contains(from))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(from));
            }

            if (!this._vertices.Contains(to))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(to));
            }
#endif

            if (!this._successorMap.TryGetValue(from, out var successorEdges))
            {
                successorEdges = new Dictionary<TVertex, List<TEdge>>();
                this._successorMap.Add(from, successorEdges);
            }

            if (!successorEdges.TryGetValue(to, out var edgeList))
            {
                edgeList = new List<TEdge>();
                successorEdges.Add(to, edgeList);
            }

            edgeList.AddRange(edges);

            if (!this._predecessorMap.TryGetValue(to, out var predecessors))
            {
                predecessors = new HashSet<TVertex>();
                this._predecessorMap.Add(to, predecessors);
            }

            predecessors.Add(from);
        }

        public override void Clear()
        {
            this._vertices.Clear();
            this._successorMap.Clear();
            this._predecessorMap.Clear();
        }

        public IReadOnlyList<TVertex> TopologicalSort()
        {
            return this.TopologicalSort(null, null);
        }

        public IReadOnlyList<TVertex> TopologicalSort(
            Func<TVertex, TVertex, IEnumerable<TEdge>, bool> tryBreakEdge)
        {
            return this.TopologicalSort(tryBreakEdge, null);
        }

        public IReadOnlyList<TVertex> TopologicalSort(
            Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            return this.TopologicalSort(null, formatCycle);
        }

        public IReadOnlyList<TVertex> TopologicalSort(
            Func<TVertex, TVertex, IEnumerable<TEdge>, bool>? tryBreakEdge,
            Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle,
            Func<string, string>? formatException = null)
        {
            var sortedQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in this._vertices)
            {
                foreach (var outgoingNeighbor in this.GetOutgoingNeighbors(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbor))
                    {
                        predecessorCounts[outgoingNeighbor]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbor] = 1;
                    }
                }
            }

            foreach (var vertex in this._vertices)
            {
                if (!predecessorCounts.ContainsKey(vertex))
                {
                    sortedQueue.Add(vertex);
                }
            }

            var index = 0;
            while (sortedQueue.Count < this._vertices.Count)
            {
                while (index < sortedQueue.Count)
                {
                    var currentRoot = sortedQueue[index];

                    foreach (var successor in this.GetOutgoingNeighbors(currentRoot).Where(neighbor => predecessorCounts.ContainsKey(neighbor)))
                    {
                        // Decrement counts for edges from sorted vertices and append any vertices that no longer have predecessors
                        predecessorCounts[successor]--;
                        if (predecessorCounts[successor] == 0)
                        {
                            sortedQueue.Add(successor);
                            predecessorCounts.Remove(successor);
                        }
                    }

                    index++;
                }

                // Cycle breaking
                if (sortedQueue.Count < this._vertices.Count)
                {
                    var broken = false;

                    var candidateVertices = predecessorCounts.Keys.ToList();
                    var candidateIndex = 0;

                    // Iterate over the unsorted vertices
                    while ((candidateIndex < candidateVertices.Count)
                        && !broken
                        && tryBreakEdge != null)
                    {
                        var candidateVertex = candidateVertices[candidateIndex];

                        // Find vertices in the unsorted portion of the graph that have edges to the candidate
                        var incomingNeighbors = this.GetIncomingNeighbors(candidateVertex)
                            .Where(neighbor => predecessorCounts.ContainsKey(neighbor)).ToList();

                        foreach (var incomingNeighbor in incomingNeighbors)
                        {
                            // Check to see if the edge can be broken
                            if (tryBreakEdge(incomingNeighbor, candidateVertex, this._successorMap[incomingNeighbor][candidateVertex]))
                            {
                                predecessorCounts[candidateVertex]--;
                                if (predecessorCounts[candidateVertex] == 0)
                                {
                                    sortedQueue.Add(candidateVertex);
                                    predecessorCounts.Remove(candidateVertex);
                                    broken = true;
                                    break;
                                }
                            }
                        }

                        candidateIndex++;
                    }

                    if (!broken)
                    {
                        // Failed to break the cycle
                        var currentCycleVertex = this._vertices.First(v => predecessorCounts.ContainsKey(v));
                        var cycle = new List<TVertex> { currentCycleVertex };
                        var finished = false;
                        while (!finished)
                        {
                            // Find a cycle
                            foreach (var predecessor in this.GetIncomingNeighbors(currentCycleVertex)
                                .Where(neighbor => predecessorCounts.ContainsKey(neighbor)))
                            {
                                if (predecessorCounts[predecessor] != 0)
                                {
                                    predecessorCounts[currentCycleVertex] = -1;

                                    currentCycleVertex = predecessor;
                                    cycle.Add(currentCycleVertex);
                                    finished = predecessorCounts[predecessor] == -1;
                                    break;
                                }
                            }
                        }

                        cycle.Reverse();

                        this.ThrowCycle(cycle, formatCycle, formatException);
                    }
                }
            }

            return sortedQueue;
        }

        private void ThrowCycle(
            List<TVertex> cycle,
            Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle,
            Func<string, string>? formatException = null)
        {
            string cycleString;
            if (formatCycle == null)
            {
                cycleString = cycle.Select(e => this.ToString(e)!).Join(" ->" + Environment.NewLine);
            }
            else
            {
                var currentCycleVertex = cycle.First();
                var cycleData = new List<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>();

                foreach (var vertex in cycle.Skip(1))
                {
                    cycleData.Add(Tuple.Create(currentCycleVertex, vertex, this.GetEdges(currentCycleVertex, vertex)));
                    currentCycleVertex = vertex;
                }

                cycleString = formatCycle(cycleData);
            }

            var message = formatException == null ? CoreStrings.CircularDependency(cycleString) : formatException(cycleString);
            throw new InvalidOperationException(message);
        }

        protected virtual string? ToString(TVertex vertex)
        {
            return vertex.ToString();
        }

        public IReadOnlyList<List<TVertex>> BatchingTopologicalSort()
        {
            return this.BatchingTopologicalSort(null);
        }

        public IReadOnlyList<List<TVertex>> BatchingTopologicalSort(
            Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle)
        {
            var currentRootsQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in this._vertices)
            {
                foreach (var outgoingNeighbor in this.GetOutgoingNeighbors(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbor))
                    {
                        predecessorCounts[outgoingNeighbor]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbor] = 1;
                    }
                }
            }

            foreach (var vertex in this._vertices)
            {
                if (!predecessorCounts.ContainsKey(vertex))
                {
                    currentRootsQueue.Add(vertex);
                }
            }

            var result = new List<List<TVertex>>();
            var nextRootsQueue = new List<TVertex>();
            var currentRootIndex = 0;

            while (currentRootIndex < currentRootsQueue.Count)
            {
                var currentRoot = currentRootsQueue[currentRootIndex];
                currentRootIndex++;

                // Remove edges from current root and add any exposed vertices to the next batch
                foreach (var successor in this.GetOutgoingNeighbors(currentRoot))
                {
                    predecessorCounts[successor]--;
                    if (predecessorCounts[successor] == 0)
                    {
                        nextRootsQueue.Add(successor);
                    }
                }

                // Roll lists over for next batch
                if (currentRootIndex == currentRootsQueue.Count)
                {
                    result.Add(currentRootsQueue);

                    currentRootsQueue = nextRootsQueue;
                    currentRootIndex = 0;

                    if (currentRootsQueue.Count != 0)
                    {
                        nextRootsQueue = new List<TVertex>();
                    }
                }
            }

            if (result.Sum(b => b.Count) != this._vertices.Count)
            {
                var currentCycleVertex = this._vertices.First(
                    v => predecessorCounts.TryGetValue(v, out var predecessorNumber) && predecessorNumber != 0);
                var cyclicWalk = new List<TVertex> { currentCycleVertex };
                var finished = false;
                while (!finished)
                {
                    foreach (var predecessor in this.GetIncomingNeighbors(currentCycleVertex))
                    {
                        if (!predecessorCounts.TryGetValue(predecessor, out var predecessorCount))
                        {
                            continue;
                        }

                        if (predecessorCount != 0)
                        {
                            predecessorCounts[currentCycleVertex] = -1;

                            currentCycleVertex = predecessor;
                            cyclicWalk.Add(currentCycleVertex);
                            finished = predecessorCounts[predecessor] == -1;
                            break;
                        }
                    }
                }

                cyclicWalk.Reverse();

                var cycle = new List<TVertex>();
                var startingVertex = cyclicWalk.First();
                cycle.Add(startingVertex);
                foreach (var vertex in cyclicWalk.Skip(1))
                {
                    if (!vertex.Equals(startingVertex))
                    {
                        cycle.Add(vertex);
                    }
                    else
                    {
                        break;
                    }
                }

                cycle.Add(startingVertex);

                this.ThrowCycle(cycle, formatCycle);
            }

            return result;
        }

        public override IEnumerable<TVertex> Vertices
            => this._vertices;

        public override IEnumerable<TVertex> GetOutgoingNeighbors(TVertex from)
        {
            return this._successorMap.TryGetValue(from, out var successorSet)
                           ? successorSet.Keys
                           : Enumerable.Empty<TVertex>();
        }

        public override IEnumerable<TVertex> GetIncomingNeighbors(TVertex to)
        {
            return this._predecessorMap.TryGetValue(to, out var predecessors)
                           ? predecessors
                           : Enumerable.Empty<TVertex>();
        }
    }
}
