using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour
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
    int index = 0;
    GameObject[] treeObjects = new GameObject[3];
    treeObjects[0] = GameObject.Find("Tree_1");
    treeObjects[1] = GameObject.Find("Tree_2");
    treeObjects[2] = GameObject.Find("Tree_3");
    foreach (GameObject treeObject in treeObjects) {
      Vector3 pos = treeObject.transform.position;
      Robots.targetPositions[index] = pos;
      Robots.targetFixed[index] = true;
      index++;
    }

    if (initFinished == false) {
      GameObject houseObject = GameObject.Find("House_Model");
      GameObject matObject = GameObject.Find("Mat");
      Vector3 origin = matObject.transform.position;
      Vector3 position = houseObject.transform.position;
      position.y = origin.y + 0.08f;
      houseObject.transform.position = position;

      Quaternion rotation = houseObject.transform.rotation;
      rotation.x = 0;
      rotation.z = 0;
      houseObject.transform.rotation = rotation;

      Robots.targetPositions[index] = position;
      Robots.targetFixed[index] = false;
      index++;
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
