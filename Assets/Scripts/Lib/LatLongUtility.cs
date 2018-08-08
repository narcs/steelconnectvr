using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For promises and REST client.
using RSG;
using Proyecto26;
using Models.Google;

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

    private static string apiKeyParam = "&key=AIzaSyDzaxE2kdHlfAUQ4ZC6nDRDbehcQkVyKwI";
    private static string gmapsAPI = "https://maps.googleapis.com/maps/api/geocode/json?address=";

    // TODO: Actually implement this properly!
    public static IPromise<LatLong> GetLatLongForAddress(string streetAddress, string city, string country) {
        string url = gmapsAPI;

        if (streetAddress != "") {
            url += (streetAddress.Replace(" ", "+") + ",+");
        }

        url += ",+" + city.Replace(" ", "+") + ",+" + country + apiKeyParam;

        return RestClient.Get<Models.Google.Maps.ResultList>(url)
            .Then(resultList => new LatLong(
                resultList.results[0].geometry.location.lat, 
                resultList.results[0].geometry.location.lng));
    }
}

public class LatLong {
    public float latitude;
    public float longitude;

    public LatLong(float latitude, float longitude) {
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public override string ToString() {
        return $"LatLong({latitude}, {longitude})";
    }
}

namespace Models {
    namespace Google {
        namespace Maps {
            [Serializable]
            public class ResultList {
                public Result[] results;
                public string status;
            }

            [Serializable]
            public class Result {
                public AddressComponents[] address_components;
                public string formatted_address;
                public Geometry geometry;
                public string place_id;
                public string[] types;
            }

            [Serializable]
            public class Geometry {
                public Bounds bounds;
                public Location location;
                public string location_type;
                public Viewport viewport;
            }

            [Serializable]
            public class Bounds {
                public Northeast northeast;
                public Southwest southwest;
            }

            [Serializable]
            public class Northeast {
                public float lat;
                public float lng;
            }

            [Serializable]
            public class Southwest {
                public float lat;
                public float lng;
            }

            [Serializable]
            public class Location {
                public float lat;
                public float lng;
            }

            [Serializable]
            public class Viewport {
                public Northeast1 northeast;
                public Southwest1 southwest;
            }

            [Serializable]
            public class Northeast1 {
                public float lat;
                public float lng;
            }

            [Serializable]
            public class Southwest1 {
                public float lat;
                public float lng;
            }

            [Serializable]
            public class AddressComponents {
                public string long_name;
                public string short_name;
                public string[] types;
            }
        }
    }
}