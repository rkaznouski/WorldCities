using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace WorldCities.Data
{
    public class ApiResult<T>
    {
        /// <summary>
        /// Private constructor called by the CreateAsync method
        /// </summary> 
        private ApiResult(
            List<T> data,
            int count,
            int pageIndex,
            int pageSize,
            string sortColumn,
            string sortOrder)
        {
            Data = data;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            SortColumn = sortColumn;
            SortOrder = sortOrder;
        }

        #region Methods
        /// <summary>
        /// Pages and/or sorts a IQueryble source
        /// </summary>
        /// <param name="source">An IQueryable source of generic type.</param>
        /// <param name="pageIndex">Zero-based current page index (0 = firstPage)</param>
        /// <param name="pageSize">The actual size of each page.</param>
        /// <param name="sortColumn">The sorting column name</param>
        /// <param name="sortOrder">The sorting order ("ASC" pr "DESC")</param>
        /// <returns>
        /// A object containing the paged result
        /// and all the relevant paging navigation info.
        /// </returns>
        public static async Task<ApiResult<T>> CreateAsync(
            IQueryable<T> source,
            int pageIndex,
            int pageSize,
            string sortColumn = null,
            string sortOrder = null)
        {
            var count = await source.CountAsync();
            
            if(!String.IsNullOrEmpty(sortColumn)
                && IsValidProperty(sortColumn))
            {
                sortOrder = !String.IsNullOrEmpty(sortOrder)
                    && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                source = source.OrderBy(
                    String.Format(
                        "{0} {1}",
                        sortColumn,
                        sortOrder)
                    );
            }

            source = source
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
            var data = await source.ToListAsync();
            return new ApiResult<T>(
                data,
                count,
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the given property name exists
        /// to protect against SQL injection attacks
        /// </summary>
        public static bool IsValidProperty(
            string propertyName,
            bool throwExceptionIfNotFound = true)
        {
            var prop = typeof(T).GetProperty(
                propertyName,
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance);
            if (prop == null && throwExceptionIfNotFound)
                throw new NotSupportedException(
                    String.Format(
                        "ERROR: Property '{0}' does not exist.",
                        propertyName));
            return prop != null;
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

        /// <summary>
        /// Sorting Column name (or null if none set)
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// Sorting Order ("ASC", "DESC" or null if none set)
        /// </summary>
        public string SortOrder { get; set; }
        #endregion
    }
}
