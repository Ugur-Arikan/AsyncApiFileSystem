namespace Examples;

/// <summary>
/// This static class pretends to be an input repository or a database;
/// which provides inputs for given id's.
/// </summary>
public static class InputsRepo
{
    static InputsRepo()
    {
        _inputs = new();
        for (int i = 0; i < 10; i++)
            _inputs.Add(i, new Input(i, new Network(), new Capacities(), new Costs()));
    }


    // data
    static Dictionary<int, Input> _inputs;


    // method
    /// <summary>
    /// Returns Some(input) if the input with the given <paramref name="id"/> exists; None otherwise.
    /// </summary>
    /// <param name="id">Id of the input to query.</param>
    public static Opt<Input> Get(int id)
        => _inputs.GetOpt(id);
}
