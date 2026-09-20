using System.Linq.Expressions;
using LinkedIn.Shared.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Shared.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies "WHERE IsDeleted = 0" to every soft-deletable entity in the model.
    /// Pronia hand-wrote a GlobalQueryFilter per entity; this discovers them
    /// automatically so a new entity can never be accidentally left unfiltered.
    /// </summary>
    public static ModelBuilder ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(property), parameter);

            entityType.SetQueryFilter(filter);
        }

        return modelBuilder;
    }
}
