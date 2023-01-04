namespace Examples;

/// <summary>
/// This data pretends to contain all required inputs for the long running job.
/// </summary>
/// <param name="Network">A network of customer locations.</param>
/// <param name="Capacities">Capacities of flights.</param>
/// <param name="Costs">Unit costs of using each arc.</param>
public record Input(int InputId, Network Network, Capacities Capacities, Costs Costs);
/// <summary>
/// Network of customer locations.
/// </summary>
public record Network;
/// <summary>
/// Capacities of flights.
/// </summary>
public record Capacities;
/// <summary>
/// Unit costs of using each arc.
/// </summary>
public record Costs;
