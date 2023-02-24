using AsyncApiFileSystem;

namespace Examples;

public class OptimizationSession : SessionDefault<OptimizationJob, Input>
{
	public OptimizationSession() : base(GetUnderlyingSession())
	{
	}
	static SessionDefault<OptimizationJob, Input> GetUnderlyingSession()
		=> NewSession.Default<OptimizationJob, Input>(ConfigJob.RootDirectory, ConfigJob.ResultFiles).Unwrap();
}
