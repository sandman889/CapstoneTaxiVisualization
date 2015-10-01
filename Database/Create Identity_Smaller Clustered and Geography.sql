--Staging database must exist!
--Load in sections, look for 1), 2), 3)...
--1) look for and change import file paths

--Drop the tables if necessary
--DROP TABLE [Staging].[dbo].[Trip_Data_Staging]
--DROP TABLE [Staging].[dbo].[Trip_Fare_Staging]
--DROP TABLE [Staging].[dbo].[Join_Staging]
--DROP TABLE [Staging].[dbo].[Identity_Smaller]

--1)  Import data and fare csv files into respective staging tables (5 minutes)
--Create trip data staging table

USE [Staging]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Trip_Data_Staging](
	[medallion] [varchar](50) NOT NULL,
	[hack_license] [varchar](50) NOT NULL,
	[vendor_id] [varchar](10) NOT NULL,
	[rate_code] [varchar](10) NULL,
	[store_and_fwd_flag] [varchar](10) NULL,
	[pickup_datetime] [datetime] NOT NULL,
	[dropoff_datetime] [datetime] NOT NULL,
	[passenger_count] [varchar](10) NULL,
	[trip_time_in_secs] [varchar](10) NULL,
	[trip_distance] [varchar](10) NULL,
	[pickup_longitude] [varchar](20) NOT NULL,
	[pickup_latitude] [varchar](20) NOT NULL,
	[dropoff_longitude] [varchar](20) NOT NULL,
	[dropoff_latitude] [varchar](20) NOT NULL
	CONSTRAINT PK_Data PRIMARY KEY (medallion, hack_license, vendor_id, pickup_datetime)

) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

--Create trip fare staging table

USE [Staging]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Trip_Fare_Staging](
	[medallion] [varchar](50) NOT NULL,
	[hack_license] [varchar](50) NOT NULL,
	[vendor_id] [varchar](10) NOT NULL,
	[pickup_datetime] [datetime] NOT NULL,
	[payment_type] [varchar](10) NULL,
	[fare_amount] [varchar](10) NULL,
	[surcharge] [varchar](10) NULL,
	[mta_tax] [varchar](10) NULL,
	[tip_amount] [varchar](10) NULL,
	[tolls_amount] [varchar](10) NULL,
	[total_amount] [varchar](10) NULL,
	CONSTRAINT PK_Fare PRIMARY KEY (medallion, hack_license, vendor_id, pickup_datetime)
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

--Bulk insert csv into trip data staging table

BULK INSERT [Staging].[dbo].[Trip_Data_Staging]
    FROM 'C:\Users\tataylor\Desktop\NYC Taxi Data 2013\trip_data_11.csv'  --change to appropriate path
    WITH
    (
    CHECK_CONSTRAINTS,
    FIRSTROW = 2,
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n', 
    MAXERRORS = 10,
    ERRORFILE = 'C:\Users\tataylor\Desktop\NYC Taxi Data 2013\trip_data_11_error_rows.csv',   --change to appropriate path 
    TABLOCK)

DECLARE @Trip_Data_Staging_Number_Of_Rows int
SET @Trip_Data_Staging_Number_Of_Rows = @@ROWCOUNT

SELECT @Trip_Data_Staging_Number_Of_Rows AS 'Trip Data Number Of Rows'

--Bulk insert csv into trip fare staging table

BULK INSERT [Staging].[dbo].[Trip_Fare_Staging]
    FROM 'C:\Users\tataylor\Desktop\NYC Taxi Data 2013\trip_fare_11.csv'  --change to appropriate path
    WITH
    (
    CHECK_CONSTRAINTS,
    FIRSTROW = 2,
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    MAXERRORS = 10,
    ERRORFILE = 'C:\Users\tataylor\Desktop\NYC Taxi Data 2013\trip_fare_11_error_rows.csv',  --change to appropriate path
    TABLOCK)

DECLARE @Trip_Fare_Staging_Number_Of_Rows int
SET @Trip_Fare_Staging_Number_Of_Rows = @@ROWCOUNT

SELECT @Trip_Fare_Staging_Number_Of_Rows AS 'Trip Fare Number Of Rows'

--2) Some verification (5 minutes)
--This should give use two staging tables with the same month of data in each one.
--The medallion, hack_license, vendor_id, and pickup_datetime comprise the primary key.

