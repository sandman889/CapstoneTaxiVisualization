USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[TwoRegionCircle]    Script Date: 12/4/2015 10:30:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Tom Taylor
-- Create date: 1120/2015
-- Description:	SPTwoRegionCircle
-- =============================================
CREATE PROCEDURE [dbo].[TwoRegionQueryCircle] 
	-- Add the parameters for the stored procedure here
	@startDateTime datetime,  --'2013-11-02 14:00:00.000'
	@endDateTime datetime,
	
    @distanceInMeters float, -- unit is in meters for geography, circle 1
	@pointLatitude float,  --decimal(9, 6) 40.750995
	@pointLongitude float,  --decimal(9,6) -73.989659

	@distanceInMeters2 float, -- unit is in meters for geography, circle 2
	@pointLatitude2 float,  --decimal(9, 6) 40.785706
	@pointLongitude2 float  --decimal(9,6) -73.962880

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
SET NOCOUNT ON;

    -- Insert statements for procedure here

DECLARE @pointAsGeography geography
DECLARE @pointAsGeography2 geography

--when using POINT (geography) the order is latitude then longitude
SET @pointAsGeography = geography::Point(@pointLatitude, @pointLongitude, 4326);
SET @pointAsGeography2 = geography::Point(@pointLatitude2, @pointLongitude2, 4326);

SELECT *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime)
AND 
[pickup_geolocation].STDistance(@pointAsGeography) < @distanceInMeters
AND 
[dropoff_geolocation].STDistance(@pointAsGeography2) < @distanceInMeters2

END

GO


