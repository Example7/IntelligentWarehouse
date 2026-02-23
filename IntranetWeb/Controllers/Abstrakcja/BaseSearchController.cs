using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IntranetWeb.Controllers.Abstrakcja
{
    public abstract class BaseSearchController<T> : Controller where T : class
    {
        protected readonly Data.Data.DataContext _context;

        protected BaseSearchController(Data.Data.DataContext context)
        {
            _context = context;
        }

        protected IQueryable<T> ApplySearch(
            IQueryable<T> query,
            string? searchTerm,
            Expression<Func<T, string?>> field)
            => ApplySearchAny(query, searchTerm, field);

        protected IQueryable<T> ApplySearchAny(
            IQueryable<T> query,
            string? searchTerm,
            params Expression<Func<T, string?>>[] fields)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || fields.Length == 0)
                return query;

            var pattern = $"%{searchTerm.Trim()}%";
            var param = Expression.Parameter(typeof(T), "e");

            Expression? bodyOr = null;

            var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) }
            )!;

            var efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));

            foreach (var f in fields)
            {
                var replacedBody = new ReplaceParameterVisitor(f.Parameters[0], param).Visit(f.Body)!;

                var coalesce = Expression.Coalesce(replacedBody, Expression.Constant(""));

                var likeCall = Expression.Call(likeMethod, efFunctions, coalesce, Expression.Constant(pattern));

                bodyOr = bodyOr == null ? likeCall : Expression.OrElse(bodyOr, likeCall);
            }

            var predicate = Expression.Lambda<Func<T, bool>>(bodyOr!, param);
            return query.Where(predicate);
        }

        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _from;
            private readonly ParameterExpression _to;

            public ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to)
            {
                _from = from;
                _to = to;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _from ? _to : base.VisitParameter(node);

            protected override Expression VisitLambda<TDelegate>(Expression<TDelegate> node)
                => Expression.Lambda(Visit(node.Body), _to);
        }
    }
}
