using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
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
    this.transform.position = origin;
    this.transform.localEulerAngles = angle;

    initFinished = false;
    InitTargets();
  }

  void InitTargets()
  {
    if (initFinished == false) {
      GameObject matObject = GameObject.Find("Mat");
      Vector3 origin = matObject.transform.position;
      Vector3 pos = new Vector3();
      pos.x = origin.x + pos.x;
      pos.y = origin.y + pos.y;
      pos.z = origin.z + pos.z;
      Robots.targetPositions[0] = pos;
      Robots.targetFixed[0] = false;
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
