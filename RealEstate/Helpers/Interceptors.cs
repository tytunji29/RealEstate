using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace RealEstate.Helpers;

public class UtcDateInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateDateTimes(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateDateTimes(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateDateTimes(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        if (property.CurrentValue is DateTime dateTime)
                        {
                            if (dateTime.Kind == DateTimeKind.Local)
                            {
                                property.CurrentValue = dateTime.ToUniversalTime();
                            }
                        }
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?))
                    {
                        if (property.CurrentValue is DateTime nullableDateTime)
                        {
                            if (nullableDateTime.Kind == DateTimeKind.Local)
                            {
                                property.CurrentValue = nullableDateTime.ToUniversalTime();
                            }
                        }
                    }
                }
            }
        }
    }
}