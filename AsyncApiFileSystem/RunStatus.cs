namespace AsyncApiFileSystem;

/// <summary>
/// Status of the run (execution) of a particular job.
/// </summary>
/// <typeparam name="K">Type of the id (key) of the job.</typeparam>
/// <param name="Id">Id (key) of the job.</param>
/// <param name="TimeBegin">Time when the job began its execution.</param>
/// <param name="TimeEnd">Time when the job's execution ended; None if it is still ongoing.</param>
/// <param name="Error">Error message associated with the failure of the job; None if there exists no error.</param>
public record RunStatus<K>(
    K Id,
    DateTime TimeBegin,
    Opt<DateTime> TimeEnd,
    Opt<string> Error
    )
    where K : IComparable<K>, IEquatable<K>
{
    // ctor
    internal static Res<RunStatus<K>> New<F>(K id, Paths<K, F> paths) where F : IRunIdFactory<K>
    {
        string pathEnd = paths.EndOf(id);
        string pathErr = paths.ErrOf(id);

        var resBeg = Paths<K, F>.ParseTime(paths.BegOf(id));
        if (resBeg.IsErr)
            return Err<RunStatus<K>>(resBeg.ToString());

        var resEnd = !File.Exists(pathEnd) ? Ok(None<DateTime>()) : Paths<K, F>.ParseTime(paths.EndOf(id)).Map(dt => Some(dt));
        if (resEnd.IsErr)
            return Err<RunStatus<K>>(resEnd.ToString());

        var resErr = !File.Exists(pathErr) ? Ok(None<string>()) : Ok().TryMap(() => File.ReadAllText(pathErr)).Map(err => Some(err));
        if (resErr.IsErr)
            return Err<RunStatus<K>>(resErr.ToString());

        return Ok(new RunStatus<K>(id, resBeg.Unwrap(), resEnd.Unwrap(), resErr.Unwrap()));
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
}
