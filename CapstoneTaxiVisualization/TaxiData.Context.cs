﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CapstoneTaxiVisualization
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class TaxiDataEntities : DbContext
    {
        public TaxiDataEntities()
            : base("name=TaxiDataEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Identity_Smaller> Identity_Smaller { get; set; }
    
        public virtual ObjectResult<GetPointsFromInsideRegion_Result> GetPointsFromInsideRegion(Nullable<System.DateTime> startDateTime, Nullable<System.DateTime> endDateTime, string polygonAsText)
        {
            var startDateTimeParameter = startDateTime.HasValue ?
                new ObjectParameter("startDateTime", startDateTime) :
                new ObjectParameter("startDateTime", typeof(System.DateTime));
    
            var endDateTimeParameter = endDateTime.HasValue ?
                new ObjectParameter("endDateTime", endDateTime) :
                new ObjectParameter("endDateTime", typeof(System.DateTime));
    
            var polygonAsTextParameter = polygonAsText != null ?
                new ObjectParameter("polygonAsText", polygonAsText) :
                new ObjectParameter("polygonAsText", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPointsFromInsideRegion_Result>("GetPointsFromInsideRegion", startDateTimeParameter, endDateTimeParameter, polygonAsTextParameter);
        }
    
        public virtual int NearestPointQuery(Nullable<System.DateTime> startDateTime, Nullable<System.DateTime> endDateTime, Nullable<double> distanceInMeters, Nullable<double> pointLatitude, Nullable<double> pointLongitude)
        {
            var startDateTimeParameter = startDateTime.HasValue ?
                new ObjectParameter("startDateTime", startDateTime) :
                new ObjectParameter("startDateTime", typeof(System.DateTime));
    
            var endDateTimeParameter = endDateTime.HasValue ?
                new ObjectParameter("endDateTime", endDateTime) :
                new ObjectParameter("endDateTime", typeof(System.DateTime));
    
            var distanceInMetersParameter = distanceInMeters.HasValue ?
                new ObjectParameter("distanceInMeters", distanceInMeters) :
                new ObjectParameter("distanceInMeters", typeof(double));
    
            var pointLatitudeParameter = pointLatitude.HasValue ?
                new ObjectParameter("pointLatitude", pointLatitude) :
                new ObjectParameter("pointLatitude", typeof(double));
    
            var pointLongitudeParameter = pointLongitude.HasValue ?
                new ObjectParameter("pointLongitude", pointLongitude) :
                new ObjectParameter("pointLongitude", typeof(double));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("NearestPointQuery", startDateTimeParameter, endDateTimeParameter, distanceInMetersParameter, pointLatitudeParameter, pointLongitudeParameter);
        }
    }
}
