namespace AsyncApiFileSystem.Commons;

/// <summary>
/// A string-id factory which auto-generates ids as Guids.
/// </summary>
public class IdFactGuid : IIdFactory<Guid>
{
    /// <summary>
    /// Auto-generates and returns a new unique key.
    /// </summary>
    /// <param name="existingIds">Set of ids that already exist, with an associated execution directory.</param>
    public Guid NewId(HashSet<Guid> existingIds)
    {
        for (int i = 0; i < existingIds.Count + 10; i++)
        {
            Guid key = Guid.NewGuid();
            if (!existingIds.Contains(key))
                return key;
        }
        throw Exc.MustNotReach;
    }
    /// <summary>
    /// Parses the id from the job's execution directory name;
    /// id is equal to the <paramref name="directoryName"/>.
    /// </summary>
    /// <param name="directoryName">Job's execution directory.</param>
    public Guid ParseId(string directoryName)
        => new(directoryName);
    /// <summary>
    /// Creates the job's unique directory name for the given <paramref name="id"/>;
    /// id is equal to the directory name.
    /// </summary>
    /// <param name="id">Id of the job to create directory name for.</param>
    public string ToDirectoryName(Guid id)
        => id.ToString();
}
