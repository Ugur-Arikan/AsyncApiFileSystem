namespace AsyncApiFileSystem.Kubernetes;

public class PersistentVolumeClaim : IVolume
{
    // required
    public readonly string ClaimName;


    // ctor
    public PersistentVolumeClaim(string claimName)
        => ClaimName = claimName;


    // write
    void IVolume.Write(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("          persistentVolumeClaim:");
        stringBuilder.Append("            claimName: ").AppendLine(ClaimName);
    }
}
