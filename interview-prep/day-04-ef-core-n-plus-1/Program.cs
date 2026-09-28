using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NPlusOneDemo;

// One in-memory SQLite database shared by every context in this demo.
using var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

var counter = new QueryCounter();
var options = new DbContextOptionsBuilder<DataContext>()
    .UseSqlite(connection)
    .AddInterceptors(counter)
    .Options;

using (var db = new DataContext(options))
{
    db.Database.EnsureCreated();
    Seed.Run(db, countryCount: 50, statesPerCountry: 3);
}

// 1. BEFORE: one query for the countries, then one more query per country.
using (var db = new DataContext(options))
{
    counter.Reset();
    var rows = new List<CountryRow>();
    foreach (var country in db.Countries.OrderBy(c => c.Id).ToList())
    {
        var states = db.States
            .Where(s => s.CountryId == country.Id)
            .OrderByDescending(s => s.Population)
            .ToList();
        rows.Add(new CountryRow(country.Name, states[0].Name, states.Count));
    }
    Report("Loop, tracked", db, rows);
}

// 2. Same loop, read-only: tracking is gone, but it is still 51 queries.
using (var db = new DataContext(options))
{
    counter.Reset();
    var rows = new List<CountryRow>();
    foreach (var country in db.Countries.AsNoTracking().OrderBy(c => c.Id).ToList())
    {
        var states = db.States
            .AsNoTracking()
            .Where(s => s.CountryId == country.Id)
            .OrderByDescending(s => s.Population)
            .ToList();
        rows.Add(new CountryRow(country.Name, states[0].Name, states.Count));
    }
    Report("Loop, AsNoTracking", db, rows);
}

// 3. Include fixes the round trips, but every entity is still tracked.
using (var db = new DataContext(options))
{
    counter.Reset();
    var rows = db.Countries
        .Include(c => c.States.OrderByDescending(s => s.Population))
        .OrderBy(c => c.Id)
        .ToList()
        .Select(c => new CountryRow(c.Name, c.States[0].Name, c.States.Count))
        .ToList();
    Report("Include, tracked", db, rows);
}

// 4. Same query, read-only: nothing goes into the change tracker.
using (var db = new DataContext(options))
{
    counter.Reset();
    var rows = db.Countries
        .AsNoTracking()
        .Include(c => c.States.OrderByDescending(s => s.Population))
        .OrderBy(c => c.Id)
        .ToList()
        .Select(c => new CountryRow(c.Name, c.States[0].Name, c.States.Count))
        .ToList();
    Report("Include, AsNoTracking", db, rows);
}

// 5. AFTER: project straight into the row the screen needs.
using (var db = new DataContext(options))
{
    counter.Reset();
    var rows = db.Countries
        .OrderBy(c => c.Id)
        .Select(c => new CountryRow(
            c.Name,
            c.States.OrderByDescending(s => s.Population).Select(s => s.Name).First(),
            c.States.Count))
        .ToList();
    Report("Select projection", db, rows);
}

void Report(string label, DataContext db, List<CountryRow> rows) =>
    Console.WriteLine(
        $"{label,-22} queries: {counter.Count,3}   tracked: {db.ChangeTracker.Entries().Count(),3}   " +
        $"rows: {rows.Count}   first: {rows[0].Name} largest: {rows[0].LargestState} ({rows[0].StateCount} states)");
