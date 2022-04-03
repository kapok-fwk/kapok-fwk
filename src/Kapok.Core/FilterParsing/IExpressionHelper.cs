﻿using System.Linq.Expressions;

namespace Kapok.Core.FilterParsing
{
    internal interface IExpressionHelper
    {
        Expression GenerateAdd(Expression left, Expression right);

        Expression GenerateEqual(Expression left, Expression right);

        Expression GenerateGreaterThan(Expression left, Expression right);

        Expression GenerateGreaterThanEqual(Expression left, Expression right);

        Expression GenerateLessThan(Expression left, Expression right);

        Expression GenerateLessThanEqual(Expression left, Expression right);

        Expression GenerateNotEqual(Expression left, Expression right);

        Expression GenerateStringConcat(Expression left, Expression right);

        Expression GenerateSubtract(Expression left, Expression right);

        Expression GenerateLike(Expression left, Expression right);

        Expression GenerateNotLike(Expression left, Expression right);
    }
}
