using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Promises and SteelConnect API access.
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

public class SteelConnectDataManager : MonoBehaviour {
    private SteelConnect _steelConnect;

    // Stored data. This data pertains to a single organization in a single realm.
    private List<Site> _baseSites = null;
    private List<SiteData> _sites = null; // Sites with coordinates.
    private List<Wan> _wans = null;
    private List<Uplink> _uplinks = null;

    private Dictionary<SiteId, List<SitelinkReporting>> _sitelinkReportings = null;

    // ---

    // Initialization is done here.
    private void Awake() {
        _steelConnect = new SteelConnect();
    }

    // ---

    public IPromise<List<SiteData>> GetSites(bool forceRefresh) {
        if (forceRefresh || _sites == null) {
            return _steelConnect.GetSitesInOrg()
                .Then(items => {
                    // Save the API sites for later.
                    _baseSites = new List<Site>(items.items);
                    return _baseSites;
                })
                .ThenAll(sites => {
                    return sites.Select(site => LatLongUtility.GetLatLongForAddress(site.street_address, site.city, site.country));
                })
                .Then(latLongs => {
                    _sites = new List<SiteData>();

                    // We now have an IEnumerable called latLongs (think of it like a list) that has elements of LatLong.
                    // Each element of latLongs corresponds to the site in _baseSites with the same index.
                    // We use this fact to link sites to their LatLongs.
                    for (int i = 0; i < _baseSites.Count; ++i) {
                        _sites.Append(new SiteData(_baseSites[i], latLongs.ElementAt(i)));
                    }

                    return _sites;
                });
        } else {
            return Promise<List<SiteData>>.Resolved(_sites);
        }
    }

    public IPromise<List<Wan>> GetWans(bool forceRefresh) {
        if (forceRefresh || _wans == null) {
            return _steelConnect.GetWansInOrg()
                .Then(items => {
                    _wans = new List<Wan>(items.items);
                    return _wans;
                });
        } else {
            return Promise<List<Wan>>.Resolved(_wans);
        }
    }

    public IPromise<List<Uplink>> GetUplinks(bool forceRefresh) {
        if (forceRefresh || _uplinks == null) {
            return _steelConnect.GetUplinksInOrg()
                .Then(items => {
                    _uplinks = new List<Uplink>(items.items);
                    return _uplinks;
                });
        } else {
            return Promise<List<Uplink>>.Resolved(_uplinks);
        }
    }

    public IPromise<Dictionary<SiteId, List<SitelinkReporting>>> GetSitelinks(bool forceRefresh) {
        if (forceRefresh || _sitelinkReportings == null) {
            return GetSites(false)
                .ThenAll(siteDatas => siteDatas.Select(siteData => _steelConnect.GetSitelinks(siteData.site.id)))
                .Then(sitelinksList => {
                    _sitelinkReportings = new Dictionary<SiteId, List<SitelinkReporting>>();

                    for (int i = 0; i < _sites.Count; ++i) {
                        _sitelinkReportings.Add(_sites[i].site.id, new List<SitelinkReporting>(sitelinksList.ElementAt(i).items));
                    }

                    return _sitelinkReportings;
                });
        } else {
            return Promise<Dictionary<SiteId, List<SitelinkReporting>>>.Resolved(_sitelinkReportings);
        }
    }
}

// ---

public class SiteData {
    public Site site;
    public LatLong coordinates;

    public SiteData(Site site, LatLong coordinates) {
        this.site = site;
        this.coordinates = coordinates;
    }
}