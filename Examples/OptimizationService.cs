using AsyncApiFileSystem.Commons;
using AsyncApiFileSystem;

namespace Examples;

/// <summary>
/// This is a static class providing endpoints for the optimization runs, that pretend to be long-running processes.
/// In the background, the service uses AsyncApiFileSystem to manage inputs and outputs.
/// </summary>
public static class OptimizationService
{
    // ctor
    static OptimizationService()
    {
        var exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()?.Location);
        var rootDirectory = Path.Join(exeDirectory, "OptimizationRuns");
        if (rootDirectory == null)
            throw new NullReferenceException(nameof(rootDirectory));

        Session = 
            CommonSessions.NewWithStringId<OptimizationJob, Input>(rootDirectory, Some(ResultFiles))
            .Unwrap(); // throw if it fails to create the session.

        Console.WriteLine($"Optimization runs will be managed in the following directory:\n{rootDirectory}");
    }


    // data
    /// <summary>
    /// Set of result/output files that will be created by the optimization job.
    /// The files will automatically be generated, and the writers will be made available to the job's run method:
    /// <code>OptimizationJob.Run</code>
    /// </summary>
    static readonly HashSet<string> ResultFiles = new() { "flows.csv", "costs.csv" };
    /// <summary>
    /// An AsyncApiFileSystem Session where:
    /// <list type="bullet">
    /// <item>Job ids are of Guid type.</item>
    /// <item>For auto-generation of job ids, it uses the default IdFactGuid.</item>
    /// <item>Input of each job is of type <see cref="Input"/>.</item>
    /// <item>Finally, the jobs are of type <see cref="OptimizationJob"/>.</item>
    /// </list>
    /// The session will automatically handle the long running optimization tasks in the background,
    /// and manage them using the file system.
    /// </summary>
    static readonly Session<OptimizationJob, Input, IdFactString, string> Session;
    /// <summary>
    /// Amount of milliseconds <see cref="OptimizationJob"/> will wait to simulate a long running process.
    /// </summary>
    internal const int JobDelayMillisecons = 20_000;


    // endpoints
    public static void AddEndpoints(WebApplication app)
    {
        // SUBMIT
        // once called
        // * an optimization run using the input with the provided inputId will be fired in the background,
        // * the endpoint will return the auto-generated unique jobId of the optimization run,
        // * status of the optimziation run can later be investigated using the jobId.
        app.MapGet("/submit/{inputId}",
            (int inputId) =>
            {
                var input = InputsRepo.Get(inputId).IntoRes("input with id: " + inputId);
                var resultJobId = input.FlatMap(input => Session.SubmitGetId(new OptimizationJob(), input));
                return resultJobId.Match
                (
                    whenOk: id => $"Sumbitted optimization run with id: {id}. Execution directory: {Session.GetJobDir(id)}",
                    whenErr: err => $"Failed ot submit the optimization run due to the following error. ${err}"
                );
            });


        // * an optimization run using the input with the provided inputId will be fired in the background,
        // * the job will get the provided jobId (which can be a meaningful scenario name, etc.)
        // * the endpoint will return back the jobId of the optimization run,
        // * status of the optimziation run can later be investigated using the jobId.
        app.MapGet("/submit-with-jobid/{inputId}/{jobId}",
            (int inputId, string jobId) =>
            {
                var input = InputsRepo.Get(inputId).IntoRes("input with id: " + inputId);
                var resultJobId = input.FlatMap(input => Session.SubmitWithId(new OptimizationJob(), input, jobId));
                return resultJobId.Match
                (
                    whenOk: id => $"Sumbitted optimization run with id: {id}. Execution directory: {Session.GetJobDir(id)}",
                    whenErr: err => $"Failed ot submit the optimization run due to the following error. ${err}"
                );
            });

        
        // STATUS
        app.MapGet("/nb-jobs", () => Session.GetNbJobs().IntoHttpResult());
        app.MapGet("/ids", () => Session.GetAllIds().IntoHttpResult());
        app.MapGet("/ids/running", () =>
        {
            return Session.GetAllIds()
                .FlatMap(ids => ids.Select(id => Session.GetStatus(id)).TryUnwrap())
                .Map(statues => statues.Where(s => s.IsRunning).Select(s => s.JobId))
                .IntoHttpResult();
        });
        app.MapGet("/ids/completed", () =>
        {
            return Session.GetAllIds()
                .FlatMap(ids => ids.Select(id => Session.GetStatus(id)).TryUnwrap())
                .Map(statues => statues.Where(s => s.IsCompleted).Select(s => s.JobId))
                .IntoHttpResult();
        });
        app.MapGet("/ids/error", () =>
        {
            return Session.GetAllIds()
                .FlatMap(ids => ids.Select(id => Session.GetStatus(id)).TryUnwrap())
                .Map(statues => statues.Where(s => s.IsError).Select(s => s.JobId))
                .IntoHttpResult();
        });
        app.MapGet("/status", () =>
        {
            return Session.GetAllIds()
               .FlatMap(ids => ids.Select(id => Session.GetStatus(id)).TryUnwrap())
               .IntoHttpResult();
        });
        app.MapGet("/status/{jobId}", (string jobId) => Session.GetStatus(jobId).Map(s => s.ToJsonFriendly()).IntoHttpResult());

        
        // READ RESULTS
        app.MapGet("/flows/{jobId}", (string jobId) => Session.ReadText(jobId, "flows.csv").IntoHttpResult());
        app.MapGet("/costs/{jobId}", (string jobId) => Session.ReadText(jobId, "costs.csv").IntoHttpResult());

        // DOWNLOAD RESULTS
        app.MapGet("/download/flows/{jobId}", (string jobId) => Session.GetDownloadPath(jobId, "flows.csv").IntoFileResult("application/text"));
        app.MapGet("/download/costs/{jobId}", (string jobId) => Session.GetDownloadPath(jobId, "costs.csv").IntoFileResult("application/text"));
        app.MapGet("/download/all-zipped/{jobId}",
            (string jobId) => Session.GetDownloadPathZippedAll(jobId, Some($"results_{jobId}")).IntoFileResult("application/zip"));


        // DELETE
        app.MapGet("/delete/{jobId}", (string jobId) => Session.Delete(jobId).IntoHttpResult());
        app.MapGet("/delete-all", () => Session.DeleteAll().IntoHttpResult());
    }
    
    
    // helpers
    static IResult IntoFileResult(this Res<string> resultPath, string filetype)
    {
        return resultPath.Match(
            whenOk: path => Results.File(path, filetype, Path.GetFileName(path)),
            whenErr: err => Get500(err));
    }
    static IResult IntoHttpResult(this Res result)
    {
        return result.Match(() => Results.Ok(), err => Get500(err));
    }
    static IResult IntoHttpResult<T>(this Res<T> result)
    {
        return result.Match(okValue => Results.Ok(okValue), err => Get500(err));
    }
    static IResult Get500(string error)
        => Results.Problem(statusCode: 500, detail: error);
}
