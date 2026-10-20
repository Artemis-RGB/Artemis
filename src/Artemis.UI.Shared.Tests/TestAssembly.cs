using Xunit.Sdk;
using Xunit.v3;

// Same reason as Artemis.Core.Tests: Artemis.Core keeps brush and effect registrations in static stores that tests would
// otherwise share
[assembly: Parallelization(Mode = ParallelMode.None)]
