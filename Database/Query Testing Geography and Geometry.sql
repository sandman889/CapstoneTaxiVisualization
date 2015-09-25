SET STATISTICS TIME ON 

DECLARE @dist FLOAT = 1000 --meters

--can get geography points in different ways
--important to know that generally the order is longitude then latitude
--as in this method, it returns:  0xE6100000010C00000000006044408FC2F5285C7F52C0
DECLARE @p GEOGRAPHY = GEOGRAPHY::STGeomFromText(
  	'POINT(' + CONVERT(Varchar(30),-73.99, 2)
  	+ ' ' + CONVERT(Varchar(30),40.75, 2) + ')',4326);

--SELECT @p

--this method is the exception, using POINT (geography) the order is latitude then longitude
--as you can see we get the exact same result:  0xE6100000010C00000000006044408FC2F5285C7F52C0
DECLARE @w GEOGRAPHY = GEOGRAPHY::Point(40.75, -73.99, 4326)
--SELECT @w

--can get geometry points in different ways as well
--important to know that generally the order is longitude then latitude
--as in this method, it returns:  0xE6100000010C8FC2F5285C7F52C00000000000604440
--it says geometry tagged text, need to know what that means!!!
--it appears that this is geometry tagged text as it matches below which is correct
DECLARE @m GEOMETRY = GEOMETRY::STGeomFromText(
  	'POINT(' + CONVERT(Varchar(30),-73.99, 2)
  	+ ' ' + CONVERT(Varchar(30),40.75, 2) + ')',4326);

--SELECT @m

--using POINT (geometry) the order is longitude (X) then latitude (Y)
--as you can see we get the exact same result:  0xE6100000010C8FC2F5285C7F52C00000000000604440
DECLARE @n GEOMETRY = GEOMETRY::Point(-73.99, 40.75, 4326)
--SELECT @n




--some_geography.STDistance(other_geography)
--if some_geography is a geography point then this will return the distance from this point in meters
--against other geography
--the return value for this is in meters because that is what SRID 4326 is all about
--1609.344 meters = 1 mile

/*
--some_geometry.STDistance(other_geography)
STDistance() for geometries does not account for any supplied SRID
Distance is returned in units of measure based on the input system. 
For all geometries this is a grid and does not take the spatial 
reference ID into account. Any stored values for Z are also ignored.

Declare @geography geography = geography::STGeomFromText('Linestring(0 2, 1 1, 2 0)',4326);
Declare @PointGeography geography=  geography::STGeomFromText('Point(0.5 0.5)',4326);
Declare @geometry geometry= geometry::STGeomFromText('Linestring(0 2, 1 1, 2 0)',4326);
Declare @PointGeometry geometry=  geometry::STGeomFromText('Point(0.5 0.5)',4326);

Select @geography.STDistance(@PointGeography) as GeographyDistance,
@geometry.STDistance(@PointGeometry) as GeometryDistance;

--
A quick conversion if we need to find the distance between two geometry
points in meters with the WGS 84 DATUM (standard latitude and longitude
system in the U.S.) This takes a performance hit through the double 
conversions, but it gets the job done. The use of the function .STAsText
does not convey the SRID so that must be handled separately.
Select 
geography::STGeomFromText(@geometry.STAsText(), 
4326).STDistance(geography::STGeomFromText(@PointGeometry.STAsText(),4326))
as GeographySpatialReferenceIdDistance;
GO

-- To view the system of measure for a specific spatial reference ID, you can use the following:

Declare @SpatialReferenceID as int = 4326; -- SRID for WGS 84 Datum
Select unit_of_measure 
From sys.spatial_reference_systems
Where spatial_reference_id = @SpatialReferenceID ;
*/

--working on this circular string
--DECLARE @g geography = 'CIRCULARSTRING(-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653)';
--SELECT @g

/*
--working on this as well, just find the single nearest point, slow
--nearest point query (just nearest point)
SELECT TOP(5000) [pickup_geolocation].STDistance(@w) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Identity_Smaller_Nov_2013 WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
*/

--geography
--nearest point query (circle of 1000m from specified point) STDistance < some_distance
SELECT TOP(5000) [pickup_geolocation].STDistance(@w) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Identity_Smaller --WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
--8.7 seconds

--nearest point query (circle of 1000m from specified point) STIntersects and STBuffer
--STBuffer returns a geography object that represents the union of all points whose distance 
--from a geography instance is less than or equal to a specified value.
SELECT TOP(5000) [pickup_geolocation].STDistance(@w) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Identity_Smaller --WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STIntersects(@w.STBuffer(@dist))=1
ORDER BY [pickup_geolocation].STDistance(@w);
--7.5 seconds so faster

