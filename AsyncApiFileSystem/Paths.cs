using System.IO.Compression;

namespace AsyncApiFileSystem;

internal record Paths<K, F>(string Dir, F IdFact)
    where K : IComparable<K>, IEquatable<K>
    where F : IRunIdFactory<K>
{
    // paths
    public string DirOf(K id)
        => Path.Join(Dir, IdFact.ToDirectoryName(id));
    public string BegOf(K id)
        => Path.Join(DirOf(id), "___beg___.txt");
    public string EndOf(K id)
        => Path.Join(DirOf(id), "___end___.txt");
    public string ErrOf(K id)
        => Path.Join(DirOf(id), "___err___.txt");
    public string FileOf(K id, string filename)
        => Path.Combine(DirOf(id), filename);

    // id
    public Res<K> NewId()
        => IdFact.NewId(Dir);
    public bool Exists(K id)
        => Directory.Exists(Path.Join(Dir, DirOf(id)));


    // io
    public Res<int> GetNbJobs()
        => OkIf(Directory.Exists(Dir))
        .TryMap(() => Directory.GetDirectories(Dir).Length);
    public Res<HashSet<K>> GetAllIds()
    {
        return OkIf(Directory.Exists(Dir))
            .TryMap(() => Directory.GetDirectories(Dir))
            .TryMap(dirs => dirs.Select(dir => IdFact.ParseId(Path.GetFileName(dir))).ToHashSet());
    }
    public Res CreateMissingDir(K id)
    {
        DirectoryInfo dir = new(DirOf(id));
        return OkIf(!dir.Exists).Try(() => dir.Create());
    }
    public Res Delete(K id)
    {
        string path = DirOf(id);
        var dir = new DirectoryInfo(path);

        var resSubdirs =
            OkIf(Directory.Exists(path))
            .OkIf(File.Exists(EndOf(id)), "Completed")
            .TryMap(() => (dir).GetDirectories())
            .TryMap(subs => subs.Select(sub => Ok().Try(() => RecursiveDelete(sub))))
            .FlatMap(results => results.Reduce(false));

        var resJobdir = Ok().Try(() => RecursiveDelete(dir));

        return ReduceResults(resSubdirs, resJobdir);
    }


    // io helpers
    static void RecursiveDelete(DirectoryInfo baseDir)
    {
        if (!baseDir.Exists)
            return;
        foreach (var file in baseDir.EnumerateFiles())
            file.Delete();
        foreach (var dir in baseDir.EnumerateDirectories())
            RecursiveDelete(dir);
        baseDir.Delete(true);
    }
    internal Res<string> Zip(K id, IEnumerable<string> paths, Opt<string> optZipFileName = default)
    {
        string containingDir = DirOf(id);
        string zipFileName = optZipFileName.UnwrapOr(Path.GetRandomFileName);
        string zipPath = Path.Join(containingDir, zipFileName);

        return Ok().TryMap(() =>
        {
            using ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            foreach (var s in paths)
                zip.CreateEntryFromFile(s, Path.GetFileName(s));
            
            return zipPath;
        });
    }
    internal static Res<DateTime> ParseTime(string path)
    {
        if (!File.Exists(path))
            return Err<DateTime>("Cannot find time-file: " + Path.GetFileName(path));

        string text = File.ReadAllText(path);
        if (!DateTime.TryParseExact(text.Trim(), Singletons.FmtTime, Singletons.Culture, System.Globalization.DateTimeStyles.None, out var date))
            return Err<DateTime>(string.Format("Failed to parse '{0}' as time.", text));
        else
            return Ok(date);
    }
}
