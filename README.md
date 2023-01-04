# AsyncApiFileSystem

A lightweight library for a simple api for async long running requests, using the file system.

By simply implementing the `IJob` interface with a couple of methods, the following features are enabled:

* submit long running processes and fire the execution in the background,
* track execution status of long running processes:
    * whether the job is still running or not,
    * start and completion time,
    * execution errors,
    * arbitrary outputs,
* read and/or download result files,
* delete jobs.

Complete auto-generated documentation can be found here:
**[sandcastle-documentation](https://ugur-arikan.github.io/AsyncApiFileSystem/docs/index.html)**.

## Example

The features of the library are illustrated in the **Examples** project. This is a web api that can directly be started to test the endpoints via swagger.

The example demonstrates a real-life example as follows:

* the custom input is defined here: [`Input`](https://github.com/Ugur-Arikan/AsyncApiFileSystem/blob/main/Examples/Input.cs),
* the custom job is implemented here: [`OptimizationJob`](https://github.com/Ugur-Arikan/AsyncApiFileSystem/blob/main/Examples/OptimizationJob.cs),
* and the session is constructed here: [`OptimizationService`](https://github.com/Ugur-Arikan/AsyncApiFileSystem/blob/main/Examples/OptimizationService.cs).

This is sufficient to provide the functionality defined by the endpoints in `OptimizationService.AddEndpoints` method.

```csharp
static void AddEndpoints(WebApplication app)
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
```
