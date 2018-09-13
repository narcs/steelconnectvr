using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using RSG;
using Proyecto26;
using Models.SteelConnect;


public class WanManager : MonoBehaviour {

    public GameObject panel;
    public GameObject wanPrefab;
    public Dictionary<string, Uplink> uplinks = new Dictionary<string, Uplink>();

    private List<Wan> _wans = new List<Wan>();
    private SteelConnect _steelConnect;
    private bool _showWans = false;

	void Start () {
        _steelConnect = new SteelConnect();
		
	}

    public void ShowHideWans() {
        _showWans = !_showWans;
        panel.transform.parent.transform.gameObject.SetActive(_showWans);
    }

    public void UpdateWans() {
        foreach (Transform child in panel.transform) {
            Destroy(child.gameObject);
        }
        _wans.Clear();
        uplinks.Clear();
        // Get WANs from SteelConnect API
        _steelConnect.GetWansInOrg()
            .Then(wans => wans.items.ToList().ForEach(wan => {
                _wans.Add(wan);
            }))
            .Then(() =>
                // Get uplinks from SteelConnect API
                _steelConnect.GetUplinksInOrg()
                    .Then(uplinks => uplinks.items.ToList().ForEach(uplink => {
                        this.uplinks[uplink.id] = uplink;
                    })))
            .Then(() => {
                // Create WAN gameObjects
                foreach (Wan wan in _wans) {
                    GameObject newWanMarkerObject = Instantiate(wanPrefab, panel.transform);
                    WanMarker newWanMarker = newWanMarkerObject.GetComponent<WanMarker>();
                    newWanMarker.wan = wan;
                    Debug.Log($"Created {wan.id}");
                }
            });


    }
	
}
