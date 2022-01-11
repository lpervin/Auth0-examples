using CFSWeb.Model.Request;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CFSWeb.Data.Extensions
{
    public static class IQueryableExtensions
    {


        public static IQueryable<TObjectType> ApplyOrder<TObjectType>(this IQueryable<TObjectType> query, SortInfo orderBy)
        {

            if (orderBy == null || string.IsNullOrEmpty(orderBy.FieldName))
                return query;

            //Get OrderBy Property
            PropertyInfo orderByPropInfo = typeof(TObjectType).GetProperty(orderBy.FieldName);
            //if Property doesnt exists exit
            if (orderByPropInfo == null)
                return query;

            //Order based on different Property Data Type data types
            //var orderPropInstance = default(orderByPropInfo.PropertyType);
            // Type propType = Nullable.GetUnderlyingType(orderByPropInfo.PropertyType);
            /**/
            switch (orderByPropInfo.PropertyType.ToString())
            {
                case "System.String":
                    return query.ApplyOrder<TObjectType, String>(orderByPropInfo, orderBy);
                case "System.Byte":
                    return query.ApplyOrder<TObjectType, Byte>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Byte]":
                    return query.ApplyOrder<TObjectType, Nullable<Byte>>(orderByPropInfo, orderBy);
                case "System.Int16":
                    return query.ApplyOrder<TObjectType, Int16>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Int16]":
                    return query.ApplyOrder<TObjectType, Nullable<Int16>>(orderByPropInfo, orderBy);
                case "System.Int32":
                    return query.ApplyOrder<TObjectType, Int32>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Int32]":
                    return query.ApplyOrder<TObjectType, Nullable<Int32>>(orderByPropInfo, orderBy);
                case "System.Int64":
                    return query.ApplyOrder<TObjectType, Int64>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Int64]":
                    return query.ApplyOrder<TObjectType, Nullable<Int64>>(orderByPropInfo, orderBy);
                case "System.Single":
                    return query.ApplyOrder<TObjectType, Single>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Single]":
                    return query.ApplyOrder<TObjectType, Nullable<Single>>(orderByPropInfo, orderBy);
                case "System.Double":
                    return query.ApplyOrder<TObjectType, Double>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Double]":
                    return query.ApplyOrder<TObjectType, Nullable<Double>>(orderByPropInfo, orderBy);
                case "System.Decimal":
                    return query.ApplyOrder<TObjectType, Decimal>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Decimal]":
                    return query.ApplyOrder<TObjectType, Nullable<Decimal>>(orderByPropInfo, orderBy);
                case "System.Boolean":
                    return query.ApplyOrder<TObjectType, Boolean>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.Boolean]":
                    return query.ApplyOrder<TObjectType, Nullable<Boolean>>(orderByPropInfo, orderBy);              
                case "System.DateTimeOffset":
                    return query.ApplyOrder<TObjectType, DateTimeOffset>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.DateTimeOffset]":
                    return query.ApplyOrder<TObjectType, Nullable<DateTimeOffset>>(orderByPropInfo, orderBy);
                case "System.DateTime":
                    return query.ApplyOrder<TObjectType, DateTime>(orderByPropInfo, orderBy);
                case "System.Nullable`1[System.DateTime]":
                    return query.ApplyOrder<TObjectType, Nullable<DateTime>>(orderByPropInfo, orderBy);
                default:
                    throw new NotImplementedException($"Sorting by {orderByPropInfo.PropertyType.ToString()} data type is not supported.");
            }
            //return null;
        }

        public static IQueryable<TObjectType> ApplyOrder<TObjectType, TTargetType>(this IQueryable<TObjectType> query, PropertyInfo orderByPropInfo, SortInfo orderBy)
        {
            //Create left side of Lamda expression o => o.PropName
            ParameterExpression lhsParam = Expression.Parameter(typeof(TObjectType), "o");
            //Create Body (right hand side) of expression o => o.PropName
            Expression orderByExpression = Expression.Property(lhsParam, orderByPropInfo);

            //create final Lamda expression to be used in OrderBy

            var orderByLamda = Expression.Lambda<Func<TObjectType, TTargetType>>(orderByExpression, lhsParam);

            //Finally Apply Order to original Query
            switch (orderBy.Order)
            {
                case SortOrder.Asc:
                    return query.OrderBy(orderByLamda);
                case SortOrder.Desc:
                    return query.OrderByDescending(orderByLamda);
                default:
                    return query;
            }

            // return null;
        }

        //public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, SortInfo orderBy, Dictionary<string, Expression<Func<T, object>>> columnsMap)
        //{
        //    if (orderBy == null || string.IsNullOrEmpty(orderBy.FieldName))
        //        return query;

        //    if (!columnsMap.ContainsKey(orderBy.FieldName))
        //        return query;


        //    switch (orderBy.Order)
        //    {
        //        case SortOrder.Asc:
        //            return query.OrderBy(columnsMap[orderBy.FieldName]);
        //        case SortOrder.Desc:
        //            return query.OrderByDescending(columnsMap[orderBy.FieldName]);
        //        default:
        //            return query;
        //    }
        //}

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PageInfo pageInfo)
        {
            if (pageInfo.PageSize == 0)
                return query;

            return query.Skip((pageInfo.PageIndex - 1) * pageInfo.PageSize)
            .Take(pageInfo.PageSize);
        }        
    }
}
