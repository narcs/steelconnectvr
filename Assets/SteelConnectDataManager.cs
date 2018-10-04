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
    // This isn't actually returned, it's just here for internal use. What's actually returned
    // are the promises.
    private List<Site> _sites = null;
    private List<Wan> _wans = null;
    private List<Uplink> _uplinks = null;
    private Dictionary<SiteId, List<SitelinkReporting>> _sitelinkReportings = null;

    // Promises are only executed once, then always just return their result, so we can store
    // the current set of promises here to return when we don't need to refresh data.
    private IPromise<List<Site>> _sitesPromise = null;
    private IPromise<List<Wan>> _wansPromise = null;
    private IPromise<List<Uplink>> _uplinksPromise = null;
    private IPromise<Dictionary<SiteId, List<SitelinkReporting>>> _sitelinksPromise = null;

    // ---

    private void Start() {
        _steelConnect = new SteelConnect();
    }

    // ---

    public IPromise<List<Site>> GetSites(bool forceRefresh) {
        if (forceRefresh || _sitesPromise == null) {
            _sitesPromise = _steelConnect.GetSitesInOrg()
                .Then(items => {
                    _sites = new List<Site>(items.items);
                    return _sites;
                })
                .ThenAll(sites => {
                    // Get coordinates for each site.
                    return sites.Select(site => LatLongUtility.GetLatLongForAddress(site.street_address, site.city, site.country));
                })
                .Then(latLongs => {
                    // We now have an IEnumerable called latLongs (think of it like a list) that has elements of LatLong.
                    // Each element of latLongs corresponds to the site in _sites with the same index.
                    // We use this fact to link sites to their LatLongs.
                    for (int i = 0; i < _sites.Count; ++i) {
                        _sites[i].coordinates = latLongs.ElementAt(i);
                    }

                    return _sites;
                });
        }

        return _sitesPromise;
    }

    public IPromise<List<Wan>> GetWans(bool forceRefresh) {
        if (forceRefresh || _wansPromise == null) {
            _wansPromise = _steelConnect.GetWansInOrg()
                .Then(items => {
                    _wans = new List<Wan>(items.items);
                    return _wans;
                });
        }

        return _wansPromise;
    }

    public IPromise<List<Uplink>> GetUplinks(bool forceRefresh) {
        if (forceRefresh || _uplinksPromise == null) {
            _uplinksPromise = _steelConnect.GetUplinksInOrg()
                .Then(items => {
                    _uplinks = new List<Uplink>(items.items);
                    return _uplinks;
                });
        }

        return _uplinksPromise;
    }

    public IPromise<Dictionary<SiteId, List<SitelinkReporting>>> GetSitelinks(bool forceRefresh) {
        if (forceRefresh || _sitelinksPromise == null) {
            _sitelinksPromise = GetSites(false)
                .ThenAll(sites => sites.Select(site => _steelConnect.GetSitelinks(site.id)))
                .Then(sitelinksList => {
                    _sitelinkReportings = new Dictionary<SiteId, List<SitelinkReporting>>();

                    for (int i = 0; i < _sites.Count; ++i) {
                        _sitelinkReportings.Add(_sites[i].id, new List<SitelinkReporting>(sitelinksList.ElementAt(i).items));
                    }

                    return _sitelinkReportings;
                });
        }

        return _sitelinksPromise;
    }
}