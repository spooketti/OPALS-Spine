// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.UIElements;
// using TMPro;
// using System;
// using Image = UnityEngine.UI.Image;

// public class Calibration : MonoBehaviour
// {
//     public ZEDArUcoDetectionManager detectionManager;
//     public GameObject centroid;
//     public GameObject tagPrefab;
//     private int lastPositionListSize = 0;
//     private bool isCalibrated = false;
//     private bool isAngleCalibrated = false;
//     private bool isSystemCalibrated = false;
//     private bool isEulerTagSeen = false;
//     private int eulerID = 0;
//     public Image radialProgresImage;

//     private List<int> seenID = new List<int>();

//     //UI
//     public GameObject eulerPanel;
//     public TMP_Text eulerStatus;
//     public GameObject systemPanel;

//     public Transform spineEulerTarget;
//     private long epochTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
//     private GameObject CreateMarkerObject(string name, int id)
//     {
//         GameObject marker = Instantiate(tagPrefab, new Vector3(0, 0, 0), Quaternion.identity);
//         marker.name = name;
//         marker.SetActive(false);
//         var moveScript = marker.GetComponent<MarkerObject_MoveToMarkerSimple>();
//         moveScript.markerID = id;
//         marker.SetActive(true);
//         marker.GetComponent<ScaleToMarkerSize>().arucoManager = detectionManager;
//         return marker;
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // centroid.gameObject.SetActive(isCalibrated);
//         if (isCalibrated)
//         {
//             Debug.Log("bro");
//             return;
//         }
//         if (!isAngleCalibrated)
//         {
//             eulerPanel.SetActive(true);

//             if (lastPositionListSize != detectionManager.CalibrationIDList.Count) //on number of tags seen changed
//             {
//                 isEulerTagSeen = false;
//                 lastPositionListSize = detectionManager.CalibrationIDList.Count;
//                 int detectedTags = detectionManager.numberOfDetectedTags;
//                 switch (detectedTags)
//                 {
//                     case 0:
//                         eulerStatus.text = "No Tags Detected";
//                         break;

//                     case 1:
//                         epochTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
//                         eulerStatus.text = $"Tag Detected: {detectionManager.oneTagDetectedId}";
//                         isEulerTagSeen = true;
//                         spineEulerTarget.GetComponent<MarkerObject_MoveToMarkerSimple>().markerID = detectionManager.oneTagDetectedId;
//                         detectionManager.GetComponent<ZEDArUcoDetectionManager>().UnregisterAllMarkers();
//                         detectionManager.GetComponent<ZEDArUcoDetectionManager>().DynamicRegisterMarker(spineEulerTarget.GetComponent<MarkerObject_MoveToMarkerSimple>());
//                         break;

//                     default:
//                         eulerStatus.text = "Please have only one tag showing";
//                         break;
//                 }
//             }
//             long progress = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - epochTime;
//             float radialProgress = Mathf.Clamp(progress / 5000f, 0f, 1f);
//             radialProgresImage.fillAmount = radialProgress;
//             if (progress >= 5000 && isEulerTagSeen)
//             {
//                 isAngleCalibrated = true;
//                 systemPanel.SetActive(true);
//                 eulerPanel.SetActive(false);
//                 eulerID = detectionManager.oneTagDetectedId;
//                 Debug.Log("euler completed");
//             }
//             return;
//         }
//         centroid.GetComponent<CentroidManager>().enabled = true;
//         if (Input.GetKey(KeyCode.E))
//         {
//             Debug.Log("pressed e");
//             isSystemCalibrated = true;
//             systemPanel.SetActive(false);
//         }
//         if (!isSystemCalibrated)
//         {

//             if (lastPositionListSize != detectionManager.CalibrationIDList.Count)
//             {
//                 lastPositionListSize = detectionManager.CalibrationIDList.Count;

//                 foreach (int id in detectionManager.CalibrationIDList)
//                 {
//                     if (!GameObject.Find($"tag{id}") && id != eulerID)
//                     {
//                         GameObject tag = CreateMarkerObject($"tag{id}", id);
//                         tag.transform.SetParent(centroid.transform);
//                     }
//                 }
//             }
//             return;
//         }
//         isCalibrated = true;
//         systemPanel.SetActive(false);

