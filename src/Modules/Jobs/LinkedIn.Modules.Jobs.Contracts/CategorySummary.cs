namespace LinkedIn.Modules.Jobs.Contracts;

/// <summary>
/// The ONLY category shape other modules are allowed to consume. Internal entities
/// stay internal, so the Jobs module can refactor freely without breaking others.
/// This is the boundary the Pronia template was missing entirely.
/// </summary>
public sealed record CategorySummary(long Id, string Name, string Slug);
