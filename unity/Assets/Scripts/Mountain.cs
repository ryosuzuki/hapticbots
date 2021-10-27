using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountain : MonoBehaviour
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
      origin.x + 0f,
      origin.y + 0.01f,
      origin.z + 0f
    );
    this.transform.localEulerAngles = angle;

    initFinished = false;
    InitTargets();
  }

  void InitTargets()
  {
    if (initFinished == false) {
      // GameObject matObject = GameObject.Find("Mat");
      // Vector3 origin = matObject.transform.position;
      GameObject mountainObject = GameObject.Find("mountain");
      Vector3 pos = mountainObject.transform.position;
      // pos.x = origin.x + pos.x;
      // pos.y = origin.y + pos.y;
      // pos.z = origin.z + pos.z;
      Robots.targetPositions[0] = pos;
      Robots.targetFixed[0] = false;
    }
    initFinished = true;
  }

  // Update is called once per frame
  void Update()
  {
    if (calibrating) {
      Calibrate();
    }

    InitTargets();
  }
}