SET STATISTICS TIME ON 

DECLARE @geom_dist FLOAT = 0.012 --no units but scale is very small

--using POINT (geometry) the order is longitude (X) then latitude (Y)
--as you can see we get the exact same result:  0xE6100000010C8FC2F5285C7F52C00000000000604440
DECLARE @f GEOMETRY = GEOMETRY::Point(-73.99, 40.75, 4326)
SELECT @f

--geometry
--slowness definitely related to scalar problem 1000 vs 0.12 makes it apples to apples and now faster than geography
--nearest point query (circle of 1000m from specified point)
SELECT TOP(5000) [pickup_geolocation].STDistance(@f) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Identity_Smaller_Geometry --WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STDistance(@f) < @geom_dist
ORDER BY [pickup_geolocation].STDistance(@f);
--10.1 seconds

--nearest point query (circle of 1000m from specified point) STIntersects and STBuffer
--Returns a geography object that represents the union of all points whose distance 
--from a geography instance is less than or equal to a specified value.
SELECT TOP(5000) [pickup_geolocation].STDistance(@f) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Identity_Smaller_Geometry --WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STIntersects(@f.STBuffer(@geom_dist))=1
ORDER BY [pickup_geolocation].STDistance(@f);
--12 seconds a little faster

--convert to meters
DECLARE @bob GEOMETRY = 0xE6100000010CBF4692205C7F52C00CAEB9A3FF5F4440
DECLARE @tom GEOMETRY = GEOMETRY::Point(-73.99, 40.75, 4326)
Select 
geography::STGeomFromText(@bob.STAsText(), 
4326).STDistance(geography::STGeomFromText(@tom.STAsText(),4326))
as GeographySpatialReferenceIdDistance;
GO
--should come out to be around 1.23316195137727 meters and it does: 1.23316195137727

--geography
--region query
DECLARE @SquareFilled geography;
--important! for all geometry shapes left hand/foot rule applies!!!
--areas lying to the left hand side of the line drawn between the points are considered to be inside the polygon
--SET @SquareFilled = geography::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SET @SquareFilled = geography::STGeomFromText('POLYGON((-73.993 40.75, -73.993 40.752, -73.995 40.752, -73.995 40.75, -73.993 40.75))', 4326);
SELECT @SquareFilled

SELECT TOP(5000) [pickup_geolocation].STIntersection(@SquareFilled).ToString() AS 'Points Inside', [pickup_latitude], [pickup_longitude], [pickup_geolocation]
--STIntersects only returns 0 or 1 regarding wether intersects or not where STIntersection should return the point if contained
--SELECT TOP(5000) @SquareFilled.STIntersects([pickup_geolocation]) AS 'Points Inside', [pickup_latitude], [pickup_longitude]
FROM [dbo].Identity_Smaller WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STIntersects(@SquareFilled) = 1
--780 ms

--geometry
--region query
DECLARE @SquareFilledGeometry geometry;
--important! for all geometry shapes left hand/foot rule applies!!!
--areas lying to the left hand side of the line drawn between the points are considered to be inside the polygon
--SET @SquareFilled = geometry::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SET @SquareFilledGeometry = geometry::STGeomFromText('POLYGON((-73.993 40.75, -73.993 40.752, -73.995 40.752, -73.995 40.75, -73.993 40.75))', 4326);
SELECT @SquareFilledGeometry

--STIntersects
SELECT TOP(5000) [pickup_geolocation].STIntersection(@SquareFilledGeometry).ToString() AS 'Points Inside', [pickup_latitude], [pickup_longitude], [pickup_geolocation]
--STIntersects only returns 0 or 1 regarding wether intersects or not where STIntersection should return the point if contained
--SELECT TOP(5000) @SquareFilled.STIntersects([pickup_geolocation]) AS 'Points Inside', [pickup_latitude], [pickup_longitude]
FROM [dbo].Identity_Smaller_Geometry WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STIntersects(@SquareFilledGeometry) = 1
--640 ms

--STWithin
SELECT TOP(5000) [pickup_geolocation].STIntersection(@SquareFilledGeometry).ToString() AS 'Points Inside', [pickup_latitude], [pickup_longitude], [pickup_geolocation]
--STIntersects only returns 0 or 1 regarding wether intersects or not where STIntersection should return the point if contained
--SELECT TOP(5000) @SquareFilled.STIntersects([pickup_geolocation]) AS 'Points Inside', [pickup_latitude], [pickup_longitude]
FROM [dbo].Identity_Smaller_Geometry WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STWithin(@SquareFilledGeometry) = 1
--565ms

