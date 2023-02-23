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
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetJobDir(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    public Res<string> GetJobDir(Id id)
    {
        string path = Paths.DirOf(id);
        return OkIf<string>(Directory.Exists(path), path);
    }


    // method - get
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetNbJobs"/>
    /// </summary>
    public Res<int> GetNbJobs()
        => Paths.GetNbJobs();
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.DoesJobExist(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job to investigate.</param>
    public bool DoesJobExist(Id id)
        => Paths.Exists(id);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetStatus(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job to get the status of.</param>
    public Res<JobStatus<Id>> GetStatus(Id id)
        => JobStatus<Id>.New(id, Paths);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetAllIds"/>
    /// </summary>
    public Res<HashSet<Id>> GetAllIds()
        => Paths.GetAllIds();


    // method - download
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetDownloadPath(Id, string)"/>
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
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetDownloadPathZipped(Id, IEnumerable{string}, Opt{string})"/>
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
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetDownloadPathZippedAll(Id, Opt{string})"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="optZipFileName">Optional name for the zip file.</param>
    public Res<string> GetDownloadPathZippedAll(Id id, Opt<string> optZipFileName = default)
        => GetDownloadPathZipped(id, JobResults, optZipFileName);


    // method - parse
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.ReadText(Id, string)"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="filename">Name of the file in the job's execution directory to read as text.</param>
    public Res<string> ReadText(Id id, string filename)
        => GetDownloadPath(id, filename).TryMap(path => File.ReadAllText(path));
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.ParseFile{R}(Id, string, Func{StreamReader, R})"/>
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
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.SubmitGetId(Job, In)"/>
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
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.SubmitWithId(Job, In, Id)"/>
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
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.Delete(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job to delete.</param>
    public Res Delete(Id id)
        => Paths.Delete(id);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.DeleteAll"/>
    /// </summary>
    public Res DeleteAll()
        => Paths.GetAllIds()
        .Map(ids => ids.Select(id => Delete(id)).Reduce(false))
        .Flatten();


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
