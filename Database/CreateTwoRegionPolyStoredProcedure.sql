USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[TwoRegioPoly]    Script Date: 12/5/2015 12:28:51 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Tom Taylor
-- Create date: 1120/2015
-- Description:	SPTwoRegionPoly
-- =============================================
CREATE PROCEDURE [dbo].[TwoRegionQueryPoly] 
	-- Add the parameters for the stored procedure here
	@startDateTime datetime,  --'2013-11-02 14:00:00.000'
	@endDateTime datetime,

	--left hand rule and longitude first
	@polygonAsText text,  --'POLYGON((-73.95150661468506 40.81085348983537, -73.95871639251709 40.801270787660926, -73.95129203796387 40.79805458768175, -73.94433975219727 40.807735203040586, -73.95150661468506 40.81085348983537))'
	@polygonAsText2 text  --'POLYGON((-73.99047374725342 40.757985223340995, -73.99455070495605 40.75226369980747, -73.98369312286377 40.7475170623211, -73.97974491119385 40.7534340514971, -73.99047374725342 40.757985223340995))' 
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
SET NOCOUNT ON;

    -- Insert statements for procedure here

DECLARE @polygonAsGeography geography  -- this will hold the text polygon as a geography
DECLARE @polygonAsGeography2 geography  -- this will hold the 2nd text polygon as a geography

--important! for all geometry shapes left hand/foot rule applies!!!
--areas lying to the left hand side of the line drawn between the points are considered to be inside the polygon
SET @polygonAsGeography = geography::STGeomFromText(@polygonAsText, 4326);
SET @polygonAsGeography2 = geography::STGeomFromText(@polygonAsText2, 4326);

SELECT *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime)
--STIntersects only returns 0 or 1 regarding wether intersects or not where STIntersection should return the point if contained
AND
[pickup_geolocation].STIntersects(@polygonAsGeography) = 1 
AND 
[dropoff_geolocation].STIntersects(@polygonAsGeography2) = 1;

END

GO


