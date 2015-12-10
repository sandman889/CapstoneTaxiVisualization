USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[NearestPoint]   Script Date: 10/29/2015 11:47:08 AM ******/
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
	@pointLongitude float,  --decimal(9,6) -73.999999

	@pickDropBoth int --0 for pickups, 1 for dropoffs, 2 for both

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
SET NOCOUNT ON;

    -- Insert statements for procedure here

--nearest point query STDistance < some_distance
DECLARE @pointAsGeography geography

--when using POINT (geography) the order is latitude then longitude
SET @pointAsGeography = geography::Point(@pointLatitude, @pointLongitude, 4326);

IF @pickDropBoth = 0 
 
SELECT TOP (10) WITH TIES [pickup_geolocation].STDistance(@pointAsGeography) AS 'Distance From', *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime)
AND [pickup_geolocation].STDistance(@pointAsGeography) < @distanceInMeters
ORDER BY [pickup_geolocation].STDistance(@pointAsGeography);

IF @pickDropBoth = 1 

SELECT TOP (10) WITH TIES [dropoff_geolocation].STDistance(@pointAsGeography) AS 'Distance From', *
FROM [dbo].[Taxi_Data]
WHERE (dropoff_datetime BETWEEN @startDateTime AND @endDateTime)
AND [dropoff_geolocation].STDistance(@pointAsGeography) < @distanceInMeters
ORDER BY [dropoff_geolocation].STDistance(@pointAsGeography);

IF @pickDropBoth = 2

SELECT TOP (5)  [pickup_geolocation].STDistance(@pointAsGeography) AS 'Distance From', *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime)
AND [pickup_geolocation].STDistance(@pointAsGeography) < @distanceInMeters
UNION
SELECT TOP (5)  [dropoff_geolocation].STDistance(@pointAsGeography) AS 'Distance From', *
FROM [dbo].[Taxi_Data]
WHERE (dropoff_datetime BETWEEN @startDateTime AND @endDateTime)
AND [dropoff_geolocation].STDistance(@pointAsGeography) < @distanceInMeters
ORDER BY 'Distance From';

END

GO