--Verify that the row counts of the two tables match

--Verify via two different joins that the unique primary keys exactly match between the two tables

--Inner Join (Number of rows must match the rows of the two joining tables)

SELECT *

FROM [Staging].[dbo].[Trip_Data_Staging] a

INNER JOIN [Staging].[dbo].[Trip_Fare_Staging] b

ON a.[medallion] = b.[medallion] and a.[hack_license] = b.[hack_license] and 
a.[vendor_id] = b.[vendor_id] and a.[pickup_datetime] = b.[pickup_datetime]

DECLARE @Inner_Join_Check_Number_Of_Rows int
SET @Inner_Join_Check_Number_Of_Rows = @@ROWCOUNT

SELECT @Inner_Join_Check_Number_Of_Rows AS 'Inner Join Check Number Of Rows'

--Full Outer Join And Excluding Nulls (Number of rows should be zero)

SELECT *

FROM [Staging].[dbo].[Trip_Data_Staging] a

FULL OUTER JOIN [Staging].[dbo].[Trip_Fare_Staging] b

ON a.[medallion] = b.[medallion] and a.[hack_license] = b.[hack_license] and 
a.[vendor_id] = b.[vendor_id] and a.[pickup_datetime] = b.[pickup_datetime]

WHERE a.[medallion] IS null OR b.[medallion] IS null

DECLARE @Full_Outer_Join_Check_Zero_Rows int
SET @Full_Outer_Join_Check_Zero_Rows = @@ROWCOUNT

SELECT @Full_Outer_Join_Check_Zero_Rows AS 'Full Outer Join Check Zero Rows'

--3) Join the two staging tables into one join table (5 minutes)
--Join both tables into one bringing in only one set of primary key columns

--Create the join table

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Join_Staging](
	[medallion] [varchar](50) NOT NULL,
	[hack_license] [varchar](50) NOT NULL,
	[vendor_id] [varchar](10) NOT NULL,
	[rate_code] [varchar](10) NULL,
	[store_and_fwd_flag] [varchar](10) NULL,
	[pickup_datetime] [datetime] NOT NULL,
	[dropoff_datetime] [datetime] NOT NULL,
	[passenger_count] [varchar](10) NULL,
	[trip_time_in_secs] [varchar](10) NULL,
	[trip_distance] [varchar](10) NULL,
	[pickup_longitude] [varchar](20) NOT NULL,
	[pickup_latitude] [varchar](20) NOT NULL,
	[dropoff_longitude] [varchar](20) NOT NULL,
	[dropoff_latitude] [varchar](20) NOT NULL,
	[payment_type] [varchar](10) NULL,
	[fare_amount] [varchar](10) NULL,
	[surcharge] [varchar](10) NULL,
	[mta_tax] [varchar](10) NULL,
	[tip_amount] [varchar](10) NULL,
	[tolls_amount] [varchar](10) NULL,
	[total_amount] [varchar](10) NULL
	CONSTRAINT Primary_Key_10 PRIMARY KEY (medallion, hack_license, vendor_id, pickup_datetime)
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

--Join into created table

INSERT INTO [Staging].[dbo].[Join_Staging]
 
SELECT a.[medallion], a.[hack_license], a.[vendor_id], [rate_code], [store_and_fwd_flag], 
a.[pickup_datetime], [dropoff_datetime], [passenger_count], [trip_time_in_secs], 
[trip_distance], [pickup_longitude], [pickup_latitude], [dropoff_longitude], 
[dropoff_latitude], [payment_type], [fare_amount], [surcharge], [mta_tax], 
[tip_amount], [tolls_amount], [total_amount]

FROM [Staging].[dbo].[Trip_Data_Staging] a
LEFT JOIN [Staging].[dbo].[Trip_Fare_Staging] b
ON a.[medallion] = b.[medallion] and a.[hack_license] = b.[hack_license] and 
a.[vendor_id] = b.[vendor_id] and a.[pickup_datetime] = b.[pickup_datetime]

DECLARE @Join_Staging_Table_Number_Of_Rows int
SET @Join_Staging_Table_Number_Of_Rows = @@ROWCOUNT

SELECT @Join_Staging_Table_Number_Of_Rows AS 'Join Staging Table Number Of Rows'

--4) Clean up latitude and longitude columns (20 minutes)
--At this point the latitude and longitude columns are still varchar(20).
--1st) Remove all of the rows with Nulls.
--2nd) Remove all of the rows with blanks.
--3rd) Remove all of the rows that are zero
--4th) Remove all of the rows containing scientific notation format(-2.83E-02)

