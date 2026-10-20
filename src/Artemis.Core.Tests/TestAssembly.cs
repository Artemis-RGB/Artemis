using Xunit.Sdk;
using Xunit.v3;

// Artemis.Core keeps brush and effect registrations in static stores and every profile element subscribes to their
// events, so tests registering test plugins would affect profiles of tests running in parallel
[assembly: Parallelization(Mode = ParallelMode.None)]
