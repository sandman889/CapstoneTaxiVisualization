--Staging database must exist!
--Join_Staging table must exist!

--Geometry NOT Geography
--5) Create the final, slimmed down table (5 minutes)
--Create the table with minimal columns and identity column for clustered index

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Identity_Smaller_Geometry](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[pickup_datetime] [datetime] NOT NULL,
	[dropoff_datetime] [datetime] NOT NULL,
	[passenger_count] [varchar](10) NULL,
	[trip_time_in_secs] [varchar](10) NULL,
	[trip_distance] [varchar](10) NULL,
	[pickup_longitude] [decimal](9,6) NOT NULL,
	[pickup_latitude] [decimal](9,6) NOT NULL,
	[dropoff_longitude] [decimal](9,6) NOT NULL,
	[dropoff_latitude] [decimal](9,6) NOT NULL,
	[fare_amount] [varchar](10) NULL,
	[tip_amount] [varchar](10) NULL,
	[tolls_amount] [varchar](10) NULL,
	[total_amount] [varchar](10) NULL,
	[pickup_geolocation] [geometry],
	[dropoff_geolocation][geometry]
	CONSTRAINT PK_ID_CLUSTERED PRIMARY KEY CLUSTERED
	([ID] ASC),
	INDEX IDX_PICKUP_NONCLUSTERED NONCLUSTERED
	([pickup_datetime] ASC),
	INDEX IDX_DROPOFF_NONCLUSTERED NONCLUSTERED
	([dropoff_datetime] ASC)
) ON [PRIMARY]

CREATE SPATIAL INDEX SPATIAL_PICKUP

ON [Staging].[dbo].[Identity_Smaller_Geometry] ([pickup_geolocation])
USING GEOMETRY_AUTO_GRID
WITH (BOUNDING_BOX = (XMIN=-77,YMIN=39,XMAX=-71,YMAX=42))

CREATE SPATIAL INDEX SPATIAL_DROPOFF

ON [Staging].[dbo].[Identity_Smaller_Geometry] ([dropoff_geolocation])
USING GEOMETRY_AUTO_GRID
WITH (BOUNDING_BOX = (XMIN=-77,YMIN=39,XMAX=-71,YMAX=42))	

 --insert certain columns from Join_Staging into this slimmed down
 --and differently indexed table

INSERT INTO [Staging].[dbo].[Identity_Smaller_Geometry]
(
[pickup_datetime], [dropoff_datetime], [passenger_count], [trip_time_in_secs], 
[trip_distance], [pickup_longitude], [pickup_latitude], [dropoff_longitude], 
[dropoff_latitude], [fare_amount], [tip_amount], [tolls_amount], [total_amount]
)
 
SELECT 
[pickup_datetime], [dropoff_datetime], [passenger_count], [trip_time_in_secs], 
[trip_distance], [pickup_longitude], [pickup_latitude], [dropoff_longitude], 
[dropoff_latitude], [fare_amount], [tip_amount], [tolls_amount], [total_amount]

FROM [Staging].[dbo].[Join_Staging]

--6) Build the geometry type spatial index for pickup (20 minutes)
--build the geometry type spatial indexes as we convert latitudes and longitudes
--into data type geometry

GO

SET ANSI_PADDING ON
GO

UPDATE [Staging].[dbo].[Identity_Smaller_Geometry]
--careful, these are different (opposite) because X (as in X_Axis) relates to longitude
--[geography]::Point ( Lat, Long, SRID )
--[geometry]::Point ( X, Y, SRID )
SET [pickup_geolocation] = [geometry]::Point([pickup_longitude], [pickup_latitude], 4326)

WHERE [pickup_latitude] IS NOT NULL AND [pickup_longitude] IS NOT NULL

--7) Build the geometry type spatial index for dropoff (20 minutes)

UPDATE [Staging].[dbo].[Identity_Smaller_Geometry]

SET [dropoff_geolocation] = [geometry]::Point([dropoff_longitude], [dropoff_latitude], 4326)

WHERE [dropoff_latitude] IS NOT NULL AND [dropoff_longitude] IS NOT NULL

GO

SET ANSI_PADDING OFF
GO


