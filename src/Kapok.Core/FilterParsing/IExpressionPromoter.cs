﻿using System.Linq.Expressions;

namespace Kapok.BusinessLayer.FilterParsing;

/// <summary>
/// Expression promoter is used to promote object or value types
/// to their destination type when an automatic promotion is available
/// such as: int to int?
/// </summary>
public interface IExpressionPromoter
{
    /// <summary>
    /// Promote an expression
    /// </summary>
    /// <param name="expr">Source expression</param>
    /// <param name="type">Destionation data type to promote</param>
    /// <param name="exact">If the match must be exact</param>
    /// <param name="convertExpr">Convert expression</param>
    /// <returns>The promoted <see cref="Expression"/></returns>
    Expression? Promote(Expression expr, Type type, bool exact, bool convertExpr);
}