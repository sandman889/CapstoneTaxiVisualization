USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[GetPointsFromInsideRegion]    Script Date: 9/24/2015 10:10:27 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Tom Taylor
-- Create date: 09/22/2015
-- Description:	SPRegion
-- =============================================
CREATE PROCEDURE [dbo].[GetPointsFromInsideRegion] 
	-- Add the parameters for the stored procedure here
	@startDateTime datetime,  --'2013-11-02 14:00:00.000'
	@endDateTime datetime,
	--left hand rule and longitude first
	@polygonAsText text  --'POLYGON((-73.993 40.75, -73.993 40.752, -73.995 40.752, -73.995 40.75, -73.993 40.75))'
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
--geography
--region query
DECLARE @polygonAsGeography geography

--important! for all geometry shapes left hand/foot rule applies!!!
--areas lying to the left hand side of the line drawn between the points are considered to be inside the polygon
--SET @SquareFilled = geography::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SET @polygonAsGeography = geography::STGeomFromText(@polygonAsText, 4326);
--SELECT @polygonAsGeography

SELECT TOP(1000) [pickup_geolocation].STIntersection(@polygonAsGeography).ToString() AS 'Points Inside', [pickup_latitude], [pickup_longitude], [pickup_geolocation], [total_amount]
--STIntersects only returns 0 or 1 regarding wether intersects or not where STIntersection should return the point if contained
--SELECT TOP(5000) @SquareFilled.STIntersects([pickup_geolocation]) AS 'Points Inside', [pickup_latitude], [pickup_longitude]
FROM [dbo].Identity_Smaller --WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN @startDateTime  AND @endDateTime)
AND [pickup_geolocation].STIntersects(@polygonAsGeography) = 1
--780 ms



END


GO

