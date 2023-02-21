using AsyncApiFileSystem.Kubernetes;
using System.Text;

ProjectedVolume projectedVolumeToken = new(new[] { new ProjectedVolumeSourceServiceAccountToken("pipelines.kubeflow.org", "token") })
{
    DefaultMode = "420"
};
Volume volumeToken = new("volume-kf-pipelines-token", projectedVolumeToken);

PersistentVolumeClaim persistentVolumeClaim = new("datatest");
Volume volumePersistent = new("datatest", persistentVolumeClaim);

Volume[] volumes = new[] { volumeToken, volumePersistent };


VolumeMount mountToken = new("/var/run/secrets/kf-pipelines", "volume-kf-pipelines-token")
{
    ReadOnly = true,
};
VolumeMount mountData = new("/data", "datatest");
VolumeMount[] mounts = new[] { mountToken, mountData };

Resource limits = new()
{
    Memory = Some("8192Mi"),
    Cpu = Some(8.0),
};
Resource requests = new()
{
    Memory = Some("1048Mi"),
    Cpu = Some(0.2),
};

Image image = new("docker.artifactory.dhl.com/srv_morph/morphlingsolver:0.0.3", ImagePullPolicy.Always)
{
    ImagePullSecretsName = Some("artifactory")
};
Container container = new(image)
{
    Commands = Some(new string[] { "/home/jovyan/morph/publinsolver/Solver", "360", "solver" }),
    VolumeMounts = Some(mounts),
    ResourceLimits = Some(limits),
    ResourceRequests = Some(requests),
};


KubernetesJob job = new("solver", container)
{
    RestartPolicy = RestartPolicy.Never,
    ServiceAccountName = "default-editor",
    Volumes = volumes,
};

var sb = new StringBuilder();
job.Write(sb);
Console.WriteLine(sb.ToString());
