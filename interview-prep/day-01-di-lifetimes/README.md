# Day 01: DI lifetimes, Scoped inside a Singleton

## The question

What happens when a Singleton takes a Scoped service in its constructor?

## The answer

The Singleton is built once, so it grabs one instance of the Scoped service and keeps it for the life of the app. Every request after that sees the same stale instance. This is called a captive dependency.

It gets worse with things like `DbContext`: one instance ends up shared across requests and threads, which is not safe.

## What the demo shows

Each "request" is one DI scope, just like ASP.NET Core creates one per HTTP request.

```
1) Bug: ReportService is a Singleton, RequestContext is Scoped
   request 1: scope has b16bad69, ReportService sees 00f704c3
   request 2: scope has 4fcb54ac, ReportService sees 00f704c3
   request 3: scope has 8499435f, ReportService sees 00f704c3

2) Fix: ReportService is Scoped too
   request 1: scope has 32634395, ReportService sees 32634395
   request 2: scope has 6cd3bca5, ReportService sees 6cd3bca5
   request 3: scope has 7d3b2ffc, ReportService sees 7d3b2ffc

3) Catch it early: ValidateScopes + ValidateOnBuild
   ... Cannot consume scoped service 'RequestContext' from singleton 'ReportService'.
```

In section 1 the Singleton sees the same ID for all three requests, and that ID doesn't match any of them. It was resolved from the root provider, not from a request scope.

## How to fix it

- Make the consumer Scoped (or Transient), so its lifetime is no longer than its dependencies.
- If it really must be a Singleton, inject `IServiceScopeFactory` and create a scope for each unit of work.
- Turn on `ValidateScopes` and `ValidateOnBuild`. ASP.NET Core does this in Development, so the app fails at startup instead of in production.

Rule of thumb: a service should never depend on something that lives shorter than it does.

## Run it

```
dotnet run
```
