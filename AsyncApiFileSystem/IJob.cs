namespace AsyncApiFileSystem;

/// <summary>
/// A long running job request with key of type <typeparamref name="K"/> and input of type <typeparamref name="I"/>.
/// </summary>
/// <typeparam name="K">Type of key of the job.</typeparam>
/// <typeparam name="I">Type of inputs of the job.</typeparam>
public interface IJob<K, I>
{
    /// <summary>
    /// Tries to initiate the job run with the given <paramref name="id"/> and <paramref name="input"/>; and returns back the result.
    /// </summary>
    /// <param name="id">Id (key) of the job to be executed.</param>
    /// <param name="input">Input of the job to be executed.</param>
    /// <returns></returns>
    Res Init(K id, I input);
    /// <summary>
    /// Executes the job with the given <paramref name="id"/> which can write to the provided <paramref name="resultFiles"/>.
    /// </summary>
    /// <param name="id">Id (key) of the job to be executed.</param>
    /// <param name="resultFiles">Dictionary of result files that the job's execution can write to.</param>
    /// <returns></returns>
    Res Run(K id, Dictionary<string, StreamWriter> resultFiles);
}
