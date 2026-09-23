using Microsoft.Extensions.DependencyInjection;

// 1. The bug: a singleton captures a scoped service.
//    Scope validation is OFF here, which is the default outside Development.
Console.WriteLine("--- Captive dependency (no validation) ---");
using (var root = BuildProvider(validate: false, useFix: false))
{
    for (var request = 1; request <= 3; request++)
    {
        using var scope = root.CreateScope();
        var scopedId = scope.ServiceProvider.GetRequiredService<TenantContext>().Id;
        var seenBySingleton = scope.ServiceProvider.GetRequiredService<ReportCache>().CurrentTenantId();
        Console.WriteLine($"Request {request}: scope has {Short(scopedId)}, singleton sees {Short(seenBySingleton)}");
    }
}

// 2. Same registrations with validation ON (what Development gives you).
Console.WriteLine();
Console.WriteLine("--- Same code with ValidateScopes + ValidateOnBuild ---");
try
{
    using var root = BuildProvider(validate: true, useFix: false);
}
catch (AggregateException ex)
{
    Console.WriteLine(ex.InnerExceptions[0].Message);
}

// 3. The fix: the singleton creates its own scope per unit of work.
Console.WriteLine();
Console.WriteLine("--- Fixed with IServiceScopeFactory ---");
using (var root = BuildProvider(validate: true, useFix: true))
{
    var cache = root.GetRequiredService<SafeReportCache>();
    for (var job = 1; job <= 3; job++)
    {
        var id = await cache.RefreshAsync();
        Console.WriteLine($"Job {job}: fresh scoped instance {Short(id)}");
    }
}

static ServiceProvider BuildProvider(bool validate, bool useFix)
{
    var services = new ServiceCollection();
    services.AddScoped<TenantContext>();

    if (useFix)
        services.AddSingleton<SafeReportCache>();
    else
        services.AddSingleton<ReportCache>();

    return services.BuildServiceProvider(new ServiceProviderOptions
    {
        ValidateScopes = validate,
        ValidateOnBuild = validate
    });
}

static string Short(Guid id) => id.ToString()[..8];

// Scoped: should be one instance per request (think DbContext or current tenant).
public sealed class TenantContext
{
    public Guid Id { get; } = Guid.NewGuid();
}

// BAD: the singleton holds one TenantContext forever.
public sealed class ReportCache(TenantContext tenant)
{
    public Guid CurrentTenantId() => tenant.Id;
}

// GOOD: resolve scoped services inside a scope you own, and dispose it.
public sealed class SafeReportCache(IServiceScopeFactory scopeFactory)
{
    public async Task<Guid> RefreshAsync()
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var tenant = scope.ServiceProvider.GetRequiredService<TenantContext>();
        return tenant.Id;
    }
}