using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using RSG;
using Proyecto26;
using Models.SteelConnect;

// Type alias for clarity.
using WanID = System.String;

public class WanManager : MonoBehaviour {

    private List<Wan> _wans = new List<Wan>();
    private Dictionary<string, Uplink> _uplinks = new Dictionary<string, Uplink>();
    private SteelConnect _steelConnect;

	void Start () {
        _steelConnect = new SteelConnect();
		
	}

    private void hello(string s) {
        Debug.Log(s);
    }

    public void UpdateWans() {
        _wans.Clear();
        _uplinks.Clear();
        // Get WANs from SteelConnect API
        _steelConnect.GetWansInOrg()
            .Then(wans => wans.items.ToList().ForEach(wan => {
                _wans.Add(wan);
            }))
            .Then(() =>
                // Get uplinks from SteelConnect API
                _steelConnect.GetUplinksInOrg()
                    .Then(uplinks => uplinks.items.ToList().ForEach(uplink => {
                        _uplinks[uplink.id] = uplink;
                    })))
            .Then(() => {
                // Create WAN gameObjects
                foreach (Wan wan in _wans) {
                    Debug.Log($"{wan.id}");
                }
            });


    }
	
}
