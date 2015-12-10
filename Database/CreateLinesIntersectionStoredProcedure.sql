USE [Staging]
GO

/****** Object:  StoredProcedure [dbo].[LinesIntersection]    Script Date: 10/29/2015 11:47:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Tom Taylor
-- Create date: 11/01/2015
-- Description:	SPLinesIntersection
-- =============================================
CREATE PROCEDURE [dbo].[LinesIntersectionQuery] 
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

DECLARE @lineStringAsGeography geography  -- this will hold the text polygon as a geography

SET @lineStringAsGeography = geography::STLineFromText(@lineStringAsText, 4326);


SELECT *
FROM [dbo].[Taxi_Data]
WHERE (pickup_datetime BETWEEN @startDateTime AND @endDateTime) AND 
geography::STLineFromText('LINESTRING('+ CAST([pickup_longitude] AS VARCHAR) + ' ' + CAST([pickup_latitude] AS VARCHAR) + ', '
+ CAST([dropoff_longitude] AS VARCHAR) + ' ' + CAST([dropoff_latitude] AS VARCHAR) + ')', 4326).MakeValid().STIntersects(@lineStringAsGeography) = 1;
--STIntersects only returns 0 or 1 regarding whether intersects or not where STIntersection should return the point if contained


END

GO


