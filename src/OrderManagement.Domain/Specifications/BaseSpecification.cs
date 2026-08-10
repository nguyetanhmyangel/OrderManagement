using System.Linq.Expressions;

namespace OrderManagement.Domain.Specifications;

public abstract class BaseSpecification<T>(Expression<Func<T, bool>> criteria) : ISpecification.ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; } = criteria;
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    // Builder methods — trả về this để chaining
    protected void AddInclude(Expression<Func<T, object>> includeExpr)
        => Includes.Add(includeExpr);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpr)
        => OrderBy = orderByExpr;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByExpr)
        => OrderByDescending = orderByExpr;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}

