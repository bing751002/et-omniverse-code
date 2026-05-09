namespace ETOmniverse.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

public static class ModelBuilderNamingExtensions
{
    public static ModelBuilder ApplyEtOmniversePluralTableNames(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                entity.SetTableName(Pluralize(tableName));
            }
        }

        return modelBuilder;
    }

    private static string Pluralize(string tableName)
    {
        if (tableName.EndsWith('s'))
        {
            return tableName;
        }

        if (tableName.EndsWith('y') && tableName.Length > 1 && !"aeiou".Contains(tableName[^2]))
        {
            return tableName[..^1] + "ies";
        }

        return tableName + "s";
    }
}
