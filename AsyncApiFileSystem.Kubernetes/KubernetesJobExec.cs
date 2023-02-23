using System.Diagnostics;

namespace AsyncApiFileSystem.Kubernetes;

internal class KubernetesJobExec<In> : IDisposable
{
    // data
    readonly KubernetesJob<In> Job;
    readonly string Id;
    bool Disposed;
    Opt<Process> MaybeProcess;


    // ctor
    public KubernetesJobExec(KubernetesJob<In> job, string id)
    {
        Job = job;
        Id = id;

        Disposed = false;
        MaybeProcess = None<Process>();
    }
    // dispose
    public void Dispose()
    {
        if (Disposed)
            return;

        if (MaybeProcess.IsSome)
        {
            var pr = MaybeProcess.Unwrap();
            pr.Kill();
            pr.Dispose();
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }


    // method
    internal Res<string> Execute()
    {
        // get dir
        var maybeDir = Job.Session.GetJobDir(Id);
        if (maybeDir.IsErr)
            return maybeDir;
        string dir = maybeDir.Unwrap();

        // write job.yaml
        string pathJobYaml = Path.Join(dir, "kubernetesjob.yaml");
        var resultWrite =
            Job.KubernetesJobBuilder(Id)
            .Try(jobBuilder =>
            {
                StringBuilder sb = new();
                jobBuilder.WriteYaml(sb);
                File.WriteAllText(pathJobYaml, sb.ToString());
            });
        if (resultWrite.IsErr)
            return Err<string>(resultWrite.ToString());

        // fire process
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = $"apply -f {pathJobYaml}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        
        try
        {
            process.WaitForExit();

            var outputLines = new List<string>();
            while (!process.StandardOutput.EndOfStream)
            {
                var output = process.StandardOutput.ReadLine();
                if (output != null)
                    outputLines.Add(output);
            }

            MaybeProcess = None<Process>();
            return Ok(string.Join(Environment.NewLine, outputLines));
        }
        catch (Exception e)
        {
            process.Kill();
            return Err<string>(e.Message + "\n" + e.InnerException?.Message + "\n" + e.StackTrace);
        }
    }
}
