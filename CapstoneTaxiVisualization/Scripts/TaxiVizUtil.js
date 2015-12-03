var TaxiVizUtil = {
    currentId: 0,
    pointsDisplayed: 0,
    boroughs: {},
    currentLayers: [],
    currentData: {},
    isDualSelect: false,
    dualSelectLayer: null,
    //function that will call the server to execute the stored proc
    //for the region query display
    RegionQueryDisplay: function (data, map) {
        var toolbar = $("#toolbar").data("kendoToolBar");
        var selected = toolbar.getSelectedFromGroup("pointsToQuery");

        $.ajax({
            type: "POST",
            url: window.location.protocol + "//" + window.location.hostname + "/CapstoneTaxiVisualization/RegionQuery",
            data: {
                startDate: $("#startDate").val(),
                endDate: $("#endDate").val(),
                boundPoints: data,
                queryFor: Number(selected.attr("value"))
            },
            success: function (response) {
                var parsedJson = JSON.parse(response);
                TaxiVizUtil.currentData = parsedJson;
                TaxiVizUtil.DrawCircles(parsedJson, map);
                TaxiVizUtil.CreateFloatingInfo(parsedJson);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                window.alert(xhr.responseText)
            }
        });
    },

    CircleQueryDisplay: function (centroid, radius, map) {
        var toolbar = $("#toolbar").data("kendoToolBar");
        var selected = toolbar.getSelectedFromGroup("pointsToQuery");

        $.ajax({
            type: "POST",
            url: window.location.protocol + "//" + window.location.hostname + "/CapstoneTaxiVisualization/Home/GetPointsInCircleRegion",
            data: {
                startDate: $("#startDate").val(),
                endDate: $("#endDate").val(),
                radius: radius,
                centroid: centroid,
                queryFor: Number(selected.attr("value"))
            },
            success: function (response) {
                var parsedJson = JSON.parse(response);
                TaxiVizUtil.currentData = parsedJson;
                TaxiVizUtil.DrawCircles(parsedJson, map);
                TaxiVizUtil.CreateFloatingInfo(parsedJson);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                window.alert(xhr.responseText)
            }
        });
    },

    LineIntersectionDisplay: function (data, map) {
        $.ajax({
            type: "POST",
            url: window.location.protocol + "//" + window.location.hostname + "/CapstoneTaxiVisualization/Home/GetTripIntersectionByLine",
            data: {
                startDate: $("#startDate").val(),
                endDate: $("#endDate").val(),
                linePoints: data
            },
            success: function (response) {
                var parsedJson = JSON.parse(response);
                TaxiVizUtil.currentData = parsedJson;
                TaxiVizUtil.DrawCircles(parsedJson, map);
                TaxiVizUtil.CreateFloatingInfo(parsedJson);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                window.alert(xhr.responseText)
            }
        });
    },

    NearestPointQueryDisplay: function (data, map) {
        var toolbar = $("#toolbar").data("kendoToolBar");
        var selected = toolbar.getSelectedFromGroup("pointsToQuery");

        $.ajax({
            type: "POST",
            url: window.location.protocol + "//" + window.location.hostname + "/CapstoneTaxiVisualization/Home/GetNearestPoint",
            data: {
                startDate: $("#startDate").val(),
                endDate: $("#endDate").val(),
                point: data[0],
                queryFor: Number(selected.attr("value"))
            },
            success: function (response) {
                var parsedJson = JSON.parse(response);
                TaxiVizUtil.currentData = parsedJson;
                TaxiVizUtil.DrawCircles(parsedJson, map);
                TaxiVizUtil.CreateFloatingInfo(parsedJson);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                window.alert(xhr.responseText)
            }
        });
    },

    DualRegionDisplay: function(regionOne, regionTwo, map) {
        $.ajax({
            type: "POST",
            url: window.location.protocol + "//" + window.location.hostname + "/CapstoneTaxiVisualization/Home/DualRegionQuery",
            data: {
                startDate: $("#startDate").val(),
                endDate: $("#endDate").val(),
                regionOnePoints: regionOne,
                regionTwoPoints: regionTwo
            },
            success: function (response) {
                var parsedJson = JSON.parse(response);
                TaxiVizUtil.currentData = parsedJson;
                TaxiVizUtil.DrawCircles(parsedJson, map);
                TaxiVizUtil.CreateFloatingInfo(parsedJson);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                window.alert(xhr.responseText)
            }
        });
    },

    //takes the GeoJson object and makes sure the points
    //follow left hand rule so SQL can spatially query them
    BuildFormattedLatLong: function (data, rewind) {
        var returnVal = [];

        if (rewind) {
            data = geojsonRewind(data, true);
        }

        if (data.geometry.type == "Polygon") {
            data.geometry.coordinates[0].forEach(function (instance) {
                returnVal.push({ "Latitude": instance[1], "Longitude": instance[0] });
            });
        }
        else if (data.geometry.type == "Point") {
            returnVal.push({ "Latitude": data.geometry.coordinates[1], "Longitude": data.geometry.coordinates[0] })
        }
        else if (data.geometry.type == "LineString") {
            data.geometry.coordinates.forEach(function (instance) {
                returnVal.push({ "Latitude": instance[1], "Longitude": instance[0] });
            });
        }

        return returnVal;
    },

    //takes the data from the server and will draw circles using d3 on the map for them
    //and create the ability to bring up a tooltip with important data
    DrawCircles: function (data, map) {
        /* Initialize the SVG layer */
        map._initPathRoot();
        /* We simply pick up the SVG from the map object */
        var svg = d3.select("#map").select("svg");

        //make a copy of the data where one will have the pickup locations as leaflet latlng objects
        //and the other will have the dropoffs
        var pickupData = data;
        var dropoffData = $.extend(true, [], data);

        var toolbar = $("#toolbar").data("kendoToolBar");
        var selected = toolbar.getSelectedFromGroup("pointsToDisplay");

        var toDisplay = selected.attr("value");

        if (toDisplay == "pickups" || toDisplay == "both") {
            /* Add a LatLng object to each item in the dataset */
            pickupData.forEach(function (d) {
                d.LatLng = new L.LatLng(d.Pickup.Latitude,
                                        d.Pickup.Longitude)

            });

            var pickupG = svg.append("g");
            var pickupFeatures = TaxiVizUtil.CreatePointsOnMap(pickupG, pickupData, "red", map)
        }

        if (toDisplay == "dropoffs" || toDisplay == "both") {
            dropoffData.forEach(function (d) {
                d.LatLng = new L.LatLng(d.Dropoff.Latitude,
                                        d.Dropoff.Longitude)

            });

            var dropoffG = svg.append("g");
            var dropoffFeatures = TaxiVizUtil.CreatePointsOnMap(dropoffG, dropoffData, "blue", map)
        }

        //set the function to redraw the points as the 
        //map is zoomed in or out
        map.on("viewreset", update);
        update();
        function update() {
            if (typeof pickupFeatures != "undefined" && pickupFeatures != null) {
                pickupFeatures.attr("transform",
                function (d) {
                    return "translate(" +
                        map.latLngToLayerPoint(d.LatLng).x + "," +
                        map.latLngToLayerPoint(d.LatLng).y + ")";
                });
            }
            if (typeof dropoffFeatures != "undefined" && dropoffFeatures != null) {
                dropoffFeatures.attr("transform",
                function (d) {
                    return "translate(" +
                        map.latLngToLayerPoint(d.LatLng).x + "," +
                        map.latLngToLayerPoint(d.LatLng).y + ")";
                });
            }
        }
    },

    //build a floating information box in the bottom left hand corner 
    //that contains general information about the most recent query
    CreateFloatingInfo: function (data) {
        //find out the average fare
        var sumFare = 0, sumTime = 0, sumDist = 0;

        data.forEach(function (d) {
            sumFare += Number(d.FareTotal);
            sumTime += Number(d.TravelTime);
            sumDist += Number(d.TripDistance);
        });

        var avgFare = sumFare / data.length;
        var avgTime = sumTime / data.length;
        var avgDist = sumDist / data.length;

        var htmlContent = "<strong>Trips Returned: </strong><span> " + data.length + "</span><br/>" +
                            "<strong>Average Fare:</strong><span> $" + Math.round(avgFare * 100) / 100 + "</span><br/>" +
                            "<strong>Average Time:</strong><span> " + Math.round((avgTime / 60) * 100) / 100 + " minutes</span><br/>" +
                            "<strong>Average Distance:</strong><span> " + Math.round(avgDist * 100) / 100 + " miles</span><br/>";

        //grab the popup element to place the content in, and if the 
        //kendoWindow data object does not exist within it, set it 
        var window = $("#quickInfo");

        if (!window.data("kendoWindow")) {
            window.kendoWindow({
                title: "Most Recent Selection:"
            });
        }

        //open the window and set the content to that defined above for the element
        window.data("kendoWindow").open();
        window.html(htmlContent);

        //get the position of the bottom left of the map to place this window on it
        var offset = $("#map").offset();
        var test = $("#map").height();

        //set the position of the window to be that of the point selected
        window.parent().css("position", "fixed");
        window.parent().css("top", $("#map").height() - window.height());
        window.parent().css("left", 0);
    },

    ClearAllLayers: function (map) {
        //remove all of the layers and pop them off of the current layers
        while (TaxiVizUtil.currentLayers.length > 0) {
            map.removeLayer(TaxiVizUtil.currentLayers.pop());
        }

        d3.selectAll("circle").remove();
    },

    CreatePointsOnMap: function (g, data, color, map) {
        var feature = g.selectAll("circle")
            .data(data)
            .enter().append("circle")
            .style("stroke", "black")
            .style("opacity", .6)
            .style("fill", color)
            .attr("id", function (d, i) {
                //create the point based on the current number of used ids
                var id = 'point' + TaxiVizUtil.currentId++;

                //add the point as a property of the element and then return it to be 
                //applied to the html circle created
                d.pointId = id;
                return id;
            })
            .attr("r", 5)
            .on("click", function (element) {
                var htmlContent = "<strong>Distance: </strong><span> " + Math.round(element.TripDistance * 100) / 100 + " miles</span><br/>" +
                                   "<strong>Fare:</strong><span> $" + Math.round(element.FareTotal * 100) / 100 + "</span><br/>" +
                                        "<strong>Time:</strong><span> " + Math.round((element.TravelTime / 60) * 100) / 100 + " minutes</span>";

                //grab the popup element to place the content in, and if the 
                //kendoWindow data object does not exist within it, set it 
                var window = $("#popup");

                if (!window.data("kendoWindow")) {
                    window.kendoWindow({
                        title: "Trip Totals:"
                    });
                }

                //grab the current point and its place on the page
                var point = $("#" + element.pointId);
                var offset = point.offset();

                //set the position of the window to be that of the point selected
                window.parent().css("top", offset.top);
                window.parent().css("left", offset.left);

                //open the window and set the content to that defined above for the element
                window.data("kendoWindow").open();
                window.html(htmlContent);
            });

        return feature;
    },

    ToggleBorough: function (points, map, shapeColor, name) {
        if (TaxiVizUtil.boroughs.hasOwnProperty(name)) {
            //remove the layer from the map and remove the property from the object
            map.removeLayer(TaxiVizUtil.boroughs[name]);
            delete TaxiVizUtil.boroughs[name];
        }
        else {
            //place to collect the vertices created from the latitude and longitude points
            var vertices = [];

            //create the leaflet latlng objects for each points
            points.forEach(function (ele) {
                vertices.push(new L.LatLng(ele.Latitude, ele.Longitude));
            })

            //add the polygon to the map
            var polyLayer = L.polygon(vertices, { color: shapeColor });

            polyLayer.on("click", function (e) {
                var boundPoints = TaxiVizUtil.BuildFormattedLatLong(this.toGeoJSON(), true);

               // TaxiVizUtil.RegionQueryDisplay(boundPoints, map);
            });

            polyLayer.addTo(map);

            //keep track of the layer to remove it
            TaxiVizUtil.boroughs[name] = polyLayer;
        }
    },

    CreateParallelCoordsChart: function (data) {
        //create the popup window
        var popupHeight = Number($(document).height()) * 0.8;
        var popupWidth = Number($(document).width()) * 0.7;

        $("#mainContent").append("<div id='parcoords' class='parcoords'></div>");

        //create color scale
        var colorScale = d3.scale.linear()
  			.domain([0, 100])
  			.range(["Goldenrod", "Green"])
  			.interpolate(d3.interpolateLab);

        //this uses the parcoords javascript library
        //to generate the graph with the ncessary functions
        var pc = d3.parcoords()("#parcoords")
            .data(data)
            .hideAxis(["Pickup"])
            .hideAxis(["Dropoff"])
            .hideAxis(["LatLng"])
            .hideAxis(["pointId"])
            .height(popupHeight - 50)
            .width(popupWidth - 50)
            .autoscale()
            .color(function (d) { return colorScale(d['FareTotal']); })
            .alpha(0.35)
            .render()
            .ticks(10)
            .createAxes()
            .brushMode("1D-axes")
            .interactive()

        var exploreCount = 0;
        var exploring = {};
        var exploreStart = false;

        //the following function and this object method call involve the drag and drop functionality
        //of the graph so you may better explore the data
        pc.svg
            .selectAll(".dimension")
            .style("cursor", "pointer")
            .on("click", function (d) {
                exploring[d] = d in exploring ? false : true;
                event.preventDefault();
                if (exploring[d]) d3.timer(explore(d, exploreCount));
            });

        function explore(dimension, count) {
            if (!exploreStart) {
                exploreStart = true;
                d3.timer(pc.brush);
            }

            var speed = (Math.round(Math.random()) ? 1 : -1) * (Math.random() + 0.5);

            return function (t) {
                if (!exploring[dimension]) return true;
                var domain = pc.yscale[dimension].domain();
                var width = (domain[1] - domain[0]) / 4;
                var center = width * 1.5 * (1 + Math.sin(speed * t / 1200)) + domain[0];

                pc.yscale[dimension].brush.extent([
                    d3.max([center - width * 0.01, domain[0] - width / 400]),
                    d3.min([center + width * 1.01, domain[1] + width / 100])
                ])(pc.g()
                    .filter(function (d) {
                        return d == dimension;
                    })
                );
            };
        }

        var window = $("#parcoords");

        //if the window doesn't actually exist, let's create it
        if (!window.data("kendoWindow")) {
            window.kendoWindow({
                title: "Parallel Coordinates:",
                height: popupHeight,
                width: popupWidth,
                close: function (e) {
                    $(this.element).remove();
                },
                resize: function (e) {
                    pc.height(Number(this._size.height));
                    pc.width(Number(this._size.width));
                    pc.data(data);
                }
            });
        }

        //open the window and set the content to that defined above for the element
        window.data("kendoWindow").center().open();
    },

    DrawPieChart: function (data) {
        $("#mainContent").append("<div id='pieChart'></div>");

        //create the popup window
        var window = $("#pieChart");

        //if the window doesn't actually exist, let's create it
        if (!window.data("kendoWindow")) {
            window.kendoWindow({
                title: "Pie Chart:",
                close: function (e) {
                    $(this.element).remove();
                }
            });
        }

        //open the window
        window.data("kendoWindow").center().open();

        //BILL WRITE CODE HERE
        var thisIsYourChart = $("#pieChart");

        var width = 960,
		    height = 500,
		    radius = Math.min(width, height) / 2;

        var color = d3.scale.ordinal()
		    .range(["#98abc5", "#8a89a6", "#7b6888", "#6b486b", "#a05d56", "#d0743c", "#ff8c00"]);

        var arc = d3.svg.arc()
		    .outerRadius(radius - 10)
		    .innerRadius(0);

        var pie = d3.layout.pie()
		    .sort(null)
		    .value(function (d) { return d.population; });

        var svg = d3.select("#pieChart").append("svg")
		    .attr("width", width)
		    .attr("height", height)
		  .append("g")
		    .attr("transform", "translate(" + width / 2 + "," + height / 2 + ")");

        d3.csv("Scripts/data.csv", function (error, data) {

            data.forEach(function (d) {
                d.population = +d.population;
            });

            var g = svg.selectAll(".arc")
                .data(pie(data))
              .enter().append("g")
                .attr("class", "arc");

            g.append("path")
                .attr("d", arc)
                .style("fill", function (d) { return color(d.data.age); });

            g.append("text")
                .attr("transform", function (d) { return "translate(" + arc.centroid(d) + ")"; })
                .attr("dy", ".35em")
                .style("text-anchor", "middle")
                .text(function (d) { return d.data.age; });

        });
    },

    DrawBarChart: function (data) {
        //create the popup window
        var window = $("#barChart");

        //if the window doesn't actually exist, let's create it
        if (!window.data("kendoWindow")) {
            window.kendoWindow({
                title: "Bar Chart:"
            });
        }

        //open the window
        window.data("kendoWindow").center().open();


        var data = [4, 8, 15, 16, 23, 42];
        console.log(TaxiVizUtil.currentData);


        var width = 420;
        var barHeight = 20;

        var x = d3.scale.linear()
            .domain([0, d3.max(data)])
            .range([0, width]);

        var chart = d3.select("#barChart")
            .attr("width", width)
            .attr("height", barHeight * data.length);

        var bar = chart.selectAll("g")
            .data(data)
            .enter().append("g")
            .attr("transform", function (d, i) { return "translate(0," + i * barHeight + ")"; });

        bar.append("rect")
            .attr("width", x)
            .attr("height", barHeight - 1);

        bar.append("text")
            .attr("x", function (d) { return x(d) - 3; })
            .attr("y", barHeight / 2)
            .attr("dy", ".35em")
            .text(function (d) { return d; });

        window.data("kendoWindow").content(chart.html);

    },

    ToggleDualSelect: function () {
        TaxiVizUtil.isDualSelect = !TaxiVizUtil.isDualSelect;
    },

    DrawParallelCoords: function () {

    }
}