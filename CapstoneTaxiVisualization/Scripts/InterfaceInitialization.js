﻿function InitializeInterface() {
    //build the map element
    L.mapbox.accessToken = 'pk.eyJ1Ijoic2FuZG1hbjg4OSIsImEiOiJjaWV1aXd0bmYwaTRscjhtMGc5Y2NqMnB2In0.tpP5eJMyyGr-haIjRUq1jQ';
    var osmUrl = 'https://api.mapbox.com/v4/mapbox.streets/{z}/{x}/{y}.png?access_token=' + L.mapbox.accessToken,
        osmAttrib = '<a href="http://www.mapbox.com/about/maps/" target="_blank">Terms &amp; Feedback</a>',
        osm = L.tileLayer(osmUrl, { maxZoom: 18, attribution: osmAttrib }),
        map = new L.Map('map', { layers: [osm], center: new L.LatLng(40.72332345541449, -73.99), zoom: 15 });

    var drawnItems = new L.FeatureGroup();
    map.addLayer(drawnItems);

    var drawControl = new L.Control.Draw({
        position: 'topright',
        draw: {
            polyline: {
                metric: true
            },
            polygon: {
                allowIntersection: false,
                showArea: true,
                drawError: {
                    color: '#b00b00',
                    timeout: 1000
                },
                shapeOptions: {
                    color: '#bada55'
                }
            },
            circle: {
                shapeOptions: {
                    color: '#662d91'
                }
            },
            marker: false
        },
        edit: {
            featureGroup: drawnItems,
            remove: false
        }
    });
    map.addControl(drawControl);

    //on draw create, format the points correctly and query the server for the points in that time
    map.on('draw:created', function (e) {
        var type = e.layerType,
            layer = e.layer;

        if (type == 'polygon' || type == 'rectangle') {
            //grab the points from the polygon, and format them correctly to send to the server
            var geoJson = layer.toGeoJSON()
            var correctedPoints = TaxiVizUtil.BuildFormattedLatLong(geoJson, true);

            TaxiVizUtil.RegionQueryDisplay(correctedPoints, map);

            drawnItems.addLayer(layer);
            TaxiVizUtil.currentLayers.push(layer);
        }
        else if (type == 'polyline') {
            //code to account for line being drawn
            var test = layer.toGeoJSON();
            var correctedPoints = TaxiVizUtil.BuildFormattedLatLong(layer.toGeoJSON(), false);

            TaxiVizUtil.LineIntersectionDisplay(correctedPoints, map);

            drawnItems.addLayer(layer);
            TaxiVizUtil.currentLayers.push(layer);
        }
        else if (type == 'circle') {
            //code to account for circle being drawn
            var centroid = TaxiVizUtil.BuildFormattedLatLong(layer.toGeoJSON(), false)[0];
            var radius = layer.getRadius();

            TaxiVizUtil.CircleQueryDisplay(centroid, radius, map);

            drawnItems.addLayer(layer);
            TaxiVizUtil.currentLayers.push(layer);
        }
    });

    $("#toolbar").kendoToolBar({
        items: [
            {type: "seperator"}, 
            { template: "<strong>Start Date: </strong><input type='text' id='startDate'/>" },
            { type: "separator" },
            { template: "<strong>End Date: </strong><input type='text' id='endDate'/>" },
            {type: "seperator" },
            {
                type: "buttonGroup",
                buttons: [
                    { type: "button", group: "pointsToQuery", togglable: true, text: "Query Pickup", attributes: { "value": "0" }, selected: true },
                    { type: "button", group: "pointsToQuery", togglable: true, text: "Query Dropoff", attributes: { "value": "1" } },
                    { type: "button", group: "pointsToQuery", togglable: true, text: "Query Both", attributes: { "value": "2" } }
                ]
            },
           { type: "separator" },
           {
               type: "buttonGroup",
               buttons: [
                   { type: "button", group: "pointsToDisplay", togglable: true, text: "Display Pickup", attributes: { "value": "pickups" }, selected: true },
                   { type: "button", group: "pointsToDisplay", togglable: true, text: "Display Dropoff", attributes: { "value": "dropoffs" } },
                   { type: "button", group: "pointsToDisplay", togglable: true, text: "Display Both", attributes: { "value": "both" } }
               ]
           },
            { type: "separator" },
            {
                type: "splitButton", id: "drawBoroughBtn", text: "Select Boroughs",
                menuButtons: [
                    { type: "button", text: "Staten Island", id: "drawStatenBtn", togglable: true, toggle: function(e) { TaxiVizUtil.ToggleBorough(NewYorkBoroughs.StatenIsland, map, BoroughColors.StatenIsland, "StatenIsland"); }},
                    { type: "button", text: "Manhattan", id: "drawManhatBtn", togglable: true, toggle: function (e) { TaxiVizUtil.ToggleBorough(NewYorkBoroughs.Manhattan, map, BoroughColors.Manhattan, "Manhattan"); } },
                    { type: "button", text: "Bronx", id: "drawBronxBtn", togglable: true, toggle: function (e) { TaxiVizUtil.ToggleBorough(NewYorkBoroughs.Bronx, map, BoroughColors.Bronx, "Bronx"); } },
                    { type: "button", text: "Brooklyn", id: "drawBrookBtn", togglable: true, toggle: function (e) { TaxiVizUtil.ToggleBorough(NewYorkBoroughs.Brooklyn, map, BoroughColors.Brooklyn, "Brooklyn"); } },
                    { type: "button", text: "Queens", id: "drawQueensBtn", togglable: true, toggle: function (e) { TaxiVizUtil.ToggleBorough(NewYorkBoroughs.Queens, map, BoroughColors.Queens, "Queens"); } }
                ]
            },
            { type: "separator" },
            { type: "button", id: "clearLayersBtn", text: "Clear Map", },
            { type: "separator" },
            //{ type: "button", id: "splitViewBtn", text: "Split View" },
            //{ type: "separator" },
            {
                type: "splitButton", text: "Visualizations",
                menuButtons: [
                    { type: "button", text: "Parallel Coordinates", id: "pCoordBtn" },
                    { type: "button", text: "Pie Chart", id: "pChartBtn" },
                    { type: "button", text: "Bar Chart", id: "bChartBtn" }
                ]
            },
           { type: "separator" }
        ]
    });

    // set up the kendo UI datepickers with the date range of the data
    $("#startDate").kendoDateTimePicker({
        min: new Date(2013, 10, 1),
        max: new Date(2013, 10, 30),
        value: new Date(2013, 10, 1, 9, 30, 00)
    });

    $("#endDate").kendoDateTimePicker({
        min: new Date(2013, 10, 1, 00, 00, 00),
        max: new Date(2013, 10, 30, 59, 59, 59),
        value: new Date(2013, 10, 1, 10, 00, 00)
    });

    
    /*$("#drawBoroughBtn").click(function () {
        var count = 0;
        for (var borough in NewYorkBoroughs) {
            TaxiVizUtil.ToggleBorough(NewYorkBoroughs[borough], map, BoroughColors[borough]);
            count++;
        }
    });*/

    $("#clearLayersBtn").click(function () {
        TaxiVizUtil.ClearAllLayers(map);
    });

    $("#pCoordBtn").click(function () {
        TaxiVizUtil.CreateParallelCoordsChart(TaxiVizUtil.currentData);
    });

    $("#bChartBtn").click(function () {
        TaxiVizUtil.DrawBarChart(TaxiVizUtil.currentData);
    });

    $("#pChartBtn").click(function () {
        TaxiVizUtil.DrawPieChart(TaxiVizUtil.currentData);
    });
}