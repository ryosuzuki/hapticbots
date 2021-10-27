using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockpit : MonoBehaviour
{
  public static bool calibrating;
  public static bool initializing;

  Vector3 originalPos;

  bool initFinished;

  void Start()
  {
    initFinished = false;
    originalPos = this.transform.position;
  }

  void Calibrate()
  {
    calibrating = false;
    GameObject matObject = GameObject.Find("Mat");
    Vector3 origin = matObject.transform.position;
    Vector3 angle = matObject.transform.localEulerAngles;
    this.transform.position = new Vector3(
      origin.x + originalPos.x,
      origin.y + originalPos.y,
      origin.z + originalPos.z
    );
    this.transform.localEulerAngles = angle;

    initFinished = false;
    InitTargets();
  }

  void InitTargets()
  {
    int index = 0;
    /*
    GameObject joystickObject = GameObject.Find("Cockpit Collider 4");
    GameObject matObject = GameObject.Find("Mat");
    Vector3 origin = matObject.transform.position;
    Vector3 position = joystickObject.transform.position;
    Robots.targetPositions[index] = position;
    Robots.targetFixed[index] = true;
    index++;
    */

    if (initFinished == false) {
      GameObject[] cockpitObjects = new GameObject[3];
      cockpitObjects[0] = GameObject.Find("Cockpit Collider 1");
      // cockpitObjects[1] = GameObject.Find("Cockpit Collider 3");
      foreach (GameObject cockpitObject in cockpitObjects) {
        Vector3 pos = cockpitObject.transform.position;
        Robots.targetPositions[index] = pos;
        Robots.targetFixed[index] = false;
        index++;
      }
    }
    initFinished = true;
  }

  void Update()
  {
    if (calibrating) {
      Calibrate();
    }

    InitTargets();
  }
}
