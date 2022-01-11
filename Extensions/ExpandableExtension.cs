using CFSWeb.Model.Request;
using LinqKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CFSWeb.Data.Extensions
{
    public static class ExpandableExtension
    {
        public static IQueryable<T> ApplySearchQueries<T>(this IQueryable<T> query, List<QueryField> searchQueries)
        {
            if (searchQueries == null || !searchQueries.Any())
                return query;

            var expressionTypesMap = GetExpressionsMap();
            if (expressionTypesMap == null)
                return query;

            //All Operation below are evaluated as "AND" operation1==true and operation2==true
            Expression<Func<T, bool>> predicate = PredicateBuilder.New<T>(true); //initializes predicate with And operations
            //loop through Search Queries and create predicates, and append to "And" 
            foreach (var search in searchQueries)
            {
                //If search operation not defined skip this iteration
                if (!expressionTypesMap.ContainsKey(search.SearchOperator))
                    continue;

                /*************************************************************************************************/
                /***code below builds Lambda Expression o => o == contstant or o => o.FieldName.Contains("x")***/
                /*************************************************************************************************/


                /********Left Hand Side (o) of Lamda o => expression**********/
                ParameterExpression lhsLamdaParam = Expression.Parameter(typeof(T));//left hand-side o => prop == contstant
                /******************/                    
                /*******Right Side (expression) of lamda o => expression *******/
                //PropertyInfo of Search Property
                PropertyInfo searchFieldPropertyInfo = typeof(T).GetProperty(search.name);
                if (searchFieldPropertyInfo == null)
                    throw new MissingMemberException($"Property {search.name} is invalid");
                // builds o.Id	of o => o.id {operator} constant
                Expression searchFieldExpression = Expression.Property(lhsLamdaParam, searchFieldPropertyInfo); //Left hand side of prop == contstant
                /*********************/
                
                
                /***********Build Expression Body of o => expression (typical expression = lhsProperty {operator} constant)**********/
                var operationAttribute = expressionTypesMap[search.SearchOperator];
                Expression expressionBody = null;

                if (operationAttribute.IsBinary)
                {
                    expressionBody = CreateBinaryExpression(operationAttribute, searchFieldPropertyInfo.PropertyType, searchFieldExpression, search.value);
                }
                else if (operationAttribute.UseMethod)
                {
                    expressionBody = CreateCallMethodExpression(operationAttribute, searchFieldPropertyInfo.PropertyType, searchFieldExpression, search.value); //Expression.Call(lhsProperty, operationAttribute.Method, null, rhsConstant);
                }
                else
                {
                    throw new InvalidOperationException("Expected either MakeBinary or Call Expression operation");
                }
                

                var theLambda = Expression.Lambda<Func<T, bool>>(expressionBody, lhsLamdaParam);

                /*"And" Operation Only (for Now)*/
                predicate = predicate.And(theLambda);
                /*When ready to support both (and + or)*/
                //if (isGroupOperationAND)
                //{
                //    predicate = predicate.And(theLambda);
                //}
                //else
                //{
                //    predicate = predicate.Or(theLambda);
                //}
            }
            // var finalExpresion = predicate.Compile();
            return query.AsExpandable().Where(predicate);
        }

        /// <summary>
        /// Creates Call Expression: o.property.Contains(constant) or searchValue.Contains(o.property)
        /// </summary>
        /// <param name="operationAttribute"></param>
        /// <param name="propertyType"></param>
        /// <param name="paramProperty"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        private static Expression CreateCallMethodExpression(ExpressionBehavior operationAttribute, Type propertyType, Expression paramProperty, object searchValue)
        {
            //MethodResultCompareValue==true return o => o.Property.Method(searchCriteria)
            if (operationAttribute.MethodResultCompareValue)
            {
                //Constant to be passed in to the Method: property.Contains(rhsConstant)
                Expression rhsConstant = Expression.Constant(ChangeType(searchValue, propertyType));
                return Expression.Call(paramProperty, operationAttribute.Method, null, rhsConstant);
            }

            if (!operationAttribute.Method.Equals("Contains"))
                throw new InvalidOperationException("Expected Contains method for this operation");

            //otherwise build this expression o => searChCriteria.AsQuerable().Method(o.Property)
            

            var searchArray = JsonConvert.DeserializeObject<long[]>(searchValue.ToString()); 
            if (searchArray == null)
                throw new InvalidOperationException("Expected Array of int64 for this operation");
            var queryableSearch = searchArray.AsQueryable<long>();


            var arrContstant = Expression.Constant(queryableSearch, queryableSearch.GetType());
            return Expression.Call
            (
                (
                    ((Expression<Func<bool>>)
                    (() => Queryable.Contains(default(IQueryable<long>), default(long)))
                ).Body as MethodCallExpression).Method,
                arrContstant,
                paramProperty
            );

        }

        /// <summary>
        /// Create the following Binary Expression: o.Property {operator} constant
        /// </summary>
        /// <param name="expressionType"></param>
        /// <param name="propertyType"></param>
        /// <param name="lhsProperty"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        private static Expression CreateBinaryExpression(ExpressionBehavior operationAttribute, Type propertyType, Expression lhsProperty, object searchValue)
        {
            //builds constant expression of o => o.id {operator} constant
            Expression rhsConstant = Expression.Constant(ChangeType(searchValue, propertyType)); //right hand side prop == contstant

            var isNullable = Nullable.GetUnderlyingType(propertyType) != null;
            //Not Nullable type
            if (!isNullable)
            {
                return Expression.MakeBinary(operationAttribute.ExpressionType, lhsProperty, rhsConstant);
            }
            
            /*******Creates expression with not NULL check*******/
                //Builds o.Property !=null
                var nullCheck = Expression.NotEqual(lhsProperty, Expression.Constant(null, typeof(object)));

                //Builds o.Property.Value == constant
                var lhsPropertyValue = Expression.Property(lhsProperty, "Value");
                var rhsCond2ValueEval = Expression.MakeBinary(operationAttribute.ExpressionType, lhsPropertyValue, rhsConstant);
                //Combines 2 Expression above into exp1 && exp2 (o != null && o.value == contstant)
                return Expression.AndAlso(nullCheck, rhsCond2ValueEval);
        }

        private static object ChangeType(object value, Type convertToType)
        {
            var t = convertToType;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }
            return Convert.ChangeType(value, t);
        }

        private static Dictionary<SearchOperator, ExpressionBehavior> _expressionTypeMap;
        private static object _lock = 1;
        internal static Dictionary<SearchOperator, ExpressionBehavior> GetExpressionsMap()
        {
            if (_expressionTypeMap != null && _expressionTypeMap.Keys.Count>0)
                return _expressionTypeMap;

            _expressionTypeMap = new Dictionary<SearchOperator, ExpressionBehavior>();

            try
            {
                lock (_lock)
                {
                    _expressionTypeMap.Clear();
                    //Equals
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.Equals))
                        _expressionTypeMap.Add(SearchOperator.Equals, new ExpressionBehavior { IsBinary = true, ExpressionType = ExpressionType.Equal });
                    //Not Equals
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.NotEqual))
                        _expressionTypeMap.Add(SearchOperator.NotEqual, new ExpressionBehavior { IsBinary = true, ExpressionType = ExpressionType.NotEqual });
                    //Greater than
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.Gt))
                        _expressionTypeMap.Add(SearchOperator.Gt, new ExpressionBehavior { IsBinary = true, ExpressionType = ExpressionType.GreaterThan });
                    //Greater Than or Equals to
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.Gte))
                        _expressionTypeMap.Add(SearchOperator.Gte, new ExpressionBehavior { IsBinary = true, ExpressionType = ExpressionType.GreaterThanOrEqual });
                    //Less Than
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.Lt))
                        _expressionTypeMap.Add(SearchOperator.Lt, new ExpressionBehavior { IsBinary = true, ExpressionType = ExpressionType.LessThan });

                    //Less than or equals to
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.Lte))
                        _expressionTypeMap.Add(SearchOperator.Lte, new ExpressionBehavior { IsBinary = true, ExpressionType = ExpressionType.LessThanOrEqual });

                    //Contains (string compare)
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.Contains))
                        _expressionTypeMap.Add(SearchOperator.Contains, new ExpressionBehavior { IsBinary = false, MethodResultCompareValue = true, ExpressionType = ExpressionType.Call, UseMethod = true, Method = "Contains" });

                    //Contains (int[] in Operator e.g. var searchFor = 1; var long[] x = {1,2,3}; x.Contains(searchFor) )
                    if (!_expressionTypeMap.ContainsKey(SearchOperator.In))
                        _expressionTypeMap.Add(SearchOperator.In, new ExpressionBehavior { IsBinary = false, MethodResultCompareValue = false, ExpressionType = ExpressionType.Call, UseMethod = true, Method = "Contains" });



                    //todo: implement not Equals to, Starts With, End With
                    /*
                      .Add("bw", new ExpressionBehavior { IsBinary = false, MethodResultCompareValue = true, ExpressionType = ExpressionType.Equal, UseMethod = true, Method = "StartsWith" })
                                .Add("bn", new ExpressionBehavior { IsBinary = false, MethodResultCompareValue = false, ExpressionType = ExpressionType.Equal, UseMethod = true, Method = "StartsWith" })
                                .Add("ew", new ExpressionBehavior { IsBinary = false, MethodResultCompareValue = true, ExpressionType = ExpressionType.Equal, UseMethod = true, Method = "EndsWith" })
                                .Add("en", new ExpressionBehavior { IsBinary = false, MethodResultCompareValue = false, ExpressionType = ExpressionType.Equal, UseMethod = true, Method = "EndsWith" })
                     */
                }

            }            
            catch (System.ArgumentException ex)
            {
                if (_expressionTypeMap!=null)
                    _expressionTypeMap.Clear();

                GetExpressionsMap();
            }
            

            return _expressionTypeMap;
        }
    }
}
