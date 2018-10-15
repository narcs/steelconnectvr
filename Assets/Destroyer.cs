using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour {

    public float movementSmoothing = 1f;
    public float laserSmoothing = 1f;
    public GameObject originalPosition;
    public Transform movementTarget;
    public Transform destroyerTransform;
    public GameObject shootTarget;
    public GameObject laser;
    public GameObject pointer;

	// Use this for initialization
	void Start () {
        originalPosition.transform.position = destroyerTransform.position;
        laser.SetActive(false);
	}

    IEnumerator Movement(Transform target, bool shoot) {
        while (Vector3.Distance(destroyerTransform.position, target.position) > 0.05f) {
            destroyerTransform.position = Vector3.Lerp(destroyerTransform.position, target.position, movementSmoothing * Time.deltaTime);

            yield return null;
        }

        if (shoot) {
            Shoot(shootTarget);
        } else {
            pointer.SetActive(true);
        }
        yield return null;
    }

    IEnumerator LaserMovement(GameObject target) {
        while (Vector3.Distance(laser.transform.position, target.transform.position) > 0.05f) {
            laser.transform.position = Vector3.Lerp(laser.transform.position, target.transform.position, laserSmoothing * Time.deltaTime);

            yield return null;
        }

        laser.SetActive(false);
        SiteMarker siteMarker = shootTarget.GetComponent<SiteMarker>();
        siteMarker.SiteDestruction();
        shootTarget = null;
        // Move back to original position
        StopCoroutine("Movement");
        StartCoroutine(Movement(originalPosition.transform, false));
        yield return null;
    }

    public void StartDestruction(GameObject target) {
        shootTarget = target;
        destroyerTransform.position = originalPosition.transform.position;
        StopCoroutine("Movement");
        StartCoroutine(Movement(movementTarget, true));
        pointer.SetActive(false);
    }

    public void Shoot(GameObject target) {
        Vector3 heading = target.transform.position - destroyerTransform.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        laser.transform.position = destroyerTransform.position;
        laser.transform.rotation = Quaternion.LookRotation(direction);
        laser.SetActive(true);
        StopCoroutine("LaserMovement");
        StartCoroutine("LaserMovement", shootTarget);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("d")) {
            StopCoroutine("Movement");
            destroyerTransform.position = originalPosition.transform.position;
            StartCoroutine("Movement", movementTarget);
        }
		
	}
}
