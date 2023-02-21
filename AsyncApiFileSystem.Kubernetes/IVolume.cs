namespace AsyncApiFileSystem.Kubernetes;

public interface IVolume
{
    internal void Write(StringBuilder stringBuilder);

    public static ProjectedVolume Projected(IProjectedVolumeSource[] sources)
        => new(sources);
    public static PersistentVolumeClaim Persistent(string claimName)
        => new(claimName);
}