/*
These columns are already all NOT NULL so they can't have Nulls

DELETE --SELECT COUNT(*) AS 'Longitude/Latitude Nulls'
 
FROM [Staging].[dbo].[Join_Staging]

WHERE [pickup_longitude] IS Null OR [pickup_latitude] IS Null OR
[dropoff_longitude] IS Null OR [dropoff_latitude] IS Null

DECLARE @Longitude_Latitude_Nulls int
SET @Longitude_Latitude_Nulls = @@ROWCOUNT

SELECT @Longitude_Latitude_Nulls AS 'Longitude/Latitude Nulls'
*/

DELETE --SELECT COUNT(*) AS 'Longitude/Latitude Blanks'
 
FROM [Staging].[dbo].[Join_Staging]

WHERE [pickup_longitude] = '' OR [pickup_latitude] = '' OR
[dropoff_longitude] = '' OR [dropoff_latitude] = ''

DECLARE @Longitude_Latitude_Blanks int
SET @Longitude_Latitude_Blanks = @@ROWCOUNT

SELECT @Longitude_Latitude_Blanks AS 'Longitude/Latitude Blanks'

DELETE --SELECT COUNT(*) AS 'Longitude/Latitude Zeros'
 
FROM [Staging].[dbo].[Join_Staging]

WHERE [pickup_longitude] = '0' OR [pickup_latitude] = '0' OR
[dropoff_longitude] = '0' OR [dropoff_latitude] = '0'

DECLARE @Longitude_Latitude_Zeros int
SET @Longitude_Latitude_Zeros = @@ROWCOUNT

SELECT @Longitude_Latitude_Zeros AS 'Longitude/Latitude Zeros'

DELETE --SELECT COUNT(*) AS 'Longitude/Latitude Scientific Notation'
 
FROM [Staging].[dbo].[Join_Staging]

WHERE [pickup_longitude] LIKE '%E%' OR [pickup_latitude] LIKE '%E%' OR
[dropoff_longitude] LIKE '%E%' OR [dropoff_latitude] LIKE '%E%'

DECLARE @Longitude_Latitude_Scientific_Notation int
SET @Longitude_Latitude_Scientific_Notation = @@ROWCOUNT

SELECT @Longitude_Latitude_Scientific_Notation AS 'Longitude/Latitude Scientific Notation'

--Change the varchar longitude and latitude columns into [decimal](19, 10)
--Picked this as it should be large enough to be safe and is the next storage size (bytes) up from [decimal](9,6)

--Change pickup_longitude column into [decimal](19, 10)

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [pickup_longitude] [decimal](19, 10)

--Change pickup_latitude column into [decimal](19, 10)

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [pickup_latitude] [decimal](19, 10)

--Change dropoff_longitude column into [decimal](19, 10)

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [dropoff_longitude] [decimal](19, 10)

--Change dropoff_latitude column into [decimal](19, 10)

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [dropoff_latitude] [decimal](19, 10)

