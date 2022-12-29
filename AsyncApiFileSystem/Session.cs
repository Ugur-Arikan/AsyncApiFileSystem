using System.ComponentModel;
namespace AsyncFileSystem;

public class Session<T, I, F, K>
    where T : IJob<K, I>
    where K : IComparable<K>, IEquatable<K>
    where F : IRunIdFactory<K>
{
    // data
    readonly HashSet<string> JobResults;
    readonly Paths<K, F> Paths;


    // ctor
    Session(Paths<K, F> paths, HashSet<string> jobResults)
    {
        Paths = paths;
        JobResults = jobResults;
    }
    public static Res<Session<T, I, F, K>> New(string dir, F idFact, HashSet<string> jobResults)
    {
        return
            Ok().Try(() =>
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            })
            .Map(() => new Paths<K, F>(dir, idFact))
            .Map(paths => new Session<T, I, F, K>(paths, jobResults));
    }


    // method - io
    public Res<string> GetJobDir(K id)
    {
        string path = Paths.DirOf(id);
        return OkIf(Directory.Exists(path)).Map(path);
    }


    // method - get
    public Res<int> GetNbResults()
        => Paths.GetNbResults();
    public Res<bool> Exists(K id)
        => Paths.Exists(id);
    public Res<RunStatus<K>> GetStatus(K id)
        => RunStatus<K>.New(id, Paths);
    public Res<HashSet<K>> GetAllIds()
        => Paths.GetAllIds();

    
    // method - download
    public Res<string> GetDownloadPath(K id, string result)
        => Ok()
        .Map(() => Paths.FileOf(id, result))
        .Map(path => File.Exists(path) ? Ok(path) : Err<string>(string.Format("Required file '{0}' does not exist.", Path.GetFileName(path))))
        .Flatten();
    public Res<string> GetDownloadPathZipped(K id, IEnumerable<string> results, Opt<string> optZipFileName = default)
    {
        return
            results.Select(result => GetDownloadPath(id, result))
            .TryUnwrap()
            .Map(paths => Paths.Zip(id, paths, optZipFileName)).Unwrap();
    }
    public Res<string> GetDownloadPathZippedAll(K id, Opt<string> optZipFileName = default)
        => GetDownloadPathZipped(id, JobResults, optZipFileName);


    // method - parse
    public Res<string> ReadText(K id, string filename)
        => GetDownloadPath(id, filename).TryMap(path => File.ReadAllText(path));
    public Res<R> ParseFile<R>(K id, string filename, Func<StreamReader, R> parser)
    {
        return
            GetDownloadPath(id, filename)
            .TryMap(path =>
            {
                using var reader = new StreamReader(path);
                return parser(reader);
            });
    }


    // method - submit
    public Res<K> SubmitGetId(T job, I input)
    {
        var resId = Paths.NewId();
        if (resId.IsErr)
            return resId;
        K id = resId.Unwrap();
        string jobdir = Paths.DirOf(id);

        return SubmitWithId(job, input, id).Map(() => id);
    }
    public Res<K> SubmitWithId(T job, I input, K id)
    {
        string jobdir = Paths.DirOf(id);
        if (Directory.Exists(jobdir))
            return Err<K>(string.Format("Job directory with id '{0}' already exists.", id));

        var successfullySubmit =
            Paths.CreateMissingDir(id)
            .TryMap(() => job.Init(id, input)).Flatten()
            .Do(() =>
            {
                BackgroundWorker worker = new();
                worker.DoWork += RunInBackground;
                worker.RunWorkerAsync(new object[2] { id, job });
            });

        successfullySubmit.DoIfErr(() => Paths.Delete(id));
        return successfullySubmit.Map(id);
    }


    // method - delete
    public Res Delete(K id)
        => Paths.Delete(id);
    public Res DeleteAll()
        => Paths.GetAllIds()
        .Map(ids => ids.Select(id => Delete(id)).Reduce(false))
        .Flatten();
    public Res DeleteFile(string path)
        => OkIf(File.Exists(path)).Try(() => File.Delete(path));


    // run
    void RunInBackground(object? _sender, DoWorkEventArgs e)
    {
        // args
        if (e.Argument == null)
            throw Exc.MustNotReach;
        var args = (object[])e.Argument;
        K id = (K)args[0];
        string jobdir = Paths.DirOf(id);
        T job = (T)args[1];

        // paths
        string pathErr = Paths.ErrOf(id);

        // writers
        var dictWriters = Writers.CreateDict(jobdir, JobResults);
        if (LogIfErr(pathErr, dictWriters).IsErr)
            return;
        using var writers = new Writers(dictWriters.Unwrap());

        // run
        var resRun =
            LogNow(Paths.BegOf(id))
            .Map(() => job.Run(id, writers.Dict)).Flatten();

        // end
        var resEnd = LogNow(Paths.EndOf(id));

        LogIfErr(pathErr, resRun);
        LogIfErr(pathErr, resEnd);
    }
    
    
    // log
    static Res LogNow(string path)
    {
        return Ok().Try(() =>
        {
            using var sw = new StreamWriter(path);
            sw.WriteLine(DateTime.Now.ToString(Singletons.FmtTime));
        });
    }
    static Res LogIfErr(string pathErr, Res res)
    {
        if (res.IsErr)
            LogIfErr(pathErr, res.ToString());
        return res;
    }
    static Res<R> LogIfErr<R>(string pathErr, Res<R> res)
    {
        if (res.IsErr)
            LogIfErr(pathErr, res.ToString());
        return res;
    }
    static void LogIfErr(string pathErr, Opt<string> errMessage)
    {
        if (errMessage.IsSome)
        {
            using var sw = new StreamWriter(pathErr, true);
            sw.WriteLine(errMessage);
        }
    }
}
