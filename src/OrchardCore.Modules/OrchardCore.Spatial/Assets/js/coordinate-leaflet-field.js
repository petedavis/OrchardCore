function addMapPicker() {

    var lat = $('[data-latitude]').val();
    var long = $('[data-longitude]').val();

    var mapCenter = [40.866667, 34.566667];
    var zoom = 0;

    if (lat && long) {
        mapCenter = [lat, long];
        zoom = 14;
    }

    var map = L.map('map', { center: mapCenter, zoom: zoom });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);
    var marker = L.marker(mapCenter).addTo(map);

    function updateMarker(lat, lng) {
        marker
            .setLatLng([lat, lng])
            .bindPopup("Location :  " + marker.getLatLng().toString())
            .openPopup();
        return false;
    };

    map.on('click', function (e) {
        $('[data-latitude]').val(e.latlng.lat);
        $('[data-longitude]').val(e.latlng.lng);
        updateMarker(e.latlng.lat, e.latlng.lng);
    });


    var updateMarkerByInputs = function () {
        return updateMarker($('[data-latitude]').val(), $('[data-longitude]').val());
    }
    $('[data-latitude]').on('input', updateMarkerByInputs);
    $('[data-longitude]').on('input', updateMarkerByInputs);
}

$(document).ready(function () {
    addMapPicker();
});
