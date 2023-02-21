namespace AsyncApiFileSystem.Kubernetes;

public class Resource
{
    public Opt<string> Memory { get; init; } = default;
    public Opt<double> Cpu { get; init; } = default;

    // write
    internal void Write(StringBuilder stringBuilder)
    {
        Memory.Do(memory => stringBuilder.Append("            memory: ").AppendLine(memory));
        Cpu.Do(cpu => stringBuilder.Append("            cpu: ").AppendLine('"' + cpu.ToString() + '"'));
    }
}
