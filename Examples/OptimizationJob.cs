using AsyncApiFileSystem;

namespace Examples;

/// <summary>
/// This class pretends to be a long-running optimization task.
/// It extends IJob&lt;Guid, <see cref="Input"/>>; meaning that:
/// <list type="bullet">
/// <item>the job id's are of type Guid,</item>
/// <item>the inputs to job execution is the Input class.</item>
/// </list>
/// The job must implement two methods:
/// <list type="bullet">
/// <item>Init: this is executed before starting the run,</item>
/// <item>Run: this method is the main execution.</item>
/// </list>
/// </summary>
public class OptimizationJob : IJob<string, Input>
{
    // data
    Input? _input;


    // method
    /// <summary>
    /// This method is executed before starting the run.
    /// </summary>
    public Res Init(string id, Input input)
    {
        if (input == null)
            return Err("input is wrong or missing.");

        _input = input;
        return Ok();
    }
    /// <summary>
    /// This is the main execution step of the job.
    /// <list type="bullet">
    /// <item>It pretends to work for 10 seconds.</item>
    /// <item>Writes some random outputs to two result files: flows.csv and costs.csv; which should be the output.</item>
    /// </list>
    /// </summary>
    public Res Run(string id, Dictionary<string, StreamWriter> resultFiles)
    {
        if (IsThereAnError())
            return Err("something went wrong.");

        // simulate a long running process 
        Thread.Sleep(ConfigJob.JobDelayMillisecons);

        // write some outputs
        var rng = new Random();
        var flowsWriter = resultFiles["flows.csv"];
        flowsWriter.WriteLine("ori,des,flow");
        for (int i = 0; i < 10; i++)
        {
            int ori = rng.Next(0, 100);
            int des = rng.Next(0, 100);
            double flow = rng.NextDouble();
            flowsWriter.WriteLine(string.Format("{0},{1},{2}", ori, des, flow));
        }

        // write some other outputs
        var costsWriter = resultFiles["costs.csv"];
        costsWriter.WriteLine("type,cost");
        costsWriter.WriteLine("transportation," + rng.NextDouble());
        costsWriter.WriteLine("handling," + rng.NextDouble());

        return Ok();
    }
    /// <summary>
    /// Randomly simulates an error just to illustrate how errors are handled.
    /// </summary>
    static bool IsThereAnError()
    {
        return (new Random()).NextDouble() < 0.2;
    }
}
