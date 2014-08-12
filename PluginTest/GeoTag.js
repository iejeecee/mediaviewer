var ge;
var geocoder;
var placemark = new Array;
var dragInfo;
var nextPlaceMarkId = 0;
var styledPlaceMarkId = -1;

google.load("earth", "1");

function init() {

    geocoder = new google.maps.Geocoder();
    google.earth.createInstance('map3d', initCallback, failureCallback);
}

function initCallback(instance) {

    ge = instance;
    ge.getWindow().setVisibility(true);

    // add a navigation control
    ge.getNavigationControl().setVisibility(ge.VISIBILITY_AUTO);

    // add some layers
    ge.getLayerRoot().enableLayerById(ge.LAYER_BORDERS, true);
    ge.getLayerRoot().enableLayerById(ge.LAYER_ROADS, true);

    window.external.initialize();

    // listen for mousedown on the window (look specifically for point placemarks)
    google.earth.addEventListener(ge.getWindow(), 'mousedown', function (event) {

        if (event.getTarget().getType() == 'KmlPlacemark' &&
            event.getTarget().getGeometry().getType() == 'KmlPoint') {
            //event.preventDefault();
            var placemark = event.getTarget();

            dragInfo = {
                placemark: event.getTarget(),
                dragged: false
            };
        }
    });

    // listen for mousemove on the globe
    google.earth.addEventListener(ge.getGlobe(), 'mousemove', function (event) {

        if (dragInfo) {

            event.preventDefault();
            var point = dragInfo.placemark.getGeometry();
            point.setLatitude(event.getLatitude());
            point.setLongitude(event.getLongitude());
            dragInfo.dragged = true;

            var i = parseInt(dragInfo.placemark.getId());

            window.external.placeMarkMoved(i, event.getLatitude(), event.getLongitude());
        }
    });

    // listen for mouseup on the window
    google.earth.addEventListener(ge.getWindow(), 'mouseup', function (event) {

        if (dragInfo) {
            if (dragInfo.dragged) {
                // if the placemark was dragged, prevent balloons from popping up
                event.preventDefault();

                var i = parseInt(dragInfo.placemark.getId());

                window.external.endPlaceMarkMoved(i);
            }

            dragInfo = null;
        }
    });
}

function createPlaceMark(latitude, longitude, placemarkName, imagePath, imageName) {

    var i = placemark.length;

    // create the placemark with id = i
    placemark[i] = ge.createPlacemark(nextPlaceMarkId.toString());

    var point = ge.createPoint('');
    point.setLatitude(latitude);
    point.setLongitude(longitude);
    placemark[i].setGeometry(point);

    // add the placemark to the earth DOM
    ge.getFeatures().appendChild(placemark[i]);

    placemark[i].setName(placemarkName);

    google.earth.addEventListener(placemark[i], 'click', function (event) {

        // prevent the default balloon from popping up
        event.preventDefault();
        /*
        var balloon = ge.createHtmlStringBalloon('');
        balloon.setFeature(event.getTarget());
        balloon.setMaxWidth(300);

        // Google logo.
        balloon.setContentString(
             '<img src="' + imagePath + '" alt="' + imageName + '" width="256">');

        ge.setBalloon(balloon);
        */
        var placeMarkId = parseInt(event.getTarget().getId());
        //stylePlaceMark(placeMarkId);
        window.external.placeMarkClicked(placeMarkId);
    });

    nextPlaceMarkId++;

    return nextPlaceMarkId - 1;

}

function deletePlaceMark(placeMarkId) {

    for (var i = 0; i < placemark.length; i++) {

        if (placemark[i].getId() == placeMarkId.toString()) {

            ge.getFeatures().removeChild(placemark[i]);
            placemark.splice(i, 1);
        }
    }
}

function lookAt(latitude, longitude, range) {

    var la = ge.createLookAt('');
    la.set(latitude, longitude,
        0, // altitude
        ge.ALTITUDE_RELATIVE_TO_GROUND,
        0, // heading
        0, // straight-down tilt
        range // range (inverse of zoom)
    );

    ge.getView().setAbstractView(la);

}

