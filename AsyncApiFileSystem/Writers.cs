namespace AsyncApiFileSystem;

internal class Writers : IDisposable
{
    // data
    internal readonly Dictionary<string, StreamWriter> Dict;
    bool Disposed = false;


    // ctor
    internal Writers(Dictionary<string, StreamWriter> dict)
    {
        Dict = dict;
    }
    internal static Res<Dictionary<string, StreamWriter>> CreateDict(string dir, IEnumerable<string> files)
    {
        var paths = files.Select(f => (f, Path.Join(dir, f)));
        Dictionary<string, StreamWriter> dict = new();
        var allOk = Ok().Try(() =>
        {
            foreach (var (f, p) in paths)
                dict.Add(f, new StreamWriter(p));
        });

        if (allOk.IsErr)
        {
            foreach (var item in dict)
                item.Value.Dispose();
            dict.Clear();
            return Err<Dictionary<string, StreamWriter>>(allOk.ToString());
        }
        else
            return Ok(dict);
    }
    public void Dispose()
    {
        if (Disposed)
            return;

        foreach (var item in Dict)
            item.Value.Dispose();
        Dict.Clear();

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
