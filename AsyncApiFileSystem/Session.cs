using AsyncApiFileSystem.Commons;

namespace AsyncApiFileSystem;

public class Session<Job, In> : SessionWrapper<Job, In, IdFactString, string>
    where Job : IJob<string, In>
{
	public Session()
	{

	}
}
