# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

- How are locations and connections represented in the baseline?

  Locations and connections are represented as two flat arrays: Location[] (nodes) and Connection[] (directed edges).


- Which operations appear frequent?

  The frequent operations are FindOutgoingConnections and FindLocationByID in WorldQueries.cs. They get run once for every connection and location in the world respectively, searching through the whole array.


- Which operation(s) do you predict may scale poorly?

  Both use linear scans, so total work grows with queries x world size. FindOutgoingConnections is the worse of the two. It scans every connection even though each location has at most 4, and it allocates a new list on every call.


- What will you investigate first?

  I plan to replace the linear scan in FindOutgoingConnections with an adjacency list. This will group the connections by their source location, built once when the world is prepared. I expect this to have the largest impact as it currently scans all connections to find at most four. While the initial build costs a little extra memory, every search will be much cheaper. I will most likely implement something similar in FindLocationByID as well.


## 2. Data representation

I have kept WorldData as it is, and instead added two lookup structures to WorldQueries. They are built once in the constructor, before any queries are made.

FindLocationById now uses a dictionary (hash table) with the location ID as the key. Instead of searching through every location, it looks the ID up directly. This makes it O(1) on average instead of the linear search's O(V). FindOutgoingConnections now uses an adjacency list. The connections are grouped into buckets by their FromLocationId, so each location has its own list of outgoing connections. Instead of scanning all connections, it only looks up the right bucket. This makes it O(1 + d) instead of O(E), where d is the number of outgoing connections. The trade-off is that building the structures costs O(V + E) time once, and uses about O(V + E) extra memory. The dictionaries only store references to the existing Location and Connection objects, not copies. Since the queries are repeated many times, this one-time cost is much cheaper than scanning the arrays on every call.

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
