namespace AsyncApiFileSystem.Kubernetes;

public class ProjectedVolume : IVolume
{
    // required
    public IProjectedVolumeSource[] Sources;


    // optional
    public Opt<string> DefaultMode { get; init; } = default;


    // ctor
    public ProjectedVolume(IProjectedVolumeSource[] sources)
        => Sources = sources;


    // write
    void IVolume.Write(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("          projected:");
        DefaultMode.Do(defaultMode =>
        {
            stringBuilder.Append("            defaultMode: ").AppendLine(defaultMode);
        });
        stringBuilder.AppendLine("            sources:");
        foreach (var source in Sources)
            source.Write(stringBuilder);
    }
}
