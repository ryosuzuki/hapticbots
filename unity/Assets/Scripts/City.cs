using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
  public static bool calibrating;
  public static bool initializing;

  void Start()
  {

  }

  void Calibrate()
  {
    calibrating = false;
    GameObject matObject = GameObject.Find("Mat");
    Vector3 origin = matObject.transform.position;
    Vector3 angle = matObject.transform.localEulerAngles;
    this.transform.position = new Vector3(
      origin.x + 1.3f,
      origin.y + 0.01f,
      origin.z + 1.3f
    );
    this.transform.localEulerAngles = angle;
  }

  // Update is called once per frame
  void Update()
  {
    if (calibrating) {
      Calibrate();
    }
  }
}
