USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[LineWithVolume]    Script Date: 12/3/2015 11:06:28 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Tom Taylor
-- Create date: 1120/2015
-- Description:	LineWithVolume
-- =============================================
CREATE PROCEDURE [dbo].[LineWithVolume] 
	-- Add the parameters for the stored procedure here
	@startDateTime datetime,  --'2013-11-02 14:00:00.000'
	@endDateTime datetime,

--longitude first
	@lineStringAsText text --'LINESTRING(-73.968450 40.748384, -74.000980 40.761885)'
	                       	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
SET NOCOUNT ON;

    -- Insert statements for procedure here

DECLARE @lineStringAsGeography geography  -- this will hold the text lineString as a geography

SET @lineStringAsGeography = geography::STLineFromText(@lineStringAsText, 4326);

DECLARE @lineStringWithVolume geography  -- this will hold the geography lineString with a "volume" to it

SET @lineStringWithVolume = @lineStringAsGeography.STBuffer(3)
-- STBuffer gives the lineString geography a 3 meter (18 feet wide) buffer 

SELECT *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime) AND 
[pickup_geolocation].STIntersects(@lineStringWithVolume) = 1;
--STIntersects only returns 0 or 1 regarding whether intersects or not where STIntersection should return the point if contained

END

GO

