namespace AsyncApiFileSystem.Kubernetes;

public class VolumeMount
{
    // data
    public readonly string MountPath;
    public readonly string Name;


    // optional
    public bool ReadOnly { get; init; } = false;


    // ctor
    public VolumeMount(string mountPath, string name)
        => (MountPath, Name) = (mountPath, name);


    // write
    internal void Write(StringBuilder stringBuilder)
    {
        stringBuilder.Append("          - mountPath: ").AppendLine(MountPath);
        stringBuilder.Append("            name: ").AppendLine(Name);
        if (ReadOnly)
            stringBuilder.Append("            readOnly: ").AppendLine(ReadOnly.ToString().ToLower());
    }
}
