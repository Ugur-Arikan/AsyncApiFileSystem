using Microsoft.AspNetCore.Http;
namespace AsyncFileSystem.Commons;

public class FormOrFsFiles
{
    // data
    readonly Opt<IFormFileCollection?> Form;
    readonly Opt<string[]> Paths;


    // ctor
    public FormOrFsFiles(string[] paths)
    {
        Form = None<IFormFileCollection?>();
        Paths = Some(paths);
    }
    public FormOrFsFiles(IFormFileCollection? form)
    {
        Form = Some(form);
        Paths = None<string[]>();
    }
    public FormOrFsFiles(HttpRequest request)
        : this(request.Form?.Files)
    {
    }


    // method
    public int Count
        => Form.Match(frm => frm == null ? 0 : frm.Count, () => Paths.Unwrap().Length);
    public Res<FormOrFsFiles> Validate(string[] expectedFiles)
    {
        if (Form.IsSome)
        {
            var files = Form.Unwrap();
            int nbFiles = files == null ? 0 : files.Count;
            if (files == null || nbFiles != expectedFiles.Length)
                return Err<FormOrFsFiles>($"{nbFiles} files are provided. However, {expectedFiles.Length} input files are expected: {string.Join(", ", expectedFiles)}.");

            foreach (var file in files)
                if (!expectedFiles.Contains(file.FileName))
                    return Err<FormOrFsFiles>($"Unexpected file '{file.FileName}': {expectedFiles.Length} input files are expected: {string.Join(", ", expectedFiles)}.");

            return Ok(this);
        }
        else
        {
            var files = Paths.Unwrap();
            int nbFiles = files == null ? 0 : files.Length;
            if (files == null || nbFiles != expectedFiles.Length)
                return Err<FormOrFsFiles>($"{nbFiles} files are provided. However, {expectedFiles.Length} input files are expected: {string.Join(", ", expectedFiles)}.");

            foreach (var file in files)
                if (!expectedFiles.Contains(Path.GetFileName(file)))
                    return Err<FormOrFsFiles>($"Unexpected file '{Path.GetFileName(file)}': {expectedFiles.Length} input files are expected: {string.Join(", ", expectedFiles)}.");

            return Ok(this);
        }
    }
    public Res CopyFilesToDir(string targetDirectory)
    {
        if (Form.IsSome)
        {
            var res = Ok();
            try
            {
                var files = Form.Unwrap();
                if (files == null)
                    throw Exc.MustNotReach;
                foreach (var file in files)
                {
                    string targetPath = Path.Join(targetDirectory, file.FileName);
                    using var stream = File.OpenWrite(targetPath);
                    file.CopyTo(stream);
                }
            }
            catch (Exception e)
            {
                res = Err(nameof(CopyFilesToDir), e);
            }
            return res;
        }
        else
        {
            return Ok().Try(() =>
            {
                foreach (var sourcePath in Paths.Unwrap())
                {
                    string targetPath = Path.Join(targetDirectory, Path.GetFileName(sourcePath));
                    File.Copy(sourcePath, targetPath);
                }
            });
        }
    }
    public Res CopyFilesToDirWithNames(string targetDirectory, IEnumerable<string> filenamesToSaveAs)
    {
        var enumFilenames = filenamesToSaveAs.GetEnumerator();
        
        if (Form.IsSome)
        {
            var res = Ok();
            try
            {
                var files = Form.Unwrap();
                if (files == null)
                    throw Exc.MustNotReach;
                foreach (var file in files)
                {
                    enumFilenames.MoveNext();
                    var filename = enumFilenames.Current;

                    string targetPath = Path.Join(targetDirectory, filename);
                    using var stream = File.OpenWrite(targetPath);
                    file.CopyTo(stream);
                }
            }
            catch (Exception e)
            {
                res = Err(nameof(CopyFilesToDir), e);
            }
            return res;
        }
        else
        {
            return Ok().Try(() =>
            {
                foreach (var sourcePath in Paths.Unwrap())
                {
                    enumFilenames.MoveNext();
                    var filename = enumFilenames.Current;

                    string targetPath = Path.Join(targetDirectory, filename);
                    File.Copy(sourcePath, targetPath);
                }
            });
        }
    }
    public Res DeleteFilesFromDir(string jobDirectory)
    {
        if (Form.IsSome)
        {
            var files = Form.Unwrap();
            if (files == null)
                throw Exc.MustNotReach;
            return
                files.Select(f => Path.Join(jobDirectory, f.FileName))
                .Select(p => Ok().Try(() => File.Delete(p))).Reduce(false);
        }
        else
        {
            return
                Paths.Unwrap()
                .Select(origPath => Path.Join(jobDirectory, Path.GetFileName(origPath)))
                .Select(p => Ok().Try(() => File.Delete(p))).Reduce(false);
        }
    }


    public string GetFilename(int index)
    {
        if (Form.IsSome)
        {
            var files = Form.Unwrap();
            if (files == null)
                throw Exc.MustNotReach;
            return files[0].FileName;
        }
        else
            return Path.GetFileName(Paths.Unwrap()[index]);
    }
}
