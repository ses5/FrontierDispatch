# REPORT.md - Individual Assignment

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
| Find location by ID | Dictionary (hash table) | O(1) average | Stores every `Location` with an int key in a dictionary. This increases work during start, but greatly improves finding a location by ID. This is because a dictionary looks up a location directly by its ID, instead of scanning through every location. This gives it an average complexity of O(1).|
| Get outgoing connections | Adjacency list (dictionary of lists) | O(1 + d) | Stores a list of a place's outgoing `Connection`s and that list in a dictionary with the place's ID as the key. This increases work during start, but greatly improves finding a place's outgoing connections. This looks up the place’s list directly (the 1), and then only goes through that place’s d connections (the d).|
| Priority queue enqueue | Binary min-heap stored in a List | O(log n) | Inserts an element at the end of a list before `SiftUp` moves it upward. It will be moved upward until its parent is less than or equal to itself. Worst case scenario is moving it up the tree's full height, resulting in complexity of O(log n).|
| Priority queue dequeue-min | Binary min-heap stored in a List | O(log n) | The cheapest element is removed from the front, and the last element is moved to the top. Then `SiftDown` moves an element down. It will be moved down until both its children are greater than or equal to itself. Worst case scenario is moving it down the tree's full height, resulting in complexity of O(log n).|
| Route finding | Dijkstra's algorithm with the binary heap | O((V + E) log V) | Each place is removed from the queue at most once, giving about V dequeues. For each place, the algorithm goes through its outgoing connections. As a result, E connections are checked and each one of them can lead to an enqueue. Every queue operation costs log V because of the binary heap. This gives a total complexity of O((V + E) log V).|

## 5. Correctness and testing

These are the 4 tests I feel are most important. 

`PriorityQueue_DequeueAll_ReturnsItemsInAscendingPriorityOrder` enqueues twelve items in mixed order, including duplicate priorities, and then dequeues all of them. They come out in ascending order and the queue ends up empty, showing that the heap invariant holds after many inserts and removals.

`Dijkstra_PrefersCheaperRouteWithMoreSteps` uses a world where the direct connection from 0 to 2 costs 10, while the route through 1 costs 2 + 3 = 5. The finder returns cost 5 and the route [0, 1, 2], showing that it picks the cheapest route rather than the one with the fewest edges, and that the path is reconstructed correctly.

`Reachability_LocationCostingExactlyBudget_IsIncluded` uses a small world where the cheapest costs from location 0 are known in advance. With a budget of 4, a location costing exactly 4 is included while one costing 5 is not, showing that the budget check uses "at most" correctly.

`Dijkstra_SharedWorldQueries_GivesSameRoutesAsOwnWorldQueries` runs 20 deterministic start/goal pairs on the medium world. It compares a route finder that builds its own `WorldQueries` with one that uses a shared instance, and checks that both return the same result, cost and route. This shows that the phase 6 optimization did not change behavior.

Other tests cover start equals goal, an unreachable goal, a zero budget, disconnected locations and null arguments to the new constructors. The `--baseline` and `--routes` workloads also print a checksum, which stayed the same before and after each optimization.

## 6. Performance investigation

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

When looking at the two `--baseline` tables, both show about 440000 bytes allocated (440000 for medium world, 447976 bytes for large world specifically) before any changes were made. This allocated memory is the result of `FindOutgoingConnections` making new lists on every call. By changing `FindOutgoingConnections` so that it uses a prepared list built once in the constructor instead of making new ones on every iteration, the allocated memory was brought down to 0 bytes. This change, together with the dictionary for `FindLocationById`, also had the largest measured effect of all my changes. The questions went from 434 ms to 0.21 ms on the medium world, because each lookup now goes straight to the right location or list instead of scanning through every location and connection. It came at a cost though, since preparation went from 0.06 ms to 0.70 ms (0.09 ms to 19.5 ms on the large world) and the dictionaries take up extra memory. This means I traded more memory and a slower start for much faster queries. For a very small world this might not be worth it, as building the dictionaries can cost more than simply scanning a handful of locations. The benefit shows when the world grows. On the large world the old version became about 16 times slower, while my version only became about 3 times slower.

Memory allocation was also minimized in the `--routes` workload through similar, but not identical changes. Before any changes, it rebuilt `WorldQueries` for every finder, meaning it was rebuilt 200 times. By making it so both `DijkstraRouteFinder` and `ReachabilityQuery` use a shared `WorldQueries`, this number was brought down to 1. The allocated memory didn't go down to 0 this time. The remaining allocations came almost entirely from the working collections created in each `FindRoute` call (210 KB per search). This includes extra arrays created when the collections grow.

The main reason I didn't change this is that it could cause finders and their searches to collide. The change would've meant keeping the collections inside the finder and clearing them between searches, instead of creating new ones each time. If two parts of the game used the same finder at once, they would write to the same collections and ruin each other's results. The memory is also only used during the search and is thrown away right after, which .NET's garbage collector handles cheaply. For these reasons I decided that reducing the allocations further wouldn't be a useful trade-off. If the world became much larger, these remaining allocations are what I would investigate next, for example by giving the collections a starting size. Since the location IDs in the supplied worlds go from 0 upwards without gaps, I would also look into replacing the dictionaries with plain arrays.

## 7. Development reflection

While adding allocation measurement to the `--baseline` workload, the query loop reported 40 bytes allocated, even though my `FindLocationById` and `FindOutgoingConnections` only return existing objects and should not allocate anything. I suspected the measurement itself rather than my code. The allocation counter was read before `Stopwatch.StartNew()`, and since Stopwatch is a class, creating it allocates an object on the heap. That allocation was therefore counted as part of the loop. To test this, I moved the counter reading so it happens after the stopwatch is created. The result dropped to 0 bytes, which confirmed the cause. I applied the same ordering to the `--routes` workload so they work the same way.

## 8. Tools and AI assistance

I've used Claude Code to help me with certain parts of this assignment. Mainly it was used to help me understand things, such as what to do and what certain things meant. I'm quite bad at reading other peoples code, so having Claude help me understand not only made me work more efficiently but has also improved my code-reading skills. 

I also had Claude help me with writing tests. I've never worked with a baseline or the console like this before so adding tests was something I understood little about. When I had Claude make me something like this I always made sure to have it explain to me how it worked so I could read, understand and learn something from it.

The only other times I ever asked Claude to directly change my files for me was during the writing of this report and when I wanted to remove all leftover comments in finished scripts. A great example of what I asked Claude to do was make the tables for me in section 6. I had no clue how to make tables in a .md file, and asking Claude to fill in the tables with all the data simply saved me from a repetitive, tedious task. I also had Claude look through my text to make sure I didn't make any mistakes such as spelling. Claude also wrote drafts for sections 5 and 7 that I then changed and adapted to fit with my work.

## 9. VG extension

VG was not attempted.
