using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess : MonoBehaviour
{
  public static bool calibrating;
  public static bool initializing;

  bool initFinished;

  void Start()
  {
    initFinished = false;
  }

  void Calibrate()
  {
    calibrating = false;
    GameObject matObject = GameObject.Find("Mat");
    Vector3 origin = matObject.transform.position;
    Vector3 angle = matObject.transform.localEulerAngles;
    this.transform.position = new Vector3(
      origin.x,
      origin.y + 0.02f,
      origin.z
    );
    this.transform.localEulerAngles = angle;

    initFinished = false;
    InitTargets();
  }

  void InitTargets()
  {
    int index = 0;
    GameObject[] chessObjects = new GameObject[3];
    chessObjects[0] = GameObject.Find("Chess King White");
    chessObjects[1] = GameObject.Find("Chess Bishop White");
    chessObjects[2] = GameObject.Find("Chess Knight White");
    foreach (GameObject chessObject in chessObjects) {
      Vector3 pos = chessObject.transform.position;
      Robots.targetPositions[index] = pos;
      Robots.targetFixed[index] = true;
      index++;
    }

    /*
    int i = 0;
    GameObject chessObject = GameObject.Find("Chess");
    foreach (Transform childTransform in chessObject.transform) {
      GameObject childObject = childTransform.gameObject;
      if (childObject.name.Contains("Chess Board") == true) {
        continue;
      }
      // GameObject matObject = GameObject.Find("Mat");
      // Vector3 origin = matObject.transform.position;
      Vector3 pos = childObject.transform.position;
      // pos.x = origin.x + pos.x;
      // pos.z = origin.z + pos.z;
      if (i >= Robots.targetPositions.Length) {
        continue;
      }
      Robots.targetPositions[i] = pos;
      Robots.targetFixed[i] = true;
      i++;
    }
    */
  }

  void Update()
  {
    if (calibrating) {
      Calibrate();
    }

    InitTargets();
  }
}
