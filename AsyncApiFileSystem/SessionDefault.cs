using AsyncApiFileSystem.Commons;

namespace AsyncApiFileSystem;

/// <summary>
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
public class SessionDefault<Job, In> : SessionWrapper<Job, In, IdFactString, string>
    where Job : IJob<string, In>
{
    SessionDefault(ISession<Job, In, IdFactString, string> session) : base(session) { }
    internal static Res<SessionDefault<Job, In>> New(string rootDirectory, Opt<HashSet<string>> jobResults)
    {
        return
            SessionCore<Job, In, IdFactString, string>.New(rootDirectory, new(), jobResults.UnwrapOr(new HashSet<string>()))
            .Map(session => new SessionDefault<Job, In>(session));
    }
}
