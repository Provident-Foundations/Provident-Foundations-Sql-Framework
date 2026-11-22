using Provident.Foundations.Sql.Schema.DOM.Expressions;

namespace Provident.Foundations.Sql.Schema.DOM.Clauses;

/// <summary>
/// Represents an expression and its sort direction in an ORDER BY clause.
/// </summary>
public sealed class SqlCodeOrderByItem
{
    /// <summary>
    /// Initializes a sort item from an expression.
    /// </summary>
    /// <param name="expression">The expression to sort by.</param>
    /// <param name="descending">Whether to sort in descending order.</param>
    public SqlCodeOrderByItem(SqlCodeExpression expression, bool descending = false)
    {
        ArgumentNullException.ThrowIfNull(expression);
        Expression = expression.Clone();
        Descending = descending;
    }

    /// <summary>Gets the expression to sort by.</summary>
    public SqlCodeExpression Expression { get; }

    /// <summary>Gets whether to sort in descending order.</summary>
    public bool Descending { get; }

    /// <summary>Creates an independent copy of the item.</summary>
    /// <returns>A copy of this item.</returns>
    public SqlCodeOrderByItem Clone() => new(Expression, Descending);
}
