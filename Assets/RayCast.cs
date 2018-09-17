using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;

public class RayCast : MonoBehaviour {
    // Flat Map
    [SerializeField]
    AbstractMap _mapManager;
    // Globe
    [SerializeField]
    GlobeTileProvider _globeMap;
    
    [SerializeField]
    Transform _objectToRotate;

    [SerializeField]
    float _multiplier;

    Vector3 _startTouchPosition;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
            int layerMask = 1 << 8;
            layerMask = ~layerMask;

            RaycastHit hit;
            Quaternion rotation = _globeMap.transform.rotation;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.magenta, 200f);
                Vector3 position = hit.point;
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.collider.gameObject.layer == _globeMap.gameObject.layer)
                    {
                        position = Quaternion.Inverse(rotation) * position;

                        Vector2d geo = Conversions.GeoFromGlobePosition(position, 100);
                        _mapManager.UpdateMap(new Mapbox.Utils.Vector2d(geo.x, geo.y), _mapManager.Zoom);
                        //Debug.Log(hit.point);
                    
                        _startTouchPosition = Input.mousePosition;
                    }
                }
                if (Input.GetMouseButton(0))
                {
                    if (hit.collider.gameObject.layer == _globeMap.gameObject.layer)
                    {
                        var dragDelta = Input.mousePosition - _startTouchPosition;
                        var axis = new Vector3(0f, -dragDelta.x * _multiplier, 0f);
                        _objectToRotate.RotateAround(_objectToRotate.position, axis, _multiplier);
                    }
                }
            }
            else
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                //Debug.Log("Did not Hit");
            }
    }
}