/*
--apparently STWithin and STContains are geometry only
SELECT TOP(5000) [pickup_geolocation].STIntersection(@SquareFilled).ToString() AS 'Points Inside', [pickup_latitude], [pickup_longitude], [pickup_geolocation]
--STIntersects only returns 0 or 1 regarding wether intersects or not where STIntersection should return the point if contained
--SELECT TOP(5000) @SquareFilled.STIntersects([pickup_geolocation]) AS 'Points Inside', [pickup_latitude], [pickup_longitude]
FROM [dbo].Identity_Smaller_Nov_2013 WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-11-02 14:00:00.000' AND '2013-11-07 18:00:00.000')
AND [pickup_geolocation].STContains(@SquareFilled) = 1
*/







--order by distance from point ascending
SELECT TOP(50) [pickup_geolocation].STDistance(@w) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Identity_Smaller_May_2013 WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-05-07 14:00:00.000' AND '2013-05-08 18:00:00.000')
AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
--3889ms

SELECT TOP(100) [pickup_geolocation].STDistance(@w) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Identity_Smaller_May_2013 --WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-05-07 14:00:00.000' AND '2013-05-08 18:00:00.000')
--AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
--2968ms

SELECT TOP(200) [pickup_geolocation].STDistance(@w) AS 'Distance From', *
FROM [dbo].Identity_Smaller_May_2013 WITH (INDEX([SPATIAL_PICKUP]))
WHERE (pickup_datetime BETWEEN '2013-05-07 14:00:00.000' AND '2013-05-07 18:00:00.000')
AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
--3050ms

SET STATISTICS TIME ON 

DECLARE @dist FLOAT = 1000  --meters

--can get geography points in different ways
--important to know that generally the order is longitude then latitude
--as in this method, it returns:  0xE6100000010C00000000006044408FC2F5285C7F52C0
DECLARE @p GEOGRAPHY = GEOGRAPHY::STGeomFromText(
  	'POINT(' + CONVERT(Varchar(30),-73.99, 2)
  	+ ' ' + CONVERT(Varchar(30),40.75, 2) + ')',4326);

--SELECT @p

--this method is the exception, using POINT the order is latitude then longitude
--as you can see we get the exact same result:  0xE6100000010C00000000006044408FC2F5285C7F52C0
DECLARE @w GEOGRAPHY = GEOGRAPHY::Point(40.75, -73.99, 4326)
--SELECT @w

--some_geography.STDistance(other_geography)
--if some_geography is a geography point then this will return the distance from this point in meters
--against other geography
--the return value for this is in meters because that is what SRID 4326 is all about
--1609.344 meters = 1 mile


--order by distance from point ascending
SELECT TOP(50) [pickup_geolocation].STDistance(@w) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Join_Staging_May_2013 WITH (INDEX([Spatial_PickUp]))
WHERE (pickup_datetime BETWEEN '2013-05-07 14:00:00.000' AND '2013-05-08 18:00:00.000')
AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
--22207ms

SELECT TOP(100) [pickup_geolocation].STDistance(@w) AS 'Distance From', [pickup_geolocation]
FROM [dbo].Join_Staging_May_2013 --WITH (INDEX([Spatial_PickUp]))
WHERE (pickup_datetime BETWEEN '2013-05-07 14:00:00.000' AND '2013-05-08 18:00:00.000')
--AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
--8465ms

SELECT TOP(200) [pickup_geolocation].STDistance(@w) AS 'Distance From', *
FROM [dbo].Join_Staging_May_2013 WITH (INDEX([Spatial_PickUp]))
WHERE (pickup_datetime BETWEEN '2013-05-07 14:00:00.000' AND '2013-05-07 18:00:00.000')
AND [pickup_geolocation].STDistance(@w) < @dist
ORDER BY [pickup_geolocation].STDistance(@w);
--19532ms

