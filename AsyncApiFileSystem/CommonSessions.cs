using AsyncApiFileSystem.Commons;

namespace AsyncApiFileSystem;

/// <summary>
/// Static class to construct common sessions.
/// </summary>
public static class CommonSessions
{
    // id: string
    /// <summary>
    /// Tries to create:
    /// <code>Session&lt;Job, In, IdFactString, string></code>
    /// 
    /// where:
    /// <list type="bullet">
    /// <item>the keys are of type string, and</item> 
    /// <item>key factory for auto-generated keys is <see cref="IdFactString"/>.</item>
    /// </list>
    /// 
    /// Returns Ok of the session if succeeds, the error if the constructor fails.
    /// </summary>
    /// <typeparam name="Job">Type of the job.</typeparam>
    /// <typeparam name="In">Type of inputs of jobs.</typeparam>
    /// <param name="rootDirectory">Root directory of jobs of the session.</param>
    /// <param name="jobResults">Names of results of jobs.</param>
    public static Res<SessionCore<Job, In, IdFactString, string>> NewWithStringId<Job, In>(string rootDirectory, Opt<HashSet<string>> jobResults = default)
        where Job : IJob<string, In>
    {
        return SessionCore<Job, In, IdFactString, string>.New(rootDirectory, new(), jobResults.UnwrapOr(new HashSet<string>()));
    }
    // id: string & input: FilesInput
    /// <summary>
    /// Tries to create:
    /// <code>Session&lt;Job, FilesInput, IdFactString, string></code>
    /// 
    /// where:
    /// <list type="bullet">
    /// <item>the keys are of type string,</item> 
    /// <item>key factory for auto-generated keys is <see cref="IdFactString"/>, and</item>
    /// <item>input is a collection of files, as <see cref="FilesInput"/>.</item>
    /// </list>
    /// 
    /// Returns Ok of the session if succeeds, the error if the constructor fails.
    /// </summary>
    /// <typeparam name="Job">Type of the job.</typeparam>
    /// <param name="rootDirectory">Root directory of jobs of the session.</param>
    /// <param name="jobResults">Names of results of jobs.</param>
    public static Res<SessionCore<Job, FilesInput, IdFactString, string>> NewWithStringIdFilesInput<Job>(string rootDirectory, Opt<HashSet<string>> jobResults = default)
        where Job : IJob<string, FilesInput>
    {
        return SessionCore<Job, FilesInput, IdFactString, string>.New(rootDirectory, new(), jobResults.UnwrapOr(new HashSet<string>()));
    }


    // id: guid
    /// <summary>
    /// Tries to create:
    /// <code>Session&lt;Job, In, IdFactGuid, Guid></code>
    /// 
    /// where:
    /// <list type="bullet">
    /// <item>the keys are of type Guid, and</item> 
    /// <item>key factory for auto-generated keys is <see cref="IdFactGuid"/>.</item>
    /// </list>
    /// 
    /// Returns Ok of the session if succeeds, the error if the constructor fails.
    /// </summary>
    /// <typeparam name="Job">Type of the job.</typeparam>
    /// <typeparam name="In">Type of inputs of jobs.</typeparam>
    /// <param name="rootDirectory">Root directory of jobs of the session.</param>
    /// <param name="jobResults">Names of results of jobs.</param>
    public static Res<SessionCore<Job, In, IdFactGuid, Guid>> NewWithGuid<Job, In>(string rootDirectory, Opt<HashSet<string>> jobResults = default)
        where Job : IJob<Guid, In>
    {
        return SessionCore<Job, In, IdFactGuid, Guid>.New(rootDirectory, new(), jobResults.UnwrapOr(new HashSet<string>()));
    }
    // id: guid & input: FilesInput
    /// <summary>
    /// Tries to create:
    /// <code>Session&lt;Job, FilesInput, IdFactGuid, Guid></code>
    /// 
    /// where:
    /// <list type="bullet">
    /// <item>the keys are of type Guid,</item> 
    /// <item>key factory for auto-generated keys is <see cref="IdFactGuid"/>, and</item>
    /// <item>input is a collection of files, as <see cref="FilesInput"/>.</item>
    /// </list>
    /// 
    /// Returns Ok of the session if succeeds, the error if the constructor fails.
    /// </summary>
    /// <typeparam name="Job">Type of the job.</typeparam>
    /// <param name="rootDirectory">Root directory of jobs of the session.</param>
    /// <param name="jobResults">Names of results of jobs.</param>
    public static Res<SessionCore<Job, FilesInput, IdFactGuid, Guid>> NewWithGuidFilesInput<Job>(string rootDirectory, Opt<HashSet<string>> jobResults = default)
        where Job : IJob<Guid, FilesInput>
    {
        return SessionCore<Job, FilesInput, IdFactGuid, Guid>.New(rootDirectory, new(), jobResults.UnwrapOr(new HashSet<string>()));
    }
}
