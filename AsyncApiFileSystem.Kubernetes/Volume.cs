namespace AsyncApiFileSystem.Kubernetes;

public class Volume
{
    // required
    public readonly string Name;
    public readonly IVolume Definition;

    // ctor
    public Volume(string name, IVolume definition)
        => (Name, Definition) = (name, definition);


    // write
    internal void Write(StringBuilder stringBuilder)
    {
        stringBuilder.Append("        - name: ").AppendLine(Name);
        Definition.Write(stringBuilder);
    }
}
