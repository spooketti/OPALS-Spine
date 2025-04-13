using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System;

public class Calibration : MonoBehaviour
{
    public ZEDArUcoDetectionManager detectionManager;
    public GameObject centroid;
    private int lastPositionListSize = 0;
    private bool isCalibrated = false;
    private bool isAngleCalibrated = false;
    private bool isSystemCalibrated = false;
    private bool isEulerTagSeen = false;

    //UI
    public GameObject eulerPanel;
    public TMP_Text eulerStatus;
    public GameObject systemPanel;

    public Transform spineEulerTarget;
    private long epochTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private GameObject CreateMarkerObject(string name, int id)
    {
        GameObject marker = new GameObject(name);
        var moveScript = marker.AddComponent<MarkerObject_MoveToMarkerSimple>();
        moveScript.markerID = id;
        marker.AddComponent<ScaleToMarkerSize>();
        marker.GetComponent<ScaleToMarkerSize>().arucoManager = detectionManager;
        return marker;
    }

    // Update is called once per frame
    void Update()
    {
        centroid.gameObject.SetActive(isCalibrated);
        if(isCalibrated)
        {
            return;
        }
        if (!isAngleCalibrated)
        {
            eulerPanel.SetActive(true);

            if (lastPositionListSize != detectionManager.CalibrationIDList.Count) //on number of tags seen changed
            {
                isEulerTagSeen = false;
                lastPositionListSize = detectionManager.CalibrationIDList.Count;
                int detectedTags = detectionManager.numberOfDetectedTags;
                switch (detectedTags)
                {
                    case 0:
                        eulerStatus.text = "No Tags Detected";
                        break;

                    case 1:
                        epochTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        eulerStatus.text = $"Tag Detected: {detectionManager.oneTagDetectedId}";
                        isEulerTagSeen = true;
                        spineEulerTarget.GetComponent<MarkerObject_MoveToMarkerSimple>().markerID = detectionManager.oneTagDetectedId;
                        break;

                    default:
                        eulerStatus.text = "Please have only one tag showing";
                        break;
                }
            }
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - epochTime >= 5 && isEulerTagSeen)
            {
                isAngleCalibrated = true;
                systemPanel.SetActive(true);
                eulerPanel.SetActive(false);
            }
            return;
        }
        if (!isSystemCalibrated)
        {
            if (lastPositionListSize != detectionManager.CalibrationIDList.Count)
            {
                foreach (Transform child in centroid.transform)
                {
                    Destroy(child.gameObject);
                }
                lastPositionListSize = detectionManager.CalibrationIDList.Count;

                foreach (int id in detectionManager.CalibrationIDList)
                {
                    GameObject tag = CreateMarkerObject($"tag{id}", id);
                    tag.transform.SetParent(centroid.transform);
                }
            }
            isSystemCalibrated = Input.GetKeyDown(KeyCode.Return);
            return;
        }
        isCalibrated = true;
        systemPanel.SetActive(false);
        

    }
}
