namespace AsyncFileSystem.Commons;

public class RunIdFactoryString : IRunIdFactory<string>
{
    public string NewId(HashSet<string> ids)
    {
        for (int i = 0; i < ids.Count + 1; i++)
        {
            string id = i.ToString();
            if (!ids.Contains(id))
                return id;
        }
        throw Exc.MustNotReach;
    }
    public string ParseId(string directoryName)
        => directoryName;
    public string ToDirectoryName(string id)
        => id;
}
