namespace AsyncApiFileSystem.Kubernetes;

/// <summary>
/// Image pull policy.
/// </summary>
public enum ImagePullPolicy
{
    /// <summary>
    /// The image is pulled only if it is not already present locally.
    /// </summary>
    IfNotPresent,
    /// <summary>
    /// Every time the kubelet launches a container, the kubelet queries the container image registry to resolve the name to an image digest. If the kubelet has a container image with that exact digest cached locally, the kubelet uses its cached image; otherwise, the kubelet pulls the image with the resolved digest, and uses that image to launch the container.
    /// </summary>
    Always,
    /// <summary>
    /// The kubelet does not try fetching the image. If the image is somehow already present locally, the kubelet attempts to start the container; otherwise, startup fails.
    /// </summary>
    Never,
}


/// <summary>
/// A container image for a kubernetes-job that will be defined as the <see cref="AsyncApiFileSystem"/> session's job.
/// </summary>
/// <param name="Name">Image name.</param>
/// <param name="ImagePullPolicy">Image pull policy.</param>
public class Image
{
    // required
    public readonly string Name;
    public readonly ImagePullPolicy ImagePullPolicy;


    // optional
    /// <summary>
    /// Name of imagePullSecrets if any.
    /// Will be skipped by default.
    /// </summary>
    public Opt<string> ImagePullSecretsName { get; set; } = default;


    // ctor
    public Image(string name, ImagePullPolicy imagePullPolicy)
    {
        Name = name;
        ImagePullPolicy = imagePullPolicy;
    }
}
