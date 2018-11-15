using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For promises and REST client.
using RSG;
using Proyecto26;

// Mapbox
using Mapbox.Geocoding;
using Mapbox.Unity;
using Mapbox.Utils;

// We're using promises/futures here for async operations. See:
// https://github.com/Real-Serious-Games/C-Sharp-Promise
// https://github.com/proyecto26/RestClient/

public static class LatLongUtility {
    public static Vector3 LatLongToCartesian(float latitude, float longitude, float sphereRadius) {
        float latitudeRadians = latitude * (Mathf.PI / 180);
        float longitudeRadians = (longitude * -1) * (Mathf.PI / 180);

        Vector3 cartVector = new Vector3();
        cartVector.z = sphereRadius * Mathf.Cos(latitudeRadians) * Mathf.Cos(longitudeRadians);
        cartVector.x = sphereRadius * Mathf.Cos(latitudeRadians) * Mathf.Sin(longitudeRadians);
        cartVector.y = sphereRadius * Mathf.Sin(latitudeRadians);

        return cartVector;
    }

    public static Vector3 LatLongToCartesian(LatLong latLong, float sphereRadius) {
        return LatLongToCartesian(latLong.latitude, latLong.longitude, sphereRadius);
    }

    // ---

    public static IPromise<LatLong> GetLatLongForAddress(string streetAddress, string city, string country) {
        // Get access singleton (object that holds a token and associated geocoder).
        MapboxAccess access = MapboxAccess.Instance;
        Geocoder geocoder = access.Geocoder;

        var promise = new Promise<LatLong>();

        string forwardGeocodeQuery = $"{streetAddress} {city} {country}";
        Debug.Log($"Performing forward geocode with query \"{forwardGeocodeQuery}\"");
        ForwardGeocodeResource forwardGeocode = new ForwardGeocodeResource(forwardGeocodeQuery);

        geocoder.Geocode(forwardGeocode, (ForwardGeocodeResponse response) => {
            /*
            Debug.Log($"Forward geocode response has features:");
            foreach (Feature feature in response.Features) {
                Debug.Log($" - {feature.Id} {feature.PlaceName} {feature.Address} {feature.Center}");
            }
            */

            // There are lots of interesting types of features returned by the API, places, POIs, localities, etc.
            // I'm assuming they're ordered by relevance, so let's just pick the first one.
            if (response.Features.Count > 0) {
                Feature chosenFeature = response.Features[0];
                Vector2d coordinates = chosenFeature.Center;
                promise.Resolve(new LatLong((float)coordinates.x, (float)coordinates.y));
            } else {
                promise.Resolve(new LatLong());
            }
        });

        return promise;
    }
}

public class LatLong {
    public float latitude;
    public float longitude;
    public bool isValid;

    public LatLong(float latitude, float longitude) {
        this.latitude = latitude;
        this.longitude = longitude;
        isValid = true;
    }

    // Error case.
    public LatLong() {
        this.latitude = 0.0f;
        this.longitude = 0.0f;
        isValid = false;
    }

    public override string ToString() {
        return $"LatLong(latitude = {latitude}, longitude = {longitude})";
    }
}