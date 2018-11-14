using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

// For promises and REST client.
// We're using promises/futures here for async operations. See:
// https://github.com/Real-Serious-Games/C-Sharp-Promise
// https://github.com/proyecto26/RestClient/
using RSG;
using Proyecto26;
using Models.SteelConnect;

// These are just to make it clearer what each function or type expects.
// These aliases are all just equivalent to `string`, and so you can pass strings with no problems.
using OrgId = System.String;
using SiteId = System.String;
using WanId = System.String;
using UplinkId = System.String;
using SitelinkId = System.String;

public class SteelConnect {
    static readonly string
        API_CONFIG = "scm.config",
        API_CONFIG_VERSION = "1.0",
        API_REPORTING = "scm.reporting",
        API_REPORTING_VERSION = "1.0";

    private string username, password, baseUrl;
    private OrgId orgId;

    public SteelConnect(string username, string password, string baseUrl, OrgId orgId) {
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

    public IPromise<SiteItems> GetSitesInOrg() {
        return RestClient.Get<SiteItems>(newConfigRequest("/org/" + orgId + "/sites"));
    }

    public IPromise<ResponseHelper> DeleteSite(SiteId siteId) {
        return RestClient.Delete(newConfigRequest("/site/" + siteId));
    }

    private IPromise<ResponseHelper> CreatePost(string entity, RequestHelper request) {
        var promise = new Promise<ResponseHelper>();

        RestClient.Post(request, (err, resp) => {
            if (err == null) {
                promise.Resolve(resp);
            } else {
                if (err is RequestException) {
                    RequestException reqErr = err as RequestException;

                    if (reqErr.StatusCode == 400) {
                        Debug.LogError($"Creation parameters were invalid for {entity}: {resp.Text}");
                    } else if (reqErr.StatusCode == 500) {
                        Debug.LogError($"Failed to create {entity}: {resp.Text}");
                    } else {
                        Debug.LogError($"Request exception: {reqErr.StatusCode} {reqErr.Message}\n    {resp.Text}\n{reqErr.StackTrace}");
                    }
                } else {
                    Debug.Log($"Other exception: {err.Message}\n{err.StackTrace}");
                }

                promise.Reject(err);
            }
        });

        return promise;
    }

    public IPromise<ResponseHelper> CreateSite(string name, string longName, string city, string country) {
        RequestHelper request = newConfigRequest($"/org/{orgId}/sites");

        request.Body = new Site {
            name = name,
            longname = longName,
            city = city,
            country = country,
        };

        return CreatePost("site", request);
    }

    public IPromise<ResponseHelper> CreateUplink(string wan, string site) {
        RequestHelper request = newConfigRequest($"/org/{orgId}/uplinks");

        request.Body = new Uplink {
            wan = wan,
            site = site,
        };

        return CreatePost("uplink", request);
    }

    public IPromise<ResponseHelper> DeleteUplink(string uplinkId) {
        return RestClient.Delete(newConfigRequest("/uplink/" + uplinkId));
    }

    public IPromise<WanItems> GetWansInOrg() {
        return RestClient.Get<WanItems>(newConfigRequest("/org/" + orgId + "/wans"));
    }

    public IPromise<UplinkItems> GetUplinksInOrg() {
        return RestClient.Get<UplinkItems>(newConfigRequest("/org/" + orgId + "/uplinks"));
    }

    public IPromise<SitelinkReportingItems> GetSitelinks(string siteId) {
        // Since using standard RestClient with returning a promise counts any non-200
        // status code as an error, but 404 is a potentially valid response for no sitelinks,
        // we need to build the promise manually ourselves.
        var sitelinksPromise = new Promise<SitelinkReportingItems>();
        
        RestClient.Get<SitelinkReportingItems>(newReportingRequest("/site/" + siteId + "/sitelinks"), (err, resp, sitelinks) => {
            if (err == null) {
                Debug.Log($"Site {siteId} has {sitelinks.items.Length} sitelink(s)");
                sitelinksPromise.Resolve(sitelinks);
            } else if (err.StatusCode == 404) {
                // No sitelinks, return empty list.
                Debug.Log($"Site {siteId} has no sitelinks");
                sitelinksPromise.Resolve(new SitelinkReportingItems { items = new SitelinkReporting[] { } });
            } else {
                Debug.LogError($"Failed to get sitelinks for {siteId}: {err.StatusCode} {err.Message}");
                
                // Some sites were returning 503, so to work around this I'm logging it then returning an empty list of sitelinks.
                sitelinksPromise.Resolve(new SitelinkReportingItems { items = new SitelinkReporting[] { } });
                //sitelinksPromise.Reject(err);
            }
        });

        return sitelinksPromise;
    }
}

// Received JSON gets deserialized into these types.
namespace Models {
    namespace SteelConnect {
        [Serializable]
        public class SiteItems {
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

            // Not part of the API response, but added later by SteelConnectDataManager.
            public LatLong coordinates;
        }

        [Serializable]
        public class SitelinkReportingItems {
            public SitelinkReporting[] items;
        }

        [Serializable]
        public class SitelinkReporting {
            public string id;
            public string local_site;
            public string remote_site;
            public string state;
            public string status;
            public string inuse;
            public float throughput_in;
            public float throughput_out;
        }

        // Since two sitelink is reported for one "logical" sitelink, we bundle them
        // together as one entity.
        public class SitelinkPair {
            // This should always have two items when used outside this file.
            public List<SitelinkReporting> pair;

            public SitelinkPair() {
                pair = new List<SitelinkReporting>();
            }

            public bool IsValid() {
                return pair.Count == 2;
            }
        }

        [Serializable]
        public class WanItems {
            public Wan[] items;
        }

        [Serializable]
        public class Wan {
            public string id;
            public string name;
            public string longname;
            public string org;
            public string[] uplinks;
        }

        [Serializable]
        public class UplinkItems {
            public Uplink[] items;
        }

        [Serializable]
        public class Uplink {
            public string id;
            public string name;
            public string org;
            public string site;
            public string wan;
            public string node;
        }
    }
}