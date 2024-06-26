﻿using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.BusinessLayer;

public static class EntityServiceExtension
{
    public static T GetByKey<T>(this IEntityReadOnlyService<T> @this, params object?[] keyValues)
        where T : class, new()
    {
        var entity = FindByKey(@this, keyValues);
        if (entity != null)
            return entity;

        var primaryKeyValues = new Dictionary<PropertyInfo, object?>();

        for (int i = 0; i < keyValues.Length; i++)
        {
#pragma warning disable CS8602
            var propertyInfo = @this.Model.PrimaryKeyProperties[i];
#pragma warning restore CS8602
            var keyValue = keyValues[i];

            primaryKeyValues.Add(propertyInfo, keyValue);
        }

        throw new EntityNotFoundByKeyException(typeof(T),  primaryKeyValues);
    }

    public static T? FindByKey<T>(this IEntityReadOnlyService<T> @this, params object?[] keyValues)
        where T : class, new()
    {
        return InternalFindByKey(@this, forUpdate: false, keyValues: keyValues);
    }

    public static T? FindByKeyForUpdate<T>(this IEntityService<T> @this, params object?[] keyValues)
        where T : class, new()
    {
        return InternalFindByKey(@this, forUpdate: true, keyValues: keyValues);
    }

    private static T? InternalFindByKey<T>(IEntityReadOnlyService<T> @this, bool forUpdate, params object?[] keyValues)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(@this, nameof(@this));
        if (@this.Model.PrimaryKeyProperties == null || @this.Model.PrimaryKeyProperties.Length == 0)
            throw new EntityNoPrimaryKeyException(typeof(T), string.Format("You can not use method {0} with this entity. It has no primary key", nameof(FindByKey)));
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));
        if (keyValues.Length == 0)
            throw new ArgumentException("No primary key value was given", nameof(keyValues));
        if (@this.Model.PrimaryKeyProperties.Length != keyValues.Length)
            throw new NotSupportedException($"The type {typeof(T).FullName} has {@this.Model.PrimaryKeyProperties.Length} primary key properties, {keyValues.Length} where given");
        if (@this.DataDomainScope == null)
            throw new ArgumentException("The entity service has the DataDomainScope not initialized.", nameof(@this));

        var param = Expression.Parameter(typeof(T));
        Expression? whereExpression = null;

        for (int i = 0; i < keyValues.Length; i++)
        {
            var propertyInfo = @this.Model.PrimaryKeyProperties[i];
            var keyValue = keyValues[i];

            // NOTE: we don't do any type matching here
            var expr = Expression.Equal(Expression.Property(param, propertyInfo), Expression.Constant(keyValue));
            if (whereExpression == null)
                whereExpression = expr;
            else
                whereExpression = Expression.And(whereExpression, expr);
        }

        var query = forUpdate ? ((IEntityService<T>)@this).AsQueryableForUpdate() : @this.AsQueryable();

        int primaryKeyPropertyId = 0;

        foreach (var dataPartition in @this.DataDomainScope.DataPartitions.Values)
        {
            if (dataPartition.InterfaceType.IsAssignableFrom(typeof(T)) &&
                // We only do the data scope assignment when the primary key column has the same name as the column defined in the interface
                // and when they appear in the same order as they are registered to the DataDomain
                @this.Model.PrimaryKeyProperties[primaryKeyPropertyId].Name == dataPartition.PartitionProperty.Name)
            {
                var modifier = new FilterExpressionModifier(FilterExpressionModifierAction.SetFilterValue,
                    typeof(T), dataPartition.PartitionProperty);
                modifier.ParameterValue = keyValues[primaryKeyPropertyId];

                query = query.Provider.CreateQuery<T>(
#pragma warning disable CS8604
                    modifier.Visit(query.Expression)
#pragma warning restore CS8604
                );

                primaryKeyPropertyId++;

                // makes sure that we don't run over the @this.Model.PrimaryKeyProperties array length
                if (@this.Model.PrimaryKeyProperties.Length == primaryKeyPropertyId)
                    break;
            }
        }

#pragma warning disable CS8604
        return query.Where(Expression.Lambda<Func<T, bool>>(whereExpression, param)).FirstOrDefault();
#pragma warning restore CS8604
    }

    public static async Task<T> GetByKeyAsync<T>(this IEntityReadOnlyService<T> @this, params object?[] keyValues)
        where T : class, new()
    {
        var entity = await FindByKeyAsync(@this, keyValues);
        if (entity == null)
        {
            var primaryKeyValues = new Dictionary<PropertyInfo, object?>();

            for (int i = 0; i < keyValues.Length; i++)
            {
#pragma warning disable CS8602
                var propertyInfo = @this.Model.PrimaryKeyProperties[i];
#pragma warning restore CS8602
                var keyValue = keyValues[i];

                primaryKeyValues.Add(propertyInfo, keyValue);
            }

            throw new EntityNotFoundByKeyException(typeof(T),  primaryKeyValues);
        }

        return entity;
    }

    public static Task<T?> FindByKeyAsync<T>(this IEntityReadOnlyService<T> @this, params object?[] keyValues)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(@this, nameof(@this));
        if (@this.Model.PrimaryKeyProperties == null || @this.Model.PrimaryKeyProperties.Length == 0)
            throw new EntityNoPrimaryKeyException(typeof(T), string.Format("You can not use method {0} with this entity.", nameof(FindByKey)));
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));
        if (keyValues.Length == 0)
            throw new ArgumentException("No primary key values where given", nameof(keyValues));
        if (@this.Model.PrimaryKeyProperties.Length != keyValues.Length)
            throw new NotSupportedException($"The type {typeof(T).FullName} has {@this.Model.PrimaryKeyProperties.Length} primary key properties, {keyValues.Length} where given");
        if (@this.DataDomainScope == null)
            throw new ArgumentException("The entity service has the DataDomainScope not initialized.", nameof(@this));

        var param = Expression.Parameter(typeof(T));
        Expression? whereExpression = null;

        for (int i = 0; i < keyValues.Length; i++)
        {
            var propertyInfo = @this.Model.PrimaryKeyProperties[i];
            var keyValue = keyValues[i];

            // NOTE: we don't do any type matching here
            var expr = Expression.Equal(Expression.Property(param, propertyInfo), Expression.Constant(keyValue));
            whereExpression = whereExpression == null
                ? expr
                : Expression.And(whereExpression, expr);
        }

        var query = @this.AsQueryable();

        int primaryKeyPropertyId = 0;

        foreach (var dataPartition in @this.DataDomainScope.DataPartitions.Values)
        {
            if (dataPartition.InterfaceType.IsAssignableFrom(typeof(T)) &&
                // We only do the data scope assignment when the primary key column has the same name as the column defined in the interface
                // and when they appear in the same order as they are registered to the DataDomain
                @this.Model.PrimaryKeyProperties[primaryKeyPropertyId].Name == dataPartition.PartitionProperty.Name)
            {
                var modifier = new FilterExpressionModifier(FilterExpressionModifierAction.SetFilterValue,
                    typeof(T), dataPartition.PartitionProperty);
                modifier.ParameterValue = keyValues[primaryKeyPropertyId];

                query = query.Provider.CreateQuery<T>(
#pragma warning disable CS8604
                    modifier.Visit(query.Expression)
#pragma warning restore CS8604
                );

                primaryKeyPropertyId++;

                // makes sure that we don't run over the @this.Model.PrimaryKeyProperties array length
                if (@this.Model.PrimaryKeyProperties.Length == primaryKeyPropertyId + 1)
                    break;
            }
        }

#pragma warning disable CS8604
        return Task.FromResult(
            query.Where(Expression.Lambda<Func<T, bool>>(whereExpression, param)).FirstOrDefault()
        );
#pragma warning restore CS8604
    }
}