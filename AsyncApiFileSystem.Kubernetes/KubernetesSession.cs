using AsyncApiFileSystem.Commons;

namespace AsyncApiFileSystem.Kubernetes;

public class KubernetesSession
{
    // ctor
    public static Res<Session<KubernetesJob<In>, In, IdFactString, string>> NewWithStringIdFilesInput<In>(string rootDirectory)
    {
        return Session<KubernetesJob<In>, In, IdFactString, string>.New(rootDirectory, new(), new HashSet<string>());
    }
}
