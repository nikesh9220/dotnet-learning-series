using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NPlusOneDemo;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<State> States { get; set; } = [];
}

public class State
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public string Name { get; set; } = "";
    public int Population { get; set; }
}

// What the screen actually needs: one flat row per country.
public record CountryRow(string Name, string LargestState, int StateCount);

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
}

// Counts every SQL command that reaches the database.
public class QueryCounter : DbCommandInterceptor
{
    public int Count { get; private set; }

    public void Reset() => Count = 0;

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Count++;
        return result;
    }
}

public static class Seed
{
    public static void Run(DataContext db, int countryCount, int statesPerCountry)
    {
        for (var c = 1; c <= countryCount; c++)
        {
            var country = new Country { Name = $"Country {c:00}" };
            for (var s = 1; s <= statesPerCountry; s++)
            {
                // Deterministic, distinct populations so "largest" is unambiguous.
                country.States.Add(new State { Name = $"State {c:00}-{s}", Population = (c * 7 + s * 13) * 100_000 });
            }
            db.Countries.Add(country);
        }
        db.SaveChanges();
    }
}