--Now that the longitude and latitude columns are in [decimal](19, 10) we can do a range based delete
--to only keep rows that have longitudes and latitudes that are somwhat near NYC
--NYC: -74W by 40.5N  so range will be: -71 - -77W by 39 - 42N
-- My thought is that this is a reasonable bounding range lookin at a map

DELETE --SELECT COUNT(*) AS 'Longitude/Latitude Out of Range'
 
FROM [Staging].[dbo].[Join_Staging]

WHERE [pickup_longitude] NOT BETWEEN -77 AND -71 
OR [dropoff_longitude] NOT BETWEEN -77 AND -71
OR [pickup_latitude] NOT BETWEEN 39 AND 42
OR [dropoff_latitude] NOT BETWEEN 39 AND 42

DECLARE @Longitude_Latitude_Out_Of_Range int
SET @Longitude_Latitude_Out_Of_Range = @@ROWCOUNT

SELECT @Longitude_Latitude_Out_Of_Range AS 'Longitude/Latitude Out Of Range'

--Should now be able to alter longitude and latitude columns to [decimal](9, 6) and NOT NULL

--Change pickup_longitude column into [decimal](9, 6) and NOT NULL

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [pickup_longitude] [decimal](9, 6) NOT NULL

--Change pickup_latitude column into [decimal](9, 6) and NOT NULL

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [pickup_latitude] [decimal](9, 6) NOT NULL

--Change dropoff_longitude column into [decimal](9, 6) and NOT NULL

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [dropoff_longitude] [decimal](9, 6) NOT NULL

--Change dropoff_latitude column into [decimal](9, 6) and NOT NULL

ALTER TABLE [Staging].[dbo].[Join_Staging]
 
ALTER COLUMN [dropoff_latitude] [decimal](9, 6) NOT NULL

--5) Create the final, slimmed down table (5 minutes)
--Create the table with minimal columns and identity column for clustered index

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Identity_Smaller](
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
	[pickup_geolocation] [geography],
	[dropoff_geolocation][geography]
	CONSTRAINT PK_ID_CLUSTERED_10 PRIMARY KEY CLUSTERED
	([ID] ASC),
	INDEX IDX_PICKUP_NONCLUSTERED NONCLUSTERED
	([pickup_datetime] ASC),
	INDEX IDX_DROPOFF_NONCLUSTERED NONCLUSTERED
	([dropoff_datetime] ASC)
) ON [PRIMARY]

CREATE SPATIAL INDEX SPATIAL_PICKUP

ON [Staging].[dbo].[Identity_Smaller] ([pickup_geolocation])
USING GEOGRAPHY_AUTO_GRID
--USING GEOMETRY_GRID
--WITH (BOUNDING_BOX = (XMIN=-77,YMIN=39,XMAX=-71,YMAX=42))

CREATE SPATIAL INDEX SPATIAL_DROPOFF

ON [Staging].[dbo].[Identity_Smaller] ([dropoff_geolocation])
USING GEOGRAPHY_AUTO_GRID
--USING GEOMETRY_GRID
--WITH (BOUNDING_BOX = (XMIN=-77,YMIN=39,XMAX=-71,YMAX=42))	

 --insert certain columns from Join_Staging into this slimmed down
 --and differently indexed table

INSERT INTO [Staging].[dbo].[Identity_Smaller]
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

--6) Build the geography type spatial index for pickup (40 minutes)
--build the geography type spatial indexes as we convert latitudes and longitudes
--into data type geography

UPDATE [Staging].[dbo].[Identity_Smaller]

SET [pickup_geolocation] = [geography]::Point([pickup_latitude], [pickup_longitude], 4326)

WHERE [pickup_latitude] IS NOT NULL AND [pickup_longitude] IS NOT NULL

--7) Build the geography type spatial index for pickup (40 minutes)

UPDATE [Staging].[dbo].[Identity_Smaller]

SET [dropoff_geolocation] = [geography]::Point([dropoff_latitude], [dropoff_longitude], 4326)

WHERE [dropoff_latitude] IS NOT NULL AND [dropoff_longitude] IS NOT NULL

GO

SET ANSI_PADDING OFF
GO


