namespace AsyncApiFileSystem.Kubernetes;

public class Container
{
    // required
    public readonly Image Image;

    
    // data - optional
    public Opt<string[]> Commands { get; init; } = default;
    public Opt<VolumeMount[]> VolumeMounts { get; init; } = default;
    public Opt<Resource> ResourceLimits { get; init; } = default;
    public Opt<Resource> ResourceRequests { get; init; } = default;


    // ctor
    public Container(Image image)
    {
        Image = image;
    }
    public Container With(Opt<string[]> commands = default, Opt<Resource> resourceLimits = default, Opt<Resource> resourceRequests = default)
    {
        var newCommands = commands.Match(c => Some(c), Commands);
        var newResourceLimits = resourceLimits.Match(r => Some(r), ResourceLimits);
        var newResourceRequests = resourceRequests.Match(r => Some(r), ResourceRequests);
        return new(Image)
        {
            Commands = newCommands,
            ResourceLimits = newResourceLimits,
            ResourceRequests = newResourceRequests,
            VolumeMounts = VolumeMounts,
        };
    }
}
