﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CapstoneTaxiVisualization.Classes
{
    public interface IPagedList
    {
        int TotalCount { get; }
        int PageCount { get; }
        int Page { get; }
        int PageSize { get; }
    }

    public class PagedList<T> : List<T>, IPagedList
    {
        public int TotalCount { get; private set; }
        public int PageCount { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }

        public PagedList(IQueryable<T> source, int page, int pageSize)
        {
            TotalCount = source.Count();
            PageCount = GetPageCount(pageSize, TotalCount);
            Page = page < 1 ? 0 : page - 1;
            PageSize = pageSize;

            AddRange(source.Skip(Page * PageSize).Take(PageSize).ToList());
        }

        private int GetPageCount(int pageSize, int totalCount)
        {
            if (pageSize == 0)
                return 0;

            var remainder = totalCount % pageSize;
            return (totalCount / pageSize) + (remainder == 0 ? 0 : 1);
        }
    }
}