namespace AsyncApiFileSystem;

/// <summary>
/// Wrapper for <see cref="SessionCore{Job, In, IdFact, Id}"/> to simplify session type names.
/// </summary>
/// <typeparam name="Job">Type of the job.</typeparam>
/// <typeparam name="In">Type of inputs of jobs.</typeparam>
/// <typeparam name="IdFact">Type of the id factory.</typeparam>
/// <typeparam name="Id">Type of the id (key) of jobs.</typeparam>
public abstract class SessionWrapper<Job, In, IdFact, Id> : ISession<Job, In, IdFact, Id>
    where Job : IJob<Id, In>
    where Id : IComparable<Id>, IEquatable<Id>
    where IdFact : IIdFactory<Id>
{
    // data
    readonly ISession<Job, In, IdFact, Id> Session;
    // ctor
    /// <summary>
    /// Wrapps the <paramref name="session"/> exposing its functionalities.
    /// </summary>
    /// <param name="session">Session to be wrapped.</param>
    protected SessionWrapper(ISession<Job, In, IdFact, Id> session)
        => Session = session;


    // ISession
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetJobDir(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    public Res<string> GetJobDir(Id id)
        => Session.GetJobDir(id);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetNbJobs"/>
    /// </summary>
    public Res<int> GetNbJobs()
        => Session.GetNbJobs();
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.DoesJobExist(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job to investigate.</param>
    public bool DoesJobExist(Id id)
        => Session.DoesJobExist(id);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetStatus(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job to get the status of.</param>
    public Res<JobStatus<Id>> GetStatus(Id id)
        => Session.GetStatus(id);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetAllIds"/>
    /// </summary>
    public Res<HashSet<Id>> GetAllIds()
        => Session.GetAllIds();
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetDownloadPath(Id, string)"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="result">Name of the result file.</param>
    /// <returns></returns>
    public Res<string> GetDownloadPath(Id id, string result)
        => Session.GetDownloadPath(id, result);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetDownloadPathZipped(Id, IEnumerable{string}, Opt{string})"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="results">Set of the result files to zip.</param>
    /// <param name="optZipFileName">Optional name for the zip file.</param>
    public Res<string> GetDownloadPathZipped(Id id, IEnumerable<string> results, Opt<string> optZipFileName = default)
        => Session.GetDownloadPathZipped(id, results, optZipFileName);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.GetDownloadPathZippedAll(Id, Opt{string})"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="optZipFileName">Optional name for the zip file.</param>
    public Res<string> GetDownloadPathZippedAll(Id id, Opt<string> optZipFileName = default)
        => Session.GetDownloadPathZippedAll(id, optZipFileName);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.ReadText(Id, string)"/>
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="filename">Name of the file in the job's execution directory to read as text.</param>
    public Res<string> ReadText(Id id, string filename)
        => Session.ReadText(id, filename);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.ParseFile{R}(Id, string, Func{StreamReader, R})"/>
    /// </summary>
    /// <typeparam name="R">Type to parse the file's text into.</typeparam>
    /// <param name="id">Id of the job.</param>
    /// <param name="filename">Name of the file to parse.</param>
    /// <param name="parser">Parser that parses all text of the file into an instance of type <typeparamref name="R"/>.</param>
    public Res<R> ParseFile<R>(Id id, string filename, Func<StreamReader, R> parser)
        => Session.ParseFile(id, filename, parser);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.SubmitGetId(Job, In)"/>
    /// </summary>
    /// <param name="job">Job to be submitted.</param>
    /// <param name="input">Inputs of the job.</param>
    public Res<Id> SubmitGetId(Job job, In input)
        => Session.SubmitGetId(job, input);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.SubmitWithId(Job, In, Id)"/>
    /// </summary>
    /// <param name="job">Job to be submitted.</param>
    /// <param name="input">Inputs of the job.</param>
    /// <param name="id">Id of the job.</param>
    public Res<Id> SubmitWithId(Job job, In input, Id id)
        => Session.SubmitWithId(job, input, id);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.Delete(Id)"/>
    /// </summary>
    /// <param name="id">Id of the job to delete.</param>
    public Res Delete(Id id)
        => Session.Delete(id);
    /// <summary>
    /// <inheritdoc cref="ISession{Job, In, IdFact, Id}.DeleteAll"/>
    /// </summary>
    public Res DeleteAll()
        => Session.DeleteAll();
}
