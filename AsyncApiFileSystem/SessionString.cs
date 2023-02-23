using AsyncApiFileSystem.Commons;

namespace AsyncApiFileSystem;

public class SessionString<Job, In> //: ISession<Job, In, IdFactString, string>
    where Job : IJob<string, In>
{
}
