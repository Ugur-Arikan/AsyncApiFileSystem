namespace AsyncApiFileSystem.Kubernetes;

public interface IProjectedVolumeSource
{
    void Write(StringBuilder stringBuilder);

    public static ProjectedVolumeSourceServiceAccountToken SourceServiceAccountToken(string audience, string path)
        => new(audience, path);
}

//public record KeyPath(string Key, string Path);

//public class ProjectedVolumeSourceSecret : IProjectedVolumeSource
//{
//    // required
//    public readonly string Name;
//    public readonly KeyPath[] Items;


//    // ctor
//    public ProjectedVolumeSourceSecret(string name, KeyPath[] items)
//        => (Name, Items) = (name, items);
//}

//public class ProjectedVolumeSourceConfigMap : IProjectedVolumeSource
//{
//    // required
//    public readonly string Name;
//    public readonly KeyPath[] Items;

//    // ctor
//    public ProjectedVolumeSourceConfigMap(string name, KeyPath[] items)
//        => (Name, Items) = (name, items);
//}

public class ProjectedVolumeSourceServiceAccountToken : IProjectedVolumeSource
{
    // required
    public readonly string Audience;
    public readonly string Path;

    // optional
    public Opt<int> ExpirationSeconds { get; init; } = default;


    // ctor
    public ProjectedVolumeSourceServiceAccountToken(string audience, string path)
        => (Audience, Path) = (audience, path);


    // write
    void IProjectedVolumeSource.Write(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("              - serviceAccountToken:");
        stringBuilder.Append("                  audience: ").AppendLine(Audience);
        stringBuilder.Append("                  path: ").AppendLine(Path);
    }
}
