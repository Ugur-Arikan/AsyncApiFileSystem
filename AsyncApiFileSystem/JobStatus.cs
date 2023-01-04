namespace AsyncApiFileSystem;

/// <summary>
/// Status of the run (execution) of a particular job.
/// </summary>
/// <typeparam name="Id">Type of the id (key) of the job.</typeparam>
/// <param name="JobId">Id (key) of the job.</param>
/// <param name="TimeBegin">Time when the job began its execution.</param>
/// <param name="TimeEnd">Time when the job's execution ended; None if it is still ongoing.</param>
/// <param name="Error">Error message associated with the failure of the job; None if there exists no error.</param>
public record JobStatus<Id>
    (
    Id JobId,
    DateTime TimeBegin,
    Opt<DateTime> TimeEnd,
    Opt<string> Error
    )
    where Id : IComparable<Id>, IEquatable<Id>
{
    // ctor
    internal static Res<JobStatus<Id>> New<F>(Id id, Paths<Id, F> paths) where F : IIdFactory<Id>
    {
        string pathEnd = paths.EndOf(id);
        string pathErr = paths.ErrOf(id);

        var resBeg = Paths<Id, F>.ParseTime(paths.BegOf(id));
        if (resBeg.IsErr)
            return Err<JobStatus<Id>>(resBeg.ToString());

        var resEnd = !File.Exists(pathEnd) ? Ok(None<DateTime>()) : Paths<Id, F>.ParseTime(paths.EndOf(id)).Map(dt => Some(dt));
        if (resEnd.IsErr)
            return Err<JobStatus<Id>>(resEnd.ToString());

        var resErr = !File.Exists(pathErr) ? Ok(None<string>()) : Ok().TryMap(() => File.ReadAllText(pathErr)).Map(err => Some(err));
        if (resErr.IsErr)
            return Err<JobStatus<Id>>(resErr.ToString());

        return Ok(new JobStatus<Id>(id, resBeg.Unwrap(), resEnd.Unwrap(), resErr.Unwrap()));
    }


    // method
    /// <summary>
    /// Returns whether the job is completed or not.
    /// </summary>
    public bool IsCompleted => TimeEnd.IsSome;
    /// <summary>
    /// Returns whether the job execution encountered an error or not.
    /// </summary>
    public bool IsError => Error.IsSome;
    /// <summary>
    /// Returns whether the job is still running or not.
    /// </summary>
    public bool IsRunning => TimeEnd.IsNone && Error.IsNone;


    /// <summary>
    /// Converts the status to deserialization-friedly version using nullable's.
    /// </summary>
    /// <returns></returns>
    public JobStatusJson<Id> ToJsonFriendly()
        => new(this);
}

/// <summary>
/// Json friendly version of the job status.
/// </summary>
/// <typeparam name="Id">Type of the id (key) of the job.</typeparam>
/// <param name="JobId">Id (key) of the job.</param>
/// <param name="TimeBegin">Time when the job began its execution.</param>
/// <param name="TimeEnd">Time when the job's execution ended; None if it is still ongoing.</param>
/// <param name="Error">Error message associated with the failure of the job; None if there exists no error.</param>
public record JobStatusJson<Id>
    (
    Id JobId,
    DateTime TimeBegin,
    DateTime? TimeEnd,
    string? Error
    )
    where Id : IComparable<Id>, IEquatable<Id>
{
    internal JobStatusJson(JobStatus<Id> jobStatus)
        : this(jobStatus.JobId, jobStatus.TimeBegin, jobStatus.TimeEnd.Match(x => x, default(DateTime?)), jobStatus.Error.Match(x => x, default(string?)))
    {
    }


    // method
    /// <summary>
    /// Returns whether the job is completed or not.
    /// </summary>
    public bool IsCompleted => TimeEnd != null;
    /// <summary>
    /// Returns whether the job execution encountered an error or not.
    /// </summary>
    public bool IsError => Error != null;
    /// <summary>
    /// Returns whether the job is still running or not.
    /// </summary>
    public bool IsRunning => TimeEnd == null && Error == null;
}