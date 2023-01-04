using System.IO.Compression;

namespace AsyncApiFileSystem;

internal record Paths<Id, IdFact>(string Dir, IdFact IdFactory)
    where Id : IComparable<Id>, IEquatable<Id>
    where IdFact : IIdFactory<Id>
{
    // paths
    public string DirOf(Id id)
        => Path.Join(Dir, IdFactory.ToDirectoryName(id));
    public string BegOf(Id id)
        => Path.Join(DirOf(id), "___beg___.txt");
    public string EndOf(Id id)
        => Path.Join(DirOf(id), "___end___.txt");
    public string ErrOf(Id id)
        => Path.Join(DirOf(id), "___err___.txt");
    public string FileOf(Id id, string filename)
        => Path.Combine(DirOf(id), filename);

    // id
    public Res<Id> NewId()
        => IdFactory.NewId(Dir);
    public bool Exists(Id id)
        => Directory.Exists(Path.Join(Dir, DirOf(id)));


    // io
    public Res<int> GetNbJobs()
        => OkIf(Directory.Exists(Dir))
        .TryMap(() => Directory.GetDirectories(Dir).Length);
    public Res<HashSet<Id>> GetAllIds()
    {
        return OkIf(Directory.Exists(Dir))
            .TryMap(() => Directory.GetDirectories(Dir))
            .TryMap(dirs => dirs.Select(dir => IdFactory.ParseId(Path.GetFileName(dir))).ToHashSet());
    }
    public Res CreateMissingDir(Id id)
    {
        DirectoryInfo dir = new(DirOf(id));
        return OkIf(!dir.Exists).Try(() => dir.Create());
    }
    public Res Delete(Id id)
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
    internal Res<string> Zip(Id id, IEnumerable<string> paths, Opt<string> optZipFileName = default)
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
