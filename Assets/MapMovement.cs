using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

public class MapMovement : MonoBehaviour {

    public float speed = .005f;

    Vector3 dragOrigin;

    [SerializeField]
    AbstractMap _mapManager;

    // Update is called once per frame
    void Update () {
        // When Mouse1 is pressed
		if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
        }
        // While Mouse1 is pressed
        if (Input.GetMouseButton(0))
        {
            var dragDelta = Input.mousePosition - dragOrigin;
            var axis = new Vector3(0f, -dragDelta.x * speed, 0f);

            Vector2 currlatlong = new Vector2((float)_mapManager.CenterLatitudeLongitude.x, (float)_mapManager.CenterLatitudeLongitude.y );
            currlatlong.x += dragDelta.y * -speed;
            currlatlong.y += dragDelta.x * -speed;
            _mapManager.UpdateMap(new Mapbox.Utils.Vector2d(currlatlong.x, currlatlong.y), _mapManager.Zoom);
        }
	}
}
