namespace AsyncFileSystem;

public interface IRunIdFactory<I>
    where I : IComparable<I>, IEquatable<I>
{
    I ParseId(string directoryName);
    string ToDirectoryName(I id);
    I NewId(HashSet<I> ids);
    Res<I> NewId(string workingDirectory)
    {
        return Ok()
            .TryMap(() => Directory.GetDirectories(workingDirectory).Select(x => ParseId(Path.GetFileName(x))).ToHashSet())
            .Map(ids => NewId(ids));
    }
}
