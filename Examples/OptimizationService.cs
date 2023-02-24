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

        Console.WriteLine($"Optimization runs will be managed in the following directory:\n{rootDirectory}");
    }


    


    // endpoints
    public static void AddEndpoints(WebApplication app)
{
    // SUBMIT
    // once called
    // * an optimization run using the input with the provided inputId will be fired in the background,
    // * the endpoint will return the auto-generated unique jobId of the optimization run,
    // * status of the optimziation run can later be investigated using the jobId.
    app.MapGet("/submit/{inputId}",
        (OptimizationSession session, int inputId) =>
        {
            var input = InputsRepo.Get(inputId).IntoRes("input with id: " + inputId);
            var resultJobId = input.FlatMap(input => session.SubmitGetId(new OptimizationJob(), input));
            return resultJobId.Match
            (
                whenOk: id => $"Sumbitted optimization run with id: {id}. Execution directory: {session.GetJobDir(id)}",
                whenErr: err => $"Failed ot submit the optimization run due to the following error. ${err}"
            );
        });


    // * an optimization run using the input with the provided inputId will be fired in the background,
    // * the job will get the provided jobId (which can be a meaningful scenario name, etc.)
    // * the endpoint will return back the jobId of the optimization run,
    // * status of the optimziation run can later be investigated using the jobId.
    app.MapGet("/submit-with-jobid/{inputId}/{jobId}",
        (OptimizationSession session, int inputId, string jobId) =>
        {
            var input = InputsRepo.Get(inputId).IntoRes("input with id: " + inputId);
            var resultJobId = input.FlatMap(input => session.SubmitWithId(new OptimizationJob(), input, jobId));
            return resultJobId.Match
            (
                whenOk: id => $"Sumbitted optimization run with id: {id}. Execution directory: {session.GetJobDir(id)}",
                whenErr: err => $"Failed ot submit the optimization run due to the following error. ${err}"
            );
        });

        
    // STATUS
    app.MapGet("/nb-jobs", (OptimizationSession session) => session.GetNbJobs().IntoHttpResult());
    app.MapGet("/ids", (OptimizationSession session) => session.GetAllIds().IntoHttpResult());
    app.MapGet("/ids/running", (OptimizationSession session) =>
    {
        return session.GetAllIds()
            .FlatMap(ids => ids.Select(id => session.GetStatus(id)).TryUnwrap())
            .Map(statues => statues.Where(s => s.IsRunning).Select(s => s.JobId))
            .IntoHttpResult();
    });
    app.MapGet("/ids/completed", (OptimizationSession session) =>
    {
        return session.GetAllIds()
            .FlatMap(ids => ids.Select(id => session.GetStatus(id)).TryUnwrap())
            .Map(statues => statues.Where(s => s.IsCompleted).Select(s => s.JobId))
            .IntoHttpResult();
    });
    app.MapGet("/ids/error", (OptimizationSession session) =>
    {
        return session.GetAllIds()
            .FlatMap(ids => ids.Select(id => session.GetStatus(id)).TryUnwrap())
            .Map(statues => statues.Where(s => s.IsError).Select(s => s.JobId))
            .IntoHttpResult();
    });
    app.MapGet("/status", (OptimizationSession session) =>
    {
        return session.GetAllIds()
            .FlatMap(ids => ids.Select(id => session.GetStatus(id)).TryUnwrap())
            .IntoHttpResult();
    });
    app.MapGet("/status/{jobId}", (OptimizationSession session, string jobId)
        => session.GetStatus(jobId).Map(s => s.ToJsonFriendly()).IntoHttpResult());

        
    // READ RESULTS
    app.MapGet("/flows/{jobId}", (OptimizationSession session, string jobId) => session.ReadText(jobId, "flows.csv").IntoHttpResult());
    app.MapGet("/costs/{jobId}", (OptimizationSession session, string jobId) => session.ReadText(jobId, "costs.csv").IntoHttpResult());


    // DOWNLOAD RESULTS
    app.MapGet("/download/flows/{jobId}", (OptimizationSession session, string jobId)
        => session.GetDownloadPath(jobId, "flows.csv").IntoFileResult("application/text"));
    app.MapGet("/download/costs/{jobId}", (OptimizationSession session, string jobId)
        => session.GetDownloadPath(jobId, "costs.csv").IntoFileResult("application/text"));
    app.MapGet("/download/all-zipped/{jobId}",
        (OptimizationSession session, string jobId)
        => session.GetDownloadPathZippedAll(jobId, Some($"results_{jobId}")).IntoFileResult("application/zip"));


    // DELETE
    app.MapGet("/delete/{jobId}", (OptimizationSession session, string jobId) => session.Delete(jobId).IntoHttpResult());
    app.MapGet("/delete-all", (OptimizationSession session) => session.DeleteAll().IntoHttpResult());
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
