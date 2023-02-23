using AsyncApiFileSystem.Commons;

namespace AsyncApiFileSystem;

/// <summary>
/// Static class for creating session variants.
/// </summary>
public static class Session
{
    /// <summary>
    /// Constructs and returns the session if succeeds; the error if the constructor fails.
    /// 
    /// <inheritdoc cref="SessionCore{Job, In, IdFact, Id}"/>
    /// 
    /// <para>
    /// Default session generic over input <typeparamref name="In"/> and job <typeparamref name="Job"/>, while having:
    /// </para>
    /// 
    /// <list type="bullet">
    /// <item>Id = string</item>
    /// <item>IdFact = <see cref="IdFactString"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="Job">Type of the job.</typeparam>
    /// <typeparam name="In">Type of inputs of jobs.</typeparam>
    /// <param name="rootDirectory">Root directory of jobs of the session.</param>
    /// <param name="jobResults">Names of results of jobs.</param>
    public static Res<SessionDefault<Job, In>> Default<Job, In>(string rootDirectory, Opt<HashSet<string>> jobResults = default)
        where Job : IJob<string, In>
        => SessionDefault<Job, In>.New(rootDirectory, jobResults);
}