//     }
// }


using UnityEngine;
using TMPro;
using Image = UnityEngine.UI.Image;

public class Calibration : MonoBehaviour
{
    public ZEDArUcoDetectionManager detectionManager;
    public GameObject centroid;
    public GameObject tagPrefab;
    public Image radialProgressImage;

    public GameObject eulerPanel;
    public TMP_Text eulerStatus;
    public GameObject systemPanel;
    public Transform spineEulerTarget;

    private int lastPositionListSize = 0;
    private bool isCalibrated = false;
    private bool isAngleCalibrated = false;
    private bool isSystemCalibrated = false;
    private bool isEulerTagSeen = false;
    private int eulerID = 0;
    private float eulerTimer = 0f;

    private void Update()
    {
        if (isCalibrated)
        {
            Debug.Log("bro");
            return;
        }

        if (!isAngleCalibrated)
        {
            HandleEulerCalibration();
            return;
        }

        if (!isSystemCalibrated)
        {
            HandleSystemCalibration();
            return;
        }

        FinalizeCalibration();
    }

    private void HandleEulerCalibration()
    {
        eulerPanel.SetActive(true);

        if (lastPositionListSize != detectionManager.CalibrationIDList.Count)
        {
            lastPositionListSize = detectionManager.CalibrationIDList.Count;
            isEulerTagSeen = false;
            eulerTimer = 0f;

            int detectedTags = detectionManager.numberOfDetectedTags;

            switch (detectedTags)
            {
                case 0:
                    eulerStatus.text = "No Tags Detected";
                    break;

                case 1:
                    isEulerTagSeen = true;
                    int tagID = detectionManager.oneTagDetectedId;
                    eulerStatus.text = $"Tag Detected: {tagID}";

                    var moveScript = spineEulerTarget.GetComponent<MarkerObject_MoveToMarkerSimple>();
                    moveScript.markerID = tagID;

                    detectionManager.UnregisterAllMarkers();
                    detectionManager.DynamicRegisterMarker(moveScript);
                    break;

                default:
                    eulerStatus.text = "Please have only one tag showing";
                    break;
            }
        }

        if (isEulerTagSeen)
        {
            eulerTimer += Time.deltaTime;
            radialProgressImage.fillAmount = Mathf.Clamp01(eulerTimer / 5f);

            if (eulerTimer >= 5f)
            {
                isAngleCalibrated = true;
                eulerID = detectionManager.oneTagDetectedId;

                eulerPanel.SetActive(false);
                systemPanel.SetActive(true);

                Debug.Log("Euler calibration completed");
            }
            return;
        }
        eulerTimer = 0f;
        radialProgressImage.fillAmount = 0f;
    }


    private void HandleSystemCalibration()
    {
        centroid.GetComponent<CentroidManager>().enabled = true;

        if (Input.GetKey(KeyCode.E))
        {
            Debug.Log("Pressed E");
            isSystemCalibrated = true;
            systemPanel.SetActive(false);
            return;
        }

        if (lastPositionListSize != detectionManager.CalibrationIDList.Count)
        {
            lastPositionListSize = detectionManager.CalibrationIDList.Count;

            foreach (int id in detectionManager.CalibrationIDList)
            {
                if (id == eulerID || GameObject.Find($"tag{id}")) continue;

                GameObject tag = CreateMarkerObject($"tag{id}", id);
                tag.transform.SetParent(centroid.transform);
            }
        }
    }

    private void FinalizeCalibration()
    {
        isCalibrated = true;
        systemPanel.SetActive(false);
    }

    private GameObject CreateMarkerObject(string name, int id)
    {
        GameObject marker = Instantiate(tagPrefab, Vector3.zero, Quaternion.identity);
        marker.name = name;
        marker.SetActive(false);

        var moveScript = marker.GetComponent<MarkerObject_MoveToMarkerSimple>();
        moveScript.markerID = id;

        marker.GetComponent<ScaleToMarkerSize>().arucoManager = detectionManager;
        marker.SetActive(true);

        return marker;
    }
}
