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

My route finder is Dijkstra's algorithm. It finds the cheapest possible path from a start location to a specified goal. If the start and the goal are the same place, it immediately returns a route containing only that place with a cost of 0. Otherwise, beginning at the start, it repeatedly picks the cheapest place it can reach out of a priority queue. The priority queue keeps the cheapest available place at the front, sifting up when a place is added and sifting down when a place is removed.

The algorithm then checks the neighbors of the picked place. If a neighbor has never been reached before, or if it's cheaper to reach it through this place than the cheapest cost found to it so far, the neighbor is added to the priority queue. At the same time, `bestCost` and `cameFrom` are updated. `bestCost` is a dictionary that stores the cheapest cost found so far from the start to each reached place. `cameFrom` is a dictionary that stores the previous place on the cheapest route to each place. On every iteration the algorithm skips outdated queue entries, where a cheaper route to that place has already been found, so that no unnecessary checks are made.

Before searching through the current place's neighbors, the algorithm checks if the current place is the goal. If it is, it builds a list by following `cameFrom` from the goal back to the start. Since this gives the places in reverse order, the list is flipped before it's returned, giving the cheapest path to the goal. It is safe to stop as soon as the goal is picked out, because no other place in the queue can lead to a cheaper path there. If the goal can't be reached, the queue eventually runs empty. The algorithm then returns a `RouteResult` with `Found` set to false and an empty route, instead of throwing an error.

`ReachabilityQuery` uses the same framework. What separates it from Dijkstra's algorithm is that it doesn't have a goal to find. Instead, it finds all places whose `bestCost` is within a set `maxTravelCost`. The budget check is located right before a neighbor is saved in `bestCost` and added to the queue. The query stops by itself, not because it reaches a goal (as there is none), but because nothing over the budget is ever added to the queue, so the queue eventually becomes empty. It then returns `bestCost` directly, since it can only contain places within the budget. The goal check, `cameFrom` and the path reconstruction have been removed, as no route needs to be built.

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

### Average baseline world-query workload (`--baseline`, medium world, 5 runs)

| Measurement | Before (starter) | After (phase 2) |
| --- | --- | --- |
| World build | 1.3993 ms | 1.3309 ms |
| Preparation | 0.0628 ms | 0.7023 ms |
| Questions (5000 iterations) | 434.4452 ms | 0.2100 ms |
| Allocated during questions | 440000 bytes | 0 bytes |
| Checksum | 6267100 | 6267100 |

### Average baseline world-query workload, large world (`--baseline` with `CreateLarge()`, 5 runs)

| Measurement | Before (starter) | After (phase 2) |
| --- | --- | --- |
| World build | 8.2625 ms | 7.9687 ms |
| Preparation | 0.0928 ms | 19.4650 ms |
| Questions (5000 iterations) | 7145.3766 ms | 0.7216 ms |
| Allocated during questions | 447976 bytes | 0 bytes |
| Checksum | 99787400 | 99787400 |

### Average route-finder workload (`--routes`, medium world, 200 requests, 5 runs)

| Measurement | Before (new WorldQueries per finder) | After (shared WorldQueries) |
| --- | --- | --- |
| Elapsed | 126.1680 ms | 89.5749 ms |
| Per request | 0.6308 ms | 0.4479 ms |
| Allocated | 115352.0 KB | 42042.2 KB |
| Checksum | 31044 | 31044 |



## 7. Development reflection

While adding allocation measurement to the `--baseline` workload, the query loop reported 40 bytes allocated, even though my `FindLocationById` and `FindOutgoingConnections` only return existing objects and should not allocate anything. I suspected the measurement itself rather than my code. The allocation counter was read before `Stopwatch.StartNew()`, and since Stopwatch is a class, creating it allocates an object on the heap. That allocation was therefore counted as part of the loop. To test this, I moved the counter reading so it happens after the stopwatch is created. The result dropped to 0 bytes, which confirmed the cause. I applied the same ordering to the `--routes` workload so they work the same way.

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.
