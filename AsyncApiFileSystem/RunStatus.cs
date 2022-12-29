namespace AsyncFileSystem;

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
    public bool IsCompleted => TimeEnd.IsSome;
    public bool IsError => Error.IsSome;
}