/*
This is how you draw objects

Drawing your first object
To start off basic, what is easier then drawing a simple square? A square consists of 4 coordinates, and is one of the most basic forms you can draw. An example of a square looks like this:
1
2
3
DECLARE @Square geometry;
SET @Square = geometry::STGeomFromText('LINESTRING (0 0, 0 100, 100 100, 100 0, 0 0)', 4326);
SELECT @Square
But what if you want a solid square, instead of an outline? In that case, you need to change the type you’re drawing into a polygon. Where the 4 lines in the example above just draw the outline of the object, a polygon (like the example below) will also contain everything within the lines you draw:
1
2
3
DECLARE @SquareFilled geometry;
SET @SquareFilled = geometry::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SELECT @SquareFilled
 
Layers
Okay, let’s take this one step further. You can also draw multiple objects in one context. These objects can be drawn next to each other, or on top of each other. Every object you draw will be drawn in a “separate layer”. Objects that don’t overlap are just 2 shapes (polygons). But if you draw 2 shapes on top of each other, it’s a whole different story. Both objects can actually aggregate into 1 big shape, or exclude each other. First, an example with 2 separate shapes:
1
2
3
4
5
6
7
8
9
DECLARE @Square geometry,
        @Triangle geometry;
 
SET @Square = geometry::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SET @Triangle = geometry::STGeomFromText('POLYGON((50 50,100 150,150 50, 50 50))', 4326);
 
SELECT @Square
UNION ALL
SELECT @Triangle

If you run the query above, you’ll see 2 objects appear: a square and a triangle. Both object overlap at a certain point, but they’re still 2 independent shapes.
 
Layer aggregation
Until now it’s just child’s play. Now we’re getting to the exiting stuff! How about combining the 2 previous objects into one big shape?
1
2
3
4
5
6
7
DECLARE @Square geometry,
        @Triangle geometry;
 
SET @Square = geometry::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SET @Triangle = geometry::STGeomFromText('POLYGON((50 50,100 150,150 50, 50 50))', 4326);
 
SELECT @Square.STUnion(@Triangle)
Now you’ll see that both objects merged into one single object. This is a result of “joining” 2 objects or layers. By using the extended method STUnion on one of your shapes, you can add another shape to it. So in the case, the triangle is added to the square.
 
Layer intersection
But what if you want to know the part of the polygon that intersects? So which part of object 1 overlaps object 2? You can do this by using the STIntersection method:
1
2
3
4
5
6
7
DECLARE @Square geometry,
        @Triangle geometry;
 
SET @Square = geometry::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SET @Triangle = geometry::STGeomFromText('POLYGON((50 50,100 150,150 50, 50 50))', 4326);
 
SELECT @Square.STIntersection(@Triangle)
Or maybe you want to know which part doesn’t overlap. Then you can query the difference of both objects:
1
2
3
4
5
6
7
DECLARE @Square geometry,
        @Triangle geometry;
 
SET @Square = geometry::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
SET @Triangle = geometry::STGeomFromText('POLYGON((50 50,100 150,150 50, 50 50))', 4326);
 
SELECT @Square.STSymDifference(@Triangle)
 
Center
As you see, there are many really cool things you can do with spatial data. One other I want to show you is how to determine the center of your object. Instead of calculating it yourself, you can use a method called STCentroid:
1
2
3
4
5
6
7
DECLARE @Square geometry;
 
SET @Square = geometry::STGeomFromText('POLYGON((0 0, 0 100, 100 100, 100 0, 0 0))', 4326);
 
SELECT @Square
UNION ALL
SELECT @Square.STCentroid().STBuffer(10)
Just to keep it visual, I’ve added a buffer to the center point. What STBuffer does, is adding a radial to the selected object. So in this case, it created a radial around the center point.
If you didn’t draw that extra radial, it would literally just be a pixel on your screen. So by adding a buffer around the center, it’s still visible. But it’s only for visual purposes, and isn’t required to make this query work.

STContains() can be used for geography data also as per http://technet.microsoft.com/en-us/library/ff929274.aspx

STContains() is a proper method, too. Examples:

update test_points_indexed 
set in_out_of_poly = geometry::Parse('POLYGON((121.038 37.597, 120.995 37.609, 120.998 37.636, 121.039 37.639, 121.038 37.597))').STContains(geog)
from tab;

update test_points_indexed 
set in_out_of_poly = x.polygon.STContains(y.geogcol1) 
from spatialtable x, test_points_indexed y;

DECLARE @poly geography;
SELECT @poly = GeogCol1 FROM dbo.SpatialTable;

UPDATE dbo.test_points_indexed
SET In_Out_of_Poly = @poly.STIntersects(geog);
 

This will set the In_out_of_polygon value for each point to 1 if it lies in the polygon, or 0 otherwise, as follows:
*/
--example
DECLARE @g geography;
DECLARE @h geography;
SET @g = geography::STGeomFromText('POLYGON((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))', 4326);
SET @h = geography::STGeomFromText('LINESTRING(-122.360 47.656, -122.343 47.656)', 4326);
SELECT @g
SELECT @h
SELECT @g.STIntersection(@h).ToString();
