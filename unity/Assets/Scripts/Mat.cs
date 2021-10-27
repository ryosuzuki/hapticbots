using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mat : MonoBehaviour
{

  public static bool calibrated;
  public static Hashtable hash;
  public GameObject pointPrefab;
  public GameObject targetPrefab;

  public static Vector3 fingerPos;
  public static string calibrateCommand;

  public static bool showPoints;
  public static bool showTargets;

  GameObject origin;
  GameObject[] pointObjects;

  [SerializeField] OVRSkeleton leftSkeleton;
  [SerializeField] OVRSkeleton rightSkeleton;

  string scene;
  float timeleft;
  string[] detectables;

  void Awake()
  {
  }

  void Start()
  {
    showPoints = false;
    showTargets = false;

    // scene = "house";
    // scene = "minecraft";
    // scene = "chess";
    scene = "mountain";
    // scene = "city";
    // scene = "cockpit";
    // scene = "body";
    print(scene);

    calibrated = false;
    hash = new Hashtable();
    pointObjects = new GameObject[0];

    GameObject houseObject = GameObject.Find("House");
    GameObject minecraftObject = GameObject.Find("MineCraft");
    GameObject chessObject = GameObject.Find("Chess");
    GameObject mountainObject = GameObject.Find("Mountain");
    // GameObject cityObject = GameObject.Find("City");
    GameObject cockpitObject = GameObject.Find("Cockpit");
    GameObject bodyObject = GameObject.Find("Body");
    GameObject tempObject = GameObject.Find("Temp Mat");
    if (tempObject != null)
      tempObject.SetActive(false);
    int targetNum = 0;
    if (scene.Equals("house")) {
      targetNum = 4;
      houseObject.SetActive(true);
      House.calibrating = true;
    } else {
      if (houseObject != null)
        houseObject.SetActive(false);
    }
    if (scene.Equals("minecraft")) {
      targetNum = 3;
      minecraftObject.SetActive(true);
      MineCraft.initializing = true;
      MineCraft.calibrating = true;
    } else {
      if (minecraftObject != null)
        minecraftObject.SetActive(false);
    }
    if (scene.Equals("chess")) {
      targetNum = 3;
      chessObject.SetActive(true);
      Chess.calibrating = true;
    } else {
      if (chessObject != null)
        chessObject.SetActive(false);
    }
    if (scene.Equals("mountain")) {
      targetNum = 1;
      mountainObject.SetActive(true);
      Mountain.calibrating = true;
    } else {
      if (mountainObject != null)
        mountainObject.SetActive(false);
    }
    /*
    if (scene.Equals("city")) {
      targetNum = 1;
      cityObject.SetActive(true);
      City.calibrating = true;
    } else {
      if (cityObject != null)
        cityObject.SetActive(false);
    }
    */
    if (scene.Equals("cockpit")) {
      targetNum = 1;
      cockpitObject.SetActive(true);
      Cockpit.calibrating = true;
    } else {
      if (cockpitObject != null)
        cockpitObject.SetActive(false);
    }
    if (scene.Equals("body")) {
      targetNum = 1;
      bodyObject.SetActive(true);
      Body.calibrating = true;
    } else {
      if (bodyObject != null)
        bodyObject.SetActive(false);
    }
    Robots.targetPositions = new Vector3[targetNum];
    Robots.targetObjects = new GameObject[targetNum];
    Robots.targetFixed = new bool[targetNum];


    if (showTargets) {
      for (int i = 0; i < targetNum; i++) {
        Transform parent = GameObject.Find("Targets").transform;
        GameObject targetObject = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity, parent);
        Robots.targetObjects[i] = targetObject;
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (calibrateCommand != null) {
      Calibrate();
    }

    try {
      fingerPos = rightSkeleton.Bones[(int) OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
    }
    catch (Exception e) {
      // print("error");
    }

    timeleft -= Time.deltaTime;
    if (timeleft <= 0.0) {
      timeleft = 1.0f;
      ComputeHeight();
    }

  }


  void Calibrate()
  {
    Debug.Log(calibrateCommand);
    if (calibrateCommand == "return") {
      calibrated = !calibrated;
    }

    if (calibrateCommand == "space") {
      Vector3 pos0 = new Vector3(0.1f, 0.2f, -0.3f);
      try {
        pos0 = leftSkeleton.Bones[(int) OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
        // origin.transform.position = pos0;
      }
      catch (Exception e) {
      }
      this.transform.position = pos0;
      /*
      Vector3 diff = pos0 - matOrigin;
      surface.transform.position = surfaceOrigin + diff;
      StartCoroutine(RecomputeHeight());
      */
    }

    if (calibrateCommand == "left") {
      Vector3 angle = this.transform.localEulerAngles;
      angle.y = angle.y + 1f; // 0.1f;
      this.transform.localEulerAngles = angle;
      /*
      Vector3 angle1 = surface.transform.localEulerAngles;
      angle1.y = angle.y;
      surface.transform.localEulerAngles = angle1;
      */
    }

    if (calibrateCommand == "right") {
      Vector3 angle = this.transform.localEulerAngles;
      angle.y = angle.y - 1f; // 0.1f;
      this.transform.localEulerAngles = angle;
      /*
      Vector3 angle1 = surface.transform.localEulerAngles;
      angle1.y = angle.y;
      surface.transform.localEulerAngles = angle1;
      */
    }
    House.calibrating = true;
    MineCraft.calibrating = true;
    Chess.calibrating = true;
    Mountain.calibrating = true;
    City.calibrating = true;
    Cockpit.calibrating = true;
    Body.calibrating = true;

    calibrateCommand = null;
    ComputeHeight();
    // StartCoroutine(RecomputeHeight());
  }

  public void UpdateHeight()
  {
    StartCoroutine(RecomputeHeight());
  }

  IEnumerator RecomputeHeight()
  {
    yield return new WaitForSeconds(1);
    ComputeHeight();
  }

  public void ComputeHeight()
  {
    // computer height from -50cm to +50cm around the origin
    hash = new Hashtable();
    RaycastHit hit;
    Vector3 origin = this.transform.position;
    Vector3 originAngle = this.transform.localEulerAngles;

    for (int i = 0; i < pointObjects.Length; i++) {
      GameObject pointObject = pointObjects[i];
      Destroy(pointObject);
    }

    pointObjects = new GameObject[56 * 56];
    int index = 0;
    for (int i = 0; i < 56; i++) {
      for (int j = 0; j < 56; j++) {
        float x = origin.x + (float) i / 55 - 0.275f;
        float z = origin.z + (float) j / 55 - 0.275f;
        Vector3 pos = new Vector3(x, 0, z);
        pos = Quaternion.Euler(new Vector3(0, originAngle.y, 0)) * pos;
        x = pos.x;
        z = pos.z;
        float offset = 1.0f;
        Vector3 startPoint = new Vector3(x, origin.y + offset, z);
        Vector3 direction = new Vector3(0, -1, 0);
        if (Physics.Raycast(startPoint, direction, out hit)) {
          if (hit.collider.gameObject.name == "Mat") {
            continue;
          }
          string name = hit.collider.gameObject.transform.root.gameObject.name;
          if (name.Contains("House") != true &&
              name.Contains("Tree") != true &&
              name.Contains("Block") != true &&
              name.Contains("MineCraft") != true &&
              name.Contains("Chess") != true &&
              name.Contains("Mountain") != true &&
              name.Contains("City") != true &&
              name.Contains("Cockpit") != true &&
              name.Contains("Body") != true
          ) {
            continue;
          }
          if (hit.collider.gameObject.name.Contains("Chess Board") == true) {
            continue;
          }
          // print(hit.distance);
          string key = string.Format("{0:F2}-{1:F2}", x, z);
          float height = offset - hit.distance;
          if (height > 0.03) {
            hash[key] = height;
            // Debug.Log(key);
            if (showPoints) {
              float y = origin.y + height;
              Transform parent = GameObject.Find("Points").transform;
              GameObject pointObject = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity, parent);
              pointObject.transform.position = new Vector3(x, y, z);
              pointObjects[index] = pointObject;
              index++;
            }
          }
        }
      }
    }
    // Debug.Log(hash);
  }
}
