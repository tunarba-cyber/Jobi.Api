namespace LinkedIn.Shared.Abstractions.Entities;

/// <summary>Marker for anything persisted with a surrogate key.</summary>
public interface IEntity
{
    long Id { get; }
}

/// <summary>Entities that track who created/modified them and when.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? ModifiedAtUtc { get; set; }
    string? ModifiedBy { get; set; }
}

/// <summary>Entities that are never physically deleted.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAtUtc { get; set; }
    string? DeletedBy { get; set; }
}
