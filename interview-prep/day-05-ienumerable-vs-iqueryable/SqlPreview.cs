using System.Linq.Expressions;

namespace Day05;

// A tiny translator for simple Where predicates, only to show the idea:
// an Expression is data, so a provider (EF Core, for example) can read it and write SQL.
// A Func<T, bool> is compiled code. Nobody can turn it into SQL.
public static class SqlPreview
{
    public static string Where<T>(Expression<Func<T, bool>> predicate) =>
        $"SELECT * FROM {typeof(T).Name}s WHERE {Render(predicate.Body)}";

    private static string Render(Expression e) => e switch
    {
        BinaryExpression b => $"{Render(b.Left)} {Op(b.NodeType)} {Render(b.Right)}",
        MemberExpression m when m.Expression is ParameterExpression => m.Member.Name,
        ConstantExpression c => c.Value is string s ? $"'{s}'" : $"{c.Value}",
        _ => throw new NotSupportedException($"Cannot translate: {e}")
    };

    private static string Op(ExpressionType t) => t switch
    {
        ExpressionType.GreaterThan => ">",
        ExpressionType.GreaterThanOrEqual => ">=",
        ExpressionType.LessThan => "<",
        ExpressionType.Equal => "=",
        ExpressionType.AndAlso => "AND",
        ExpressionType.OrElse => "OR",
        _ => throw new NotSupportedException(t.ToString())
    };
}