# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

- How are locations and connections represented in the baseline?

  Locations and connections are represented as two flat arrays: Location[] (nodes) and Connection[] (directed edges).


- Which operations appear frequent?

  The frequent operations are FindOutgoingConnections FindLocationByID in WorldQueries.cs. They get run once for every connection and location in the world respectively, searching through the whole array.


- Which operation(s) do you predict may scale poorly?

  Both use linear scans, so total work grows with queries x world size. FindOutgoingConnections is the worse of the two. It scans every connection even though each location has at most 4, and it allocates a new list on every call.


- What will you investigate first?

  I Plan to replace the linear scan in FindOutgoingConnections with an adjacency list. This will group the connections by their source location, built once when the world is prepared. I expect this to have the largest impact as it currently scans all connections to find at most four. While the initial build costs a little extra memory, every search will be much cheaper. I will most likely implement something similar in FindLocationByID as well.


## 2. Data representation

Describe important changes/retained choices, operations affected, and time/memory trade-offs.

## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation |
| --- | --- | --- | --- |
| Find location by ID | | | |
| Get outgoing connections | | | |
| Priority queue enqueue | | | |
| Priority queue dequeue-min | | | |
| Route finding | | | |

## 5. Correctness and testing

Important tests/edge cases and what they demonstrate.

## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.
