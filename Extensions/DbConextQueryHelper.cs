using CFSWeb.Data.EfDbContext;
using CFSWeb.Model.Request;
using CFSWeb.Model.Response;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFSWeb.Data.Extensions
{
    public static class DbConextQueryHelper
    {
        public static async Task<PagedResponse<List<TEntity>>> ApplyQueryAsync<TEntity>(DbContext dnConext, QueryResource requestQuery) where TEntity : class
        {
            if (requestQuery == null)
            {
                requestQuery = new QueryResource();
            }
            //Filter By Search Criteria 1st
            var query = dnConext.Set<TEntity>().AsQueryable().ApplySearchQueries(requestQuery.SearchQueries);
            //Total Count of Filtered Items
            var totalCount = await query.CountAsync();
            //Apply Sort
            query = query.ApplyOrder(requestQuery.PagingInfo.OrderBy);
            //Apply Paging
            query = query.ApplyPaging(requestQuery.PagingInfo);

            //Final Results
            var finalResults = await query.ToListAsync();

            return new PagedResponse<List<TEntity>>
            {
                TotalCount = totalCount,
                ResponseData = finalResults,
                PagingInfo = requestQuery.PagingInfo
            };
        }

        //public static async Task<PageResponseWtSummary<List<TEntity>, TSummary>> ApplyQueryWtSummaryAsync<TEntity, TSummary>(DbContext dnConext, QueryResource requestQuery) where TEntity : class
        //{
        //    if (requestQuery == null)
        //    {
        //        requestQuery = new QueryResource();
        //    }
        //    //Filter By Search Criteria 1st
        //    var query = dnConext.Set<TEntity>().AsQueryable().ApplySearchQueries(requestQuery.SearchQueries);
        //    //Total Count of Filtered Items
        //    var totalCount = await query.CountAsync();
        //    var totalSummary = await query.SumAsync(p => p.)
        //    //Apply Sort
        //    query = query.ApplyOrder(requestQuery.PagingInfo.OrderBy);
        //    //Apply Paging
        //    query = query.ApplyPaging(requestQuery.PagingInfo);

        //    //Final Results
        //    var finalResults = await query.ToListAsync();

        //    return new PageResponseWtSummary<List<TEntity>, TSummary>
        //    {
        //        TotalCount = totalCount,
        //        Summary = 
        //        ResponseData = finalResults,
        //        PagingInfo = requestQuery.PagingInfo
        //    };
        //}
    }
}
