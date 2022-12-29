namespace AsyncFileSystem;

public interface IJob<K, I>
{
    Res Init(K id, I input);
    Res Run(K id, Dictionary<string, StreamWriter> resultFiles);
}
