function InitializeInterface() {
    $("#toolbar").kendoToolBar({
        items: [
            { type: "button", id: "clearLayersBtn", text: "Clear Map" },
            { type: "separator" },
            {
                type: "splitButton", id: "drawBoroughBtn", text: "View Boroughs",
                menuButtons: [
                    { text: "Staten Island", id: "drawStatenBtn" },
                    { text: "Manhattan", id: "drawManhatBtn" },
                    { text: "Bronx", id: "drawBronxBtn" },
                    { text: "Brooklyn", id: "drawBrookBtn" },
                    { text: "Queens", id: "drawQueensBtn" }
                ]
            },
            { type: "separator" },
            { type: "button", id: "splitViewBtn", text: "Split View" },
            { type: "separator" },
            {
                type: "splitButton", text: "Visualizations",
                menuButtons: [
                    { text: "Chart 1", icon: "insert-n" },
                    { text: "Chart 2", icon: "insert-m" },
                    { text: "Chart 3", icon: "insert-s" }
                ]
            },
            { type: "separator" },
            { template: "<strong>Start Date: </strong><input type='text' id='startDate'/>" },
            { type: "separator" },
            { template: "<strong>End Date: </strong><input type='text' id='endDate'/>" },
            { type: "separator" },
           {
               template: "<strong>Query Points: </strong>" +
               "<input type='radio' name='pointsToQuery' value='0' id='pickupQuery' class='k-radio' checked/><label class='k-radio-label' for='pickupQuery'>Pickup</label>" +
               "<input type='radio' name='pointsToQuery' value='1' id='dropoffQuery' class='k-radio'/><label class='k-radio-label' for='dropoffQuery'> Dropoff</label>" +
               "<input type='radio' name='pointsToQuery' value='2' id='bothQuery' class='k-radio'/><label class='k-radio-label' for='bothQuery'>Both </label>"
           },
           { type: "separator" },
           {
               template: "<strong>Display Points: </strong>" +
                 "<input type='radio' name='pointsToDisplay' value='pickups' id='pickupDisplay' class='k-radio' checked/><label class='k-radio-label' for='pickupDisplay'>Pickup</label>" +
                 "<input type='radio' name='pointsToDisplay' value='dropoffs' id='dropoffDisplay' class='k-radio'/><label class='k-radio-label' for='dropoffDisplay'> Dropoff</label>" +
                 "<input type='radio' name='pointsToDisplay' value='both' id='bothDisplay' class='k-radio'/><label class='k-radio-label' for='bothDisplay'>Both </label>"
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
            var correctedPoints = TaxiVizUtil.BuildFormattedLatLong(geoJson);

            TaxiVizUtil.RegionQueryDisplay(correctedPoints, map);

            drawnItems.addLayer(layer);
            TaxiVizUtil.currentLayers.push(layer);
        }
        else if (type == 'polyline') {
            //code to account for line being drawn
            //alert("Tried to draw a polyline, tsk tsk not implemented yet!");
            var test = layer.toGeoJSON();
            drawnItems.addLayer(layer);
            //layer.remove();
        }
        else if (type == 'circle') {
            //code to account for circle being drawn
            // alert("Tried to draw a circle, tsk tsk not implemented yet!");
            var test2 = layer.toGeoJSON();
            var test3 = layer.getRadius();
            drawnItems.addLayer(layer);
            //layer.remove();
        }
    });

    $("#drawBoroughBtn").click(function () {
        var count = 0;
        for (var borough in NewYorkBoroughs) {
            TaxiVizUtil.DrawBorough(NewYorkBoroughs[borough], map, BoroughColors[borough]);
            count++;
        }
    });

    $("#drawStatenBtn").click(function () {
        TaxiVizUtil.DrawBorough(NewYorkBoroughs.StatenIsland, map, BoroughColors.StatenIsland);
    });

    $("#drawManhatBtn").click(function () {
        TaxiVizUtil.DrawBorough(NewYorkBoroughs.Manhattan, map, BoroughColors.Manhattan);
    });

    $("#drawBronxBtn").click(function () {
        TaxiVizUtil.DrawBorough(NewYorkBoroughs.Bronx, map, BoroughColors.Bronx);
    });

    $("#drawBrookBtn").click(function () {
        TaxiVizUtil.DrawBorough(NewYorkBoroughs.Brooklyn, map, BoroughColors.Brooklyn);
    });

    $("#drawQueensBtn").click(function () {
        TaxiVizUtil.DrawBorough(NewYorkBoroughs.Queens, map, BoroughColors.Queens);
    });

    $("#clearLayersBtn").click(function () {
        TaxiVizUtil.ClearAllLayers(map);
    });
}