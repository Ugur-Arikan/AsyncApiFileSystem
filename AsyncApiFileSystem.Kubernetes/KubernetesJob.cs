using AsyncApiFileSystem.Commons;

namespace AsyncApiFileSystem.Kubernetes;

public enum RestartPolicy
{
    Never,
    OnFailure,
}

public class KubernetesJobBuilder
{
    // required
    public readonly string Name;
    public readonly Container Container;

    
    // optional
    public RestartPolicy RestartPolicy { get; init; } = RestartPolicy.Never;
    public Opt<string> ServiceAccountName { get; init; } = default;
    public Opt<Volume[]> Volumes { get; init; } = default;


    // ctor
    public KubernetesJobBuilder(string name, Container container)
    {
        Name = name;
        Container = container;
    }
    

    // from template
    public KubernetesJobBuilder With(Opt<string> name = default, Opt<string[]> commands = default, Opt<Resource> resourceLimits = default, Opt<Resource> resourceRequests = default)
    {
        Container newContainer = Container.With(commands, resourceLimits, resourceRequests);
        string newName = name.Match(n => n, Name);
        return new(newName, newContainer)
        {
            RestartPolicy = RestartPolicy,
            ServiceAccountName = ServiceAccountName,
            Volumes = Volumes
        };
    }


    // write
    public void WriteYaml(StringBuilder sb)
    {
        sb.AppendLine("apiVersion: batch/v1");
        sb.AppendLine("kind: Job");
        sb.AppendLine("metadata:");
        sb.Append("  name: ").AppendLine(Name);
        sb.AppendLine("spec:");
        sb.AppendLine("  template:");
        sb.AppendLine("    spec:");
        ServiceAccountName.Do(serviceAccountName =>
        {
            sb.Append("      serviceAccountName: ").AppendLine(serviceAccountName);
        });

        sb.AppendLine("      containers:");
        sb.Append("      - name: ").AppendLine(Name);
        sb.Append("        image: ").AppendLine(Container.Image.Name);
        sb.Append("        imagePullPolicy: ").AppendLine(Container.Image.ImagePullPolicy.ToString());
        Container.Commands.Do(commands =>
        {
            string command = '[' + string.Join(", ", commands.Select(cmd => '"' + cmd + '"')) + ']';
            sb.Append("        command: ").AppendLine(command);
        });
        Container.VolumeMounts.Do(volumeMounts =>
        {
            sb.AppendLine("        volumeMounts:");
            foreach (var volumeMount in volumeMounts)
                volumeMount.Write(sb);
        });
        if (Container.ResourceLimits.IsSome || Container.ResourceRequests.IsSome)
            sb.AppendLine("        resources:");
        Container.ResourceLimits.Do(resources =>
        {
            sb.AppendLine("          limits:");
            resources.Write(sb);
        });
        Container.ResourceRequests.Do(resources =>
        {
            sb.AppendLine("          requests:");
            resources.Write(sb);
        });

        Volumes.Do(volumes =>
        {
            sb.AppendLine("      volumes:");
            foreach (var volume in volumes)
                volume.Write(sb);
        });
        sb.Append("      restartPolicy: ").AppendLine(RestartPolicy.ToString());
        Container.Image.ImagePullSecretsName.Do(imagePullSecretsName =>
        {
            sb.AppendLine("      imagePullSecrets:");
            sb.Append("      - name: ").AppendLine(imagePullSecretsName.ToString());
        });
    }
}

public abstract class KubernetesJob<In> : IJob<string, In>
{
    // required
    public readonly Session<KubernetesJob<In>, In, IdFactString, string> Session;
    internal readonly Func<string, Res<KubernetesJobBuilder>> KubernetesJobBuilder;


    // ctor
    public KubernetesJob(Session<KubernetesJob<In>, In, IdFactString, string> session, Func<string, Res<KubernetesJobBuilder>> kubernetesJobBuilder)
    {
        Session = session;
        KubernetesJobBuilder = kubernetesJobBuilder;
    }


    // session
    public abstract Res Init(string id, In input);
    public Res Run(string id, Dictionary<string, StreamWriter> _)
    {
        using var exec = new KubernetesJobExec<In>(this, id);
        var exeRes = exec.Execute();
        exeRes.DoIfErr(err => Console.Error.WriteLine(err));
        return exeRes.WithoutVal();
    }
}
