using System;
using System.Diagnostics;
using FrontierDispatch.Core;

if (args.Length > 0 && args[0] == "--baseline")
{
    RunBaseline();
    return;
}

if (args.Length > 0 && args[0] == "--routes")
{
    RunRouteWorkload();
    return;
}

// Small sanity-check application.
// This is separate from the larger performance workload below.
WorldData world = SampleWorldFactory.CreateSmall();
var queries = new WorldQueries(world);

Console.WriteLine("Frontier Dispatch - assignment starter");
Console.WriteLine($"Small demo locations: {world.Locations.Count}");
Console.WriteLine($"Find 734: {queries.FindLocationById(734)?.Name ?? "missing"}");
Console.WriteLine();
Console.WriteLine("Run with --baseline to record the supplied baseline workload.");

static void RunRouteWorkload()
{
    WorldData world = WorldDatasetFactory.CreateMedium();
    int locationCount = world.Locations.Count;

    const int routeRequests = 200;

    new DijkstraRouteFinder(world).FindRoute(0, locationCount - 1);

    long checksum = 0;
    
    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    var sw  = Stopwatch.StartNew();

    for (int i = 0; i < routeRequests; ++i)
    {
        int start = (i * 997) % locationCount;
        int goal = (i * 7919 + 1234) % locationCount;

        var finder = new DijkstraRouteFinder(world);
        RouteResult result = finder.FindRoute(start, goal);

        checksum += result.TotalCost;
    }

    sw.Stop();
    long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

    Console.WriteLine("Route-finder workload (new finder per request)");
    Console.WriteLine($"Locations: {locationCount}");
    Console.WriteLine($"Connections: {world.Connections.Count}");
    Console.WriteLine($"Route requests: {routeRequests}");
    Console.WriteLine($"Elapsed: {sw.Elapsed.TotalMilliseconds:F4} ms");
    Console.WriteLine($"Per request: {sw.Elapsed.TotalMilliseconds / routeRequests:F4} ms");
    Console.WriteLine($"Allocated: {allocatedBytes / 1024.0:F1} KB");
    Console.WriteLine($"Checksum: {checksum}");
}

static void RunBaseline()
{
    var worldBuildStopwatch = Stopwatch.StartNew();
    WorldData world = WorldDatasetFactory.CreateMedium();
    worldBuildStopwatch.Stop();
    
    var preparationStopwatch = Stopwatch.StartNew();
    var queries = new WorldQueries(world);
    preparationStopwatch.Stop();
    
    // The checksum provides a stable sanity check that the workload still
    // produces equivalent results after implementation changes.
    int checksum = 0;

    const int iterations = 5000;
    
    var questionsStopwatch = Stopwatch.StartNew();
    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

    for (int i = 0; i < iterations; ++i)
    {
        // Deterministically visit many different location IDs rather than
        // repeatedly querying the same location.
        int id = (i * 997) % world.Locations.Count;

        Location? location = queries.FindLocationById(id);
        checksum += location?.Id ?? 0;
        checksum += queries.FindOutgoingConnections(id).Count;
    }

    questionsStopwatch.Stop();
    long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

    Console.WriteLine("Baseline world-query workload");
    Console.WriteLine($"Locations: {world.Locations.Count}");
    Console.WriteLine($"Connections: {world.Connections.Count}");
    Console.WriteLine($"Iterations: {iterations}");
    Console.WriteLine($"World build Elapsed: {worldBuildStopwatch.Elapsed.TotalMilliseconds:F4} ms");
    Console.WriteLine($"Preparation Elapsed: {preparationStopwatch.Elapsed.TotalMilliseconds:F4} ms");
    Console.WriteLine($"Questions Elapsed: {questionsStopwatch.Elapsed.TotalMilliseconds:F4} ms");
    Console.WriteLine($"Allocated: {allocatedBytes} Bytes");
    Console.WriteLine($"Checksum: {checksum}");
    Console.WriteLine("Absolute time varies by machine. Keep this result for before/after comparison.");
}
