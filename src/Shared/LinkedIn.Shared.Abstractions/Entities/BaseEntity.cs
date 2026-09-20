namespace LinkedIn.Shared.Abstractions.Entities;

/// <summary>
/// Shared base for all entities across all modules. Pronia duplicated this per
/// module; here it lives once so auditing and soft-delete behave identically
/// everywhere and are applied automatically by EF interceptors.
/// </summary>
public abstract class BaseEntity : IEntity, IAuditable, ISoftDeletable
{
    public long Id { get; set; }

    // IAuditable - populated automatically by AuditingInterceptor
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDeletable - Remove() is rewritten into an UPDATE by SoftDeleteInterceptor
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
