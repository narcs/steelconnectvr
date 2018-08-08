using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For promises and REST client.
using RSG;
using Proyecto26;
using Models.SteelConnect;

// We're using promises/futures here for async operations. See:
// https://github.com/Real-Serious-Games/C-Sharp-Promise
// https://github.com/proyecto26/RestClient/

public class SteelConnect {
    static readonly string
        API_CONFIG = "scm.config",
        API_CONFIG_VERSION = "1.0",
        API_REPORTING = "scm.reporting",
        API_REPORTING_VERSION = "1.0";

    private string username, password, baseUrl, orgId;

    public SteelConnect(string username, string password, string baseUrl, string orgId) {
        this.username = username;
        this.password = password;
        this.baseUrl = baseUrl;
        this.orgId = orgId;
    }

    // For easy testing.
    public SteelConnect() : this(
        "ayabb1@student.monash.edu",
        "vt55SNpha8xub7pRgVmy",
        "monash.riverbed.cc",
        "org-Monash-9a986912d67e72e2") {

    }

    // ---

    static string makeAuthorizationHeaderContent(string username, string password) {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }

    string getConfigApiUrl() {
        return $"https://{baseUrl}/api/{API_CONFIG}/{API_CONFIG_VERSION}";
    }

    string getReportingApiUrl() {
        return $"https://{baseUrl}/api/{API_REPORTING}/{API_REPORTING_VERSION}";
    }

    RequestHelper newRequest(string uri) {
        Debug.Log($"New API request: {uri}");

        return new RequestHelper {
            Uri = uri,
            Headers = new Dictionary<string, string>
            {
                { "Authorization", makeAuthorizationHeaderContent(username, password) },
                { "UserAgent", $"{Application.productName} {Application.version}" }
            }
        };
    }

    RequestHelper newConfigRequest(string path) {
        return newRequest(getConfigApiUrl() + path);
    }

    RequestHelper newReportingRequest(string path) {
        return newRequest(getReportingApiUrl() + path);
    }

    // ---

    public IPromise<Sites> GetSitesInOrg() {
        return RestClient.Get<Sites>(newConfigRequest("/org/" + orgId + "/sites"));
    }

    // TODO: How to deal with 404 being a legitimate response (for no sitelinks)?
    public IPromise<Sitelinks> GetSitelinks(string siteId) {
        return RestClient.Get<Sitelinks>(newReportingRequest("/site/" + siteId + "/sitelinks"));
    }
}

// Received JSON gets deserialized into these types.
namespace Models {
    namespace SteelConnect {
        [Serializable]
        public class Sites {
            public Site[] items;
        }

        [Serializable]
        public class Site {
            public string id;
            public string name;
            public string longname;
            public string org;
            public string country;
            public string city;
            public string street_address;
        }

        [Serializable]
        public class Sitelinks {
            public Sitelink[] items;
        }

        [Serializable]
        public class Sitelink {
            public string id;
            public string remote_site;
        }
    }

}