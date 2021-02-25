using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldCities.Data
{
    public class ApiResult<T>
    {
        // Private constructor called by the CreateAsync method
        private ApiResult(
            List<T> data,
            int count,
            int pageIndex,
            int pageSize)
        {
            Data = data;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        #region Methods
        // Pages a IQueryable source
        // param name="source" > IQueryable source of generic type
        // param name="pageIndex" > Zero-based current page index (0 = first page)
        // param name="pageSize" > The actual size of each page
        // return > A object containing the paged result and all the relevant paging navigation info
        public static async Task<ApiResult<T>> CreateAsync(
            IQueryable<T> source,
            int pageIndex,
            int pageSize)
        {
            var count = await source.CountAsync();
            source = source
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            var data = await source.ToListAsync();

            return new ApiResult<T>(
                data,
                count,
                pageIndex,
                pageSize);
        }
        #endregion

        #region Properties
        // The data result
        public List<T> Data { get; private set; }

        // Zero-based index of current page
        public int PageIndex { get; private set; }

        // Number of items contained in each page
        public int PageSize { get; private set; }

        // Total items count
        public int TotalCount { get; private set; }

        // Total pages count
        public int TotalPages { get; private set; }

        // TRUE if the current page has a previous page,
        // FALSE otherwise
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        // TRUE if the current page gas a next page,
        // FALSE otherwise
        public bool HasNextPage
        {
            get
            {
                return ((PageIndex + 1) < TotalPages);
            }
        }
        #endregion
    }
}
