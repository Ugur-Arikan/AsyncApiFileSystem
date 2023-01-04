namespace AsyncApiFileSystem;

/// <summary>
/// A factory for run id's (keys) where each id is of type <typeparamref name="Id"/>.
/// </summary>
/// <typeparam name="Id">Type of the id (key).</typeparam>
public interface IIdFactory<Id>
    where Id : IComparable<Id>, IEquatable<Id>
{
    /// <summary>
    /// Parses and returns the id of type <typeparamref name="Id"/> from <paramref name="directoryName"/> of the job's directory.
    /// </summary>
    /// <param name="directoryName">Job's execution directory.</param>
    Id ParseId(string directoryName);

    /// <summary>
    /// Creates the job's unique directory name for the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Id of the job to create directory name for.</param>
    string ToDirectoryName(Id id);

    /// <summary>
    /// Given the set of <paramref name="existingIds"/>, creates and returns a new unique id.
    /// </summary>
    /// <param name="existingIds">Set of ids that already exist, with an associated execution directory.</param>
    Id NewId(HashSet<Id> existingIds);

    /// <summary>
    /// Tries to create a new unique id in the <paramref name="workingDirectory"/>, considering existing directories;
    /// and returns the result.
    /// </summary>
    /// <param name="workingDirectory">Parent directory containing execution directories of existing jobs.</param>
    Res<Id> NewId(string workingDirectory)
    {
        return Ok()
            .TryMap(() => Directory.GetDirectories(workingDirectory).Select(x => ParseId(Path.GetFileName(x))).ToHashSet())
            .Map(ids => NewId(ids));
    }
}