function lookAtQuery(query) {

    // bias results towards the current viewport
    var latLngBox = ge.getView().getViewportGlobeBounds();
    var sw = new google.maps.LatLng(latLngBox.getSouth(), latLngBox.getWest());
    var ne = new google.maps.LatLng(latLngBox.getNorth(), latLngBox.getEast());

    var bounds = new google.maps.LatLngBounds(sw, ne);

    geocoder.geocode({ 'address': query, 'bounds': bounds }, function (results, status) {

        if (status == google.maps.GeocoderStatus.OK) {

            if (results[0]) {

                var coord = results[0].geometry.location;
                lookAt(coord.lat(), coord.lng(), 2000);

            }

        } else if (status == google.maps.GeocoderStatus.ZERO_RESULTS) {

            failureCallback('Location not found: ' + query);

        } else {

            failureCallback('Geocoder failed due to: ' + status);
        }
    });
}

function getPlaceMark(placeMarkId) {

    for (var i = 0; i < placemark.length; i++) {

        if (placemark[i].getId() == placeMarkId.toString()) {

            return (placemark[i]);
        }
    }

    return (null);
}

function lookAtPlaceMark(placeMarkId) {

    var point = getPlaceMark(placeMarkId).getGeometry();

    // look at the placemark we created
    lookAt(point.getLatitude(), point.getLongitude(), 500);

}

function getViewPortCenter() {

    var latLngBox = ge.getView().getViewportGlobeBounds();
    var lat = (latLngBox.getNorth() + latLngBox.getSouth()) / 2.0;
    var lng = (latLngBox.getWest() + latLngBox.getEast()) / 2.0;

    //var center = new google.maps.LatLng(lat, lng);

    return lat.toString() + ' ' + lng.toString();
}

function reverseGeoCodePlaceMark(placeMarkId) {

    var point = getPlaceMark(placeMarkId).getGeometry();

    var latlng = new google.maps.LatLng(point.getLatitude(), point.getLongitude());

    geocoder.geocode({ 'location': latlng }, function (results, status) {

        if (status == google.maps.GeocoderStatus.OK) {

            if (results[0]) {

                window.external.addressUpdate(results[0].formatted_address);

            }

        } else if (status == google.maps.GeocoderStatus.ZERO_RESULTS) {

            window.external.addressUpdate('unknown address');

        } else {

            failureCallback('Geocoder failed due to: ' + status);
        }
    });

}

function stylePlaceMark(placeMarkId) {

    if (placeMarkId == styledPlaceMarkId) return;

    var styleMap = ge.createStyleMap('');

    // Create selected style for style map
    var selectedStyle = ge.createStyle('');
    var selectedIcon = ge.createIcon('');
    selectedIcon.setHref('http://maps.google.com/mapfiles/kml/pushpin/red-pushpin.png');
    selectedStyle.getIconStyle().setIcon(selectedIcon);

    styleMap.setNormalStyle(selectedStyle);

    var selectedPlaceMark = getPlaceMark(placeMarkId);
    selectedPlaceMark.setStyleSelector(styleMap);

    if (styledPlaceMarkId != -1) {

        var deselectedPlaceMark = getPlaceMark(styledPlaceMarkId);

        if (deselectedPlaceMark == null) return;

        var styleMap = ge.createStyleMap('');

        // Create normal style for style map
        var normalStyle = ge.createStyle('');
        var normalIcon = ge.createIcon('');
        normalIcon.setHref('http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png');
        normalStyle.getIconStyle().setIcon(normalIcon);

        styleMap.setNormalStyle(normalStyle);

        deselectedPlaceMark.setStyleSelector(styleMap);
    }

    styledPlaceMarkId = placeMarkId;
}

function setBuildings(isVisible, isLowRes) {

    if (isLowRes == true) {

        ge.getLayerRoot().enableLayerById(ge.LAYER_BUILDINGS_LOW_RESOLUTION, isVisible);

    } else {

        ge.getLayerRoot().enableLayerById(ge.LAYER_BUILDINGS, isVisible);
    }

}

function setRoads(isVisible) {

    ge.getLayerRoot().enableLayerById(ge.LAYER_ROADS, isVisible);

}

function setBorders(isVisible) {

    ge.getLayerRoot().enableLayerById(ge.LAYER_BORDERS, isVisible);
}

function setTerrain(isVisible) {

    ge.getLayerRoot().enableLayerById(ge.LAYER_TERRAIN, isVisible);
}


function failureCallback(errorCode) {

    window.external.failure(errorCode);
}
