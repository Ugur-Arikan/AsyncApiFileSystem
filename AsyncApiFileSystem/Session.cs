using System.ComponentModel;
namespace AsyncApiFileSystem;

/// <summary>
/// An api session for handling long running job requests.
/// </summary>
/// <typeparam name="Job">Type of the job.</typeparam>
/// <typeparam name="In">Type of inputs of jobs.</typeparam>
/// <typeparam name="IdFact">Type of the id factory.</typeparam>
/// <typeparam name="Id">Type of the id (key) of jobs.</typeparam>
public class Session<Job, In, IdFact, Id>
    where Job : IJob<Id, In>
    where Id : IComparable<Id>, IEquatable<Id>
    where IdFact : IIdFactory<Id>
{
    // data
    readonly HashSet<string> JobResults;
    readonly Paths<Id, IdFact> Paths;


    // ctor
    Session(Paths<Id, IdFact> paths, HashSet<string> jobResults)
    {
        Paths = paths;
        JobResults = jobResults;
    }
    /// <summary>
    /// Tries to create a new session and returns the result.
    /// </summary>
    /// <param name="rootDirectory">Root directory of jobs of the session.</param>
    /// <param name="idFactory">Id factory of the session..</param>
    /// <param name="jobResults">Names of results of jobs.</param>
    /// <returns></returns>
    public static Res<Session<Job, In, IdFact, Id>> New(string rootDirectory, IdFact idFactory, HashSet<string> jobResults)
    {
        return
            Ok().Try(() =>
            {
                if (!Directory.Exists(rootDirectory))
                    Directory.CreateDirectory(rootDirectory);
            })
            .Map(() => new Paths<Id, IdFact>(rootDirectory, idFactory))
            .Map(paths => new Session<Job, In, IdFact, Id>(paths, jobResults));
    }


    // method - io
    /// <summary>
    /// Returns Ok of the execution directory of the job with the given <paramref name="id"/>;
    /// Err if the directory is absent.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    public Res<string> GetJobDir(Id id)
    {
        string path = Paths.DirOf(id);
        return OkIf<string>(Directory.Exists(path), path);
    }


    // method - get
    /// <summary>
    /// Returns the number of jobs.
    /// </summary>
    public Res<int> GetNbJobs()
        => Paths.GetNbJobs();
    /// <summary>
    /// Returns whether the job with the given <paramref name="id"/> exists or not.
    /// </summary>
    /// <param name="id">Id of the job to investigate.</param>
    public bool Exists(Id id)
        => Paths.Exists(id);
    /// <summary>
    /// Tries to get and return the status of the job with the given <paramref name="id"/>;
    /// returns Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job to get the status of.</param>
    public Res<JobStatus<Id>> GetStatus(Id id)
        => JobStatus<Id>.New(id, Paths);
    /// <summary>
    /// Tries to get the set of id's of all existing jobs;
    /// returns Err if it fails.
    /// </summary>
    public Res<HashSet<Id>> GetAllIds()
        => Paths.GetAllIds();

    
    // method - download
    /// <summary>
    /// Returns Ok of the download path of the given <paramref name="result"/> file of the job with the given <paramref name="id"/>;
    /// returns the Err if it fails or is absent.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="result">Name of the result file.</param>
    /// <returns></returns>
    public Res<string> GetDownloadPath(Id id, string result)
    {
        string path = Paths.FileOf(id, result);
        return OkIf(File.Exists(path), path, string.Format("Required file '{0}' does not exist.", Path.GetFileName(path)));
    }
    /// <summary>
    /// Tries to zip the desired <paramref name="results"/> files of the job with the given <paramref name="id"/>
    /// and returns Ok of the download path of the zip file;
    /// returns the Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="results">Set of the result files to zip.</param>
    /// <param name="optZipFileName">Optional name for the zip file.</param>
    public Res<string> GetDownloadPathZipped(Id id, IEnumerable<string> results, Opt<string> optZipFileName = default)
    {
        return results
            .Select(result => GetDownloadPath(id, result))
            .TryUnwrap()
            .FlatMap(paths => Paths.Zip(id, paths, optZipFileName));
    }
    /// <summary>
    /// Tries to zip all result files of the job with the given <paramref name="id"/>
    /// and returns Ok of the download path of the zip file;
    /// returns the Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="optZipFileName">Optional name for the zip file.</param>
    public Res<string> GetDownloadPathZippedAll(Id id, Opt<string> optZipFileName = default)
        => GetDownloadPathZipped(id, JobResults, optZipFileName);


    // method - parse
    /// <summary>
    /// Tries to read and return all text of the file with the given <paramref name="filename"/>
    /// in the execution directory of the job with the given <paramref name="id"/>;
    /// returns the Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="filename">Name of the file in the job's execution directory to read as text.</param>
    public Res<string> ReadText(Id id, string filename)
        => GetDownloadPath(id, filename).TryMap(path => File.ReadAllText(path));
    /// <summary>
    /// Tries to parse the file into an instance of type <typeparamref name="R"/> and returns the result.
    /// </summary>
    /// <typeparam name="R">Type to parse the file's text into.</typeparam>
    /// <param name="id">Id of the job.</param>
    /// <param name="filename">Name of the file to parse.</param>
    /// <param name="parser">Parser that parses all text of the file into an instance of type <typeparamref name="R"/>.</param>
    public Res<R> ParseFile<R>(Id id, string filename, Func<StreamReader, R> parser)
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
    /// <summary>
    /// Tries to submit the <paramref name="job"/> with the given <paramref name="input"/>;
    /// while auto-generating its id and returing it;
    /// returns Err if submission fails.
    /// </summary>
    /// <param name="job">Job to be submitted.</param>
    /// <param name="input">Inputs of the job.</param>
    public Res<Id> SubmitGetId(Job job, In input)
    {
        var resId = Paths.NewId();
        if (resId.IsErr)
            return resId;
        Id id = resId.Unwrap();

        return SubmitWithId(job, input, id);
    }
    /// <summary>
    /// Tries to submit the <paramref name="job"/> with the given <paramref name="input"/> and given <paramref name="id"/>
    /// and returns back the Ok(id) if it succeeds;
    /// returns Err if submission fails.
    /// </summary>
    /// <param name="job">Job to be submitted.</param>
    /// <param name="input">Inputs of the job.</param>
    /// <param name="id">Id of the job.</param>
    public Res<Id> SubmitWithId(Job job, In input, Id id)
    {
        string jobdir = Paths.DirOf(id);
        if (Directory.Exists(jobdir))
            return Err<Id>(string.Format("Job directory with id '{0}' already exists.", id));

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
        return successfullySubmit.Map(() => id);
    }


    // method - delete
    /// <summary>
    /// Tries to delete the job with the given <paramref name="id"/>, and returns the result.
    /// </summary>
    /// <param name="id">Id of the job to delete.</param>
    public Res Delete(Id id)
        => Paths.Delete(id);
    /// <summary>
    /// Tries to delete all existing jobs and returns the result.
    /// </summary>
    public Res DeleteAll()
        => Paths.GetAllIds()
        .Map(ids => ids.Select(id => Delete(id)).Reduce(false))
        .Flatten();
    /// <summary>
    /// Tries to delete a file with the given <paramref name="path"/>, and returns the result.
    /// </summary>
    /// <param name="path">Path of the file to delete.</param>
    Res DeleteFile(string path)
        => OkIf(File.Exists(path)).Try(() => File.Delete(path));


    // run
    void RunInBackground(object? _sender, DoWorkEventArgs e)
    {
        // args
        if (e.Argument == null)
            throw Exc.MustNotReach;
        var args = (object[])e.Argument;
        Id id = (Id)args[0];
        string jobdir = Paths.DirOf(id);
        Job job = (Job)args[1];

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
