namespace AsyncApiFileSystem;

/// <summary>
/// An api session for handling long running job requests.
/// </summary>
/// <typeparam name="Job">Type of the job.</typeparam>
/// <typeparam name="In">Type of inputs of jobs.</typeparam>
/// <typeparam name="IdFact">Type of the id factory.</typeparam>
/// <typeparam name="Id">Type of the id (key) of jobs.</typeparam>
public interface ISession<Job, In, IdFact, Id>
    where Job : IJob<Id, In>
    where Id : IComparable<Id>, IEquatable<Id>
    where IdFact : IIdFactory<Id>
{
    // get - io
    /// <summary>
    /// Returns Ok of the execution directory of the job with the given <paramref name="id"/>;
    /// Err if the directory is absent.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    Res<string> GetJobDir(Id id);


    // get
    /// <summary>
    /// Returns the number of jobs.
    /// </summary>
    Res<int> GetNbJobs();
    /// <summary>
    /// Returns whether the job with the given <paramref name="id"/> exists or not.
    /// </summary>
    /// <param name="id">Id of the job to investigate.</param>
    bool DoesJobExist(Id id);
    /// <summary>
    /// Tries to get and return the status of the job with the given <paramref name="id"/>;
    /// returns Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job to get the status of.</param>
    Res<JobStatus<Id>> GetStatus(Id id);
    /// <summary>
    /// Tries to get the set of id's of all existing jobs;
    /// returns Err if it fails.
    /// </summary>
    Res<HashSet<Id>> GetAllIds();


    // result
    /// <summary>
    /// Returns Ok of the download path of the given <paramref name="result"/> file of the job with the given <paramref name="id"/>;
    /// returns the Err if it fails or is absent.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="result">Name of the result file.</param>
    /// <returns></returns>
    Res<string> GetDownloadPath(Id id, string result);
    /// <summary>
    /// Tries to zip the desired <paramref name="results"/> files of the job with the given <paramref name="id"/>
    /// and returns Ok of the download path of the zip file;
    /// returns the Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="results">Set of the result files to zip.</param>
    /// <param name="optZipFileName">Optional name for the zip file.</param>
    Res<string> GetDownloadPathZipped(Id id, IEnumerable<string> results, Opt<string> optZipFileName = default);
    /// <summary>
    /// Tries to zip all result files of the job with the given <paramref name="id"/>
    /// and returns Ok of the download path of the zip file;
    /// returns the Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="optZipFileName">Optional name for the zip file.</param>
    Res<string> GetDownloadPathZippedAll(Id id, Opt<string> optZipFileName = default);


    // result - file
    /// <summary>
    /// Tries to read and return all text of the file with the given <paramref name="filename"/>
    /// in the execution directory of the job with the given <paramref name="id"/>;
    /// returns the Err if it fails.
    /// </summary>
    /// <param name="id">Id of the job.</param>
    /// <param name="filename">Name of the file in the job's execution directory to read as text.</param>
    Res<string> ReadText(Id id, string filename);
    /// <summary>
    /// Tries to parse the file into an instance of type <typeparamref name="R"/> and returns the result.
    /// </summary>
    /// <typeparam name="R">Type to parse the file's text into.</typeparam>
    /// <param name="id">Id of the job.</param>
    /// <param name="filename">Name of the file to parse.</param>
    /// <param name="parser">Parser that parses all text of the file into an instance of type <typeparamref name="R"/>.</param>
    Res<R> ParseFile<R>(Id id, string filename, Func<StreamReader, R> parser);


    // submit
    /// <summary>
    /// Tries to submit the <paramref name="job"/> with the given <paramref name="input"/>;
    /// while auto-generating its id and returing it;
    /// returns Err if submission fails.
    /// </summary>
    /// <param name="job">Job to be submitted.</param>
    /// <param name="input">Inputs of the job.</param>
    Res<Id> SubmitGetId(Job job, In input);
    /// <summary>
    /// Tries to submit the <paramref name="job"/> with the given <paramref name="input"/> and given <paramref name="id"/>
    /// and returns back the Ok(id) if it succeeds;
    /// returns Err if submission fails.
    /// </summary>
    /// <param name="job">Job to be submitted.</param>
    /// <param name="input">Inputs of the job.</param>
    /// <param name="id">Id of the job.</param>
    Res<Id> SubmitWithId(Job job, In input, Id id);


    // delete
    /// <summary>
    /// Tries to delete the job with the given <paramref name="id"/>, and returns the result.
    /// </summary>
    /// <param name="id">Id of the job to delete.</param>
    Res Delete(Id id);
    /// <summary>
    /// Tries to delete all existing jobs and returns the result.
    /// </summary>
    Res DeleteAll();
}
