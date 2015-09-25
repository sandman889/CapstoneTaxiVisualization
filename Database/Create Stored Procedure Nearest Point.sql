USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[NearestPointQuery]    Script Date: 9/24/2015 10:10:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Tom Taylor
-- Create date: 09/22/2015
-- Description:	SPNearestPoint
-- =============================================
CREATE PROCEDURE [dbo].[NearestPointQuery] 
	-- Add the parameters for the stored procedure here
	@startDateTime datetime,  --'2013-11-02 14:00:00.000'
	@endDateTime datetime,
	@distanceInMeters float, -- unit is in meters for geography
	@pointLatitude float,  --decimal(9, 6) 40.757575
	@pointLongitude float  --decimal(9,6) -73.999999
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
--geography
--nearest point query STDistance < some_distance
DECLARE @pointAsGeography geography

--when using POINT (geography) the order is latitude then longitude
SET @pointAsGeography = geography::Point(@pointLatitude, @pointLongitude, 4326);
--SELECT @pointAsGeography

SELECT TOP(5000) [pickup_geolocation].STDistance(@pointAsGeography) AS 'Distance From', [pickup_latitude], [pickup_longitude], [pickup_geolocation], [total_amount]
FROM [dbo].Identity_Smaller --WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STDistance(@pointAsGeography) < @distanceInMeters
ORDER BY [pickup_geolocation].STDistance(@pointAsGeography);
--8.7 seconds


END

GO

