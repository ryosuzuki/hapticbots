using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineCraft : MonoBehaviour
{
  public GameObject blockPrefab;
  public static bool calibrating;
  public static bool initializing;

  GameObject[] blockObjects;

  void Start()
  {
  }

  void Init()
  {
    initializing = false;
    int size = 3 * 3 * 3;
    int index = 0;
    blockObjects = new GameObject[size];
    for (int i = 0; i < 3; i++) {
      for (int j = 0; j < 2; j++) {
        for (int k = 0; k < 3; k++) {
          Transform parent = GameObject.Find("MineCraft").transform;
          GameObject blockObject = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity, parent);
          blockObject.tag = "Block";
          float x = (float) i * 0.05f - 0.05f;
          float z = (float) j * 0.05f - 0.05f;
          float y = (float) k * 0.05f + 0.03f;
          Vector3 pos = new Vector3(x, y ,z);
          blockObject.transform.position = pos;
          blockObject.name = "Block-" + i + k;
          blockObjects[index] = blockObject;
          index++;
        }
      }
    }
    Calibrate();
  }

  void Calibrate()
  {
    calibrating = false;
    GameObject matObject = GameObject.Find("Mat");
    Vector3 origin = matObject.transform.position;
    Vector3 angle = matObject.transform.localEulerAngles;
    int index = 0;
    this.transform.position = origin;
    this.transform.localEulerAngles = angle;
  }

  void Update()
  {
    if (initializing) {
      Init();
    }

    if (calibrating) {
      Calibrate();
    }

    for (int i = 0; i < 3; i++) {
      int index = i * 6;
      GameObject blockObject = blockObjects[index];
      // GameObject matObject = GameObject.Find("Mat");
      // Vector3 origin = matObject.transform.position;
      // Vector3 originAngle = matObject.transform.localEulerAngles;
      Vector3 pos = blockObject.transform.position;
      // pos.x = origin.x + pos.x;
      // pos.z = origin.z + pos.z;
      // pos.y = origin.y + pos.y;
      // pos = Quaternion.Euler(new Vector3(0, originAngle.y, 0)) * pos;
      if (i >= Robots.targetPositions.Length) {
        continue;
      }
      Robots.targetPositions[i] = pos;
      Robots.targetFixed[i] = true;
    }
  }
}
