namespace Examples;

/// <summary>
/// Job configurations.
/// </summary>
internal static class ConfigJob
{
    /// <summary>
    /// Set of result/output files that will be created by the optimization job.
    /// The files will automatically be generated, and the writers will be made available to the job's run method:
    /// <code>OptimizationJob.Run</code>
    /// </summary>
    internal static readonly HashSet<string> ResultFiles = new() { "flows.csv", "costs.csv" };
    /// <summary>
    /// Root directory of jobs of the session.
    /// </summary>
    internal static readonly string RootDirectory = Path.Join(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()?.Location), "OptimizationRuns");
    /// <summary>
    /// Amount of milliseconds <see cref="OptimizationJob"/> will wait to simulate a long running process.
    /// </summary>
    internal const int JobDelayMillisecons = 20_000;
}
