using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House2 : MonoBehaviour
{
  public GameObject housePrefab;
  public GameObject treePrefab;
  public static bool calibrating;
  public static bool initializing;

  GameObject houseObject;
  GameObject[] treeObjects;
  Vector3[] treePositions;

  void Start()
  {

  }

  void Init()
  {
    initializing = false;
    houseObject = Instantiate(housePrefab);
    Rigidbody rb = houseObject.GetComponent<Rigidbody>();
    int size = 3;
    float scale = 0.8f;
    treeObjects = new GameObject[size];
    treePositions = new Vector3[size];
    for (int i = 0; i < treeObjects.Length; i++) {
      GameObject treeObject = Instantiate(treePrefab);
      treeObject.tag = "Tree";
      Vector3 originalScale = treeObject.transform.localScale;
      treeObject.transform.localScale = new Vector3(
        originalScale.x * scale,
        originalScale.y * scale,
        originalScale.z * scale
      );
      treeObjects[i] = treeObject;
    }
    treePositions[0] = new Vector3(0.2f, 0.1f * scale, 0.2f);
    treePositions[1] = new Vector3(0.2f, 0.1f * scale, -0.2f);
    treePositions[2] = new Vector3(-0.2f, 0.1f * scale, 0.2f);
    Calibrate();
  }


  void Calibrate()
  {
    calibrating = false;
    GameObject matObject = GameObject.Find("Mat");
    Vector3 origin = matObject.transform.position;
    Vector3 angle = matObject.transform.localEulerAngles;
    houseObject.transform.position = new Vector3(
      origin.x,
      origin.y + 0.08f,
      origin.z
    );
    houseObject.transform.localEulerAngles = angle;
    for (int i = 0; i < treeObjects.Length; i++) {
      Vector3 treePosition = treePositions[i];
      float x = origin.x + treePosition.x;
      float y = origin.y + treePosition.y;
      float z = origin.z + treePosition.z;
      Vector3 pos = new Vector3(
        origin.x + treePosition.x,
        origin.y + treePosition.y,
        origin.z + treePosition.z
      );
      pos = Quaternion.Euler(new Vector3(0, angle.y, 0)) * pos;
      GameObject treeObject = treeObjects[i];
      treeObject.transform.position = pos;
      treeObject.transform.localEulerAngles = angle;
    }

  }

  // Update is called once per frame
  void Update()
  {
    if (initializing) {
      Init();
    }

    if (calibrating) {
      Calibrate();
    }

    GameObject matObject = GameObject.Find("Mat");
    Vector3 origin = matObject.transform.position;
    Vector3 position = houseObject.transform.position;
    position.y = origin.y + 0.08f;
    houseObject.transform.position = position;

    Quaternion rotation = houseObject.transform.rotation;
    rotation.x = 0;
    rotation.z = 0;
    houseObject.transform.rotation = rotation;
  }
}
