using Microsoft.AspNetCore.Http;
namespace AsyncApiFileSystem.Commons;

/// <summary>
/// Collection of files which can either be from a web-form or file-system.
/// </summary>
public class FormOrFileSystemFiles
{
    // data
    readonly Opt<IFormFileCollection?> Form;
    readonly Opt<string[]> Paths;


    // ctor
    /// <summary>
    /// Constructs the files object as file-system files with the given <paramref name="paths"/>.
    /// </summary>
    /// <param name="paths">Paths of the files in the file system.</param>
    public FormOrFileSystemFiles(string[] paths)
    {
        Form = None<IFormFileCollection?>();
        Paths = Some(paths);
    }
    /// <summary>
    /// Constructs the files object from the file-collection of a web-form.
    /// </summary>
    /// <param name="formFileCollection">File collection of the form.</param>
    public FormOrFileSystemFiles(IFormFileCollection? formFileCollection)
    {
        Form = Some(formFileCollection);
        Paths = None<string[]>();
    }
    /// <summary>
    /// onstructs the files object from the file-collection of the web-form of the <paramref name="request"/>.
    /// </summary>
    /// <param name="request">Request to create the files for.</param>
    public FormOrFileSystemFiles(HttpRequest request)
        : this(request.Form?.Files)
    {
    }


    // method
    /// <summary>
    /// Number of files.
    /// </summary>
    public int Count
        => Form.Match(frm => frm == null ? 0 : frm.Count, () => Paths.Unwrap().Length);
    /// <summary>
    /// Validates the files-object by checking whether all <paramref name="expectedFileNames"/> exist or not.
    /// Returns the Err if validation fails; Ok of itself otherwise.
    /// </summary>
    /// <param name="expectedFileNames">Collection of expected file names.</param>
    /// <returns></returns>
    public Res<FormOrFileSystemFiles> Validate(string[] expectedFileNames)
    {
        if (Form.IsSome)
        {
            var files = Form.Unwrap();
            int nbFiles = files == null ? 0 : files.Count;
            if (files == null || nbFiles != expectedFileNames.Length)
                return Err<FormOrFileSystemFiles>($"{nbFiles} files are provided. However, {expectedFileNames.Length} input files are expected: {string.Join(", ", expectedFileNames)}.");

            foreach (var file in files)
                if (!expectedFileNames.Contains(file.FileName))
                    return Err<FormOrFileSystemFiles>($"Unexpected file '{file.FileName}': {expectedFileNames.Length} input files are expected: {string.Join(", ", expectedFileNames)}.");

            return Ok(this);
        }
        else
        {
            var files = Paths.Unwrap();
            int nbFiles = files == null ? 0 : files.Length;
            if (files == null || nbFiles != expectedFileNames.Length)
                return Err<FormOrFileSystemFiles>($"{nbFiles} files are provided. However, {expectedFileNames.Length} input files are expected: {string.Join(", ", expectedFileNames)}.");

            foreach (var file in files)
                if (!expectedFileNames.Contains(Path.GetFileName(file)))
                    return Err<FormOrFileSystemFiles>($"Unexpected file '{Path.GetFileName(file)}': {expectedFileNames.Length} input files are expected: {string.Join(", ", expectedFileNames)}.");

            return Ok(this);
        }
    }
    /// <summary>
    /// Tries to copy all files to the <paramref name="targetDirectory"/> and returns the result.
    /// </summary>
    /// <param name="targetDirectory">Target directory to copy the files to.</param>
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
    /// <summary>
    /// Tries to copy all files to the <paramref name="targetDirectory"/> and returns the result.
    /// </summary>
    /// <param name="targetDirectory">Target directory to copy the files to.</param>
    /// <param name="filenamesToSaveAs">Filenames to overwrite the names of the copied files.</param>
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
    /// <summary>
    /// Tries to delete files from the <paramref name="targetDirectory"/> and returns the result.
    /// </summary>
    /// <param name="targetDirectory">Target directory to delete the files from.</param>
    /// <returns></returns>
    public Res DeleteFilesFromDir(string targetDirectory)
    {
        if (Form.IsSome)
        {
            var files = Form.Unwrap();
            if (files == null)
                throw Exc.MustNotReach;
            return
                files.Select(f => Path.Join(targetDirectory, f.FileName))
                .Select(p => Ok().Try(() => File.Delete(p))).Reduce(false);
        }
        else
        {
            return
                Paths.Unwrap()
                .Select(origPath => Path.Join(targetDirectory, Path.GetFileName(origPath)))
                .Select(p => Ok().Try(() => File.Delete(p))).Reduce(false);
        }
    }

    /// <summary>
    /// Returns the name of the file with the given index.
    /// </summary>
    /// <param name="index">Index of the file.</param>
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
