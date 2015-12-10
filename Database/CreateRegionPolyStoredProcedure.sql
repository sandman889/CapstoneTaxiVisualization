USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[RegionQueryPoly]    Script Date: 11/3/2015 11:46:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Tom Taylor
-- Create date: 09/22/2015
-- Description:	SPRegion
-- =============================================
CREATE PROCEDURE [dbo].[RegionQueryPoly] 
	-- Add the parameters for the stored procedure here
	@startDateTime datetime,  --'2013-11-02 14:00:00.000'
	@endDateTime datetime,

	--left hand rule and longitude first
	@polygonAsText text,  --'POLYGON((-73.993 40.75, -73.993 40.752, -73.995 40.752, -73.995 40.75, -73.993 40.75))'

	@pickDropBoth int --0 for pickups, 1 for dropoffs, 2 for both
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
SET NOCOUNT ON;

    -- Insert statements for procedure here

DECLARE @polygonAsGeography geography  -- this will hold the text polygon as a geography

--important! for all geometry shapes left hand/foot rule applies!!!
--areas lying to the left hand side of the line drawn between the points are considered to be inside the polygon
SET @polygonAsGeography = geography::STGeomFromText(@polygonAsText, 4326);

IF @pickDropBoth = 0 
 
SELECT *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime) AND [pickup_geolocation].STIntersects(@polygonAsGeography) = 1;
--STIntersects only returns 0 or 1 regarding wether intersects or not where STIntersection should return the point if contained

IF @pickDropBoth = 1 

SELECT *
FROM [dbo].[Taxi_Data]
WHERE (dropoff_datetime BETWEEN @startDateTime AND @endDateTime) AND [dropoff_geolocation].STIntersects(@polygonAsGeography) = 1;

IF @pickDropBoth = 2

SELECT *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime) AND [pickup_geolocation].STIntersects(@polygonAsGeography) = 1
UNION
SELECT *
FROM [dbo].[Taxi_Data]
WHERE (dropoff_datetime BETWEEN @startDateTime AND @endDateTime) AND [dropoff_geolocation].STIntersects(@polygonAsGeography) = 1;


END


GO


