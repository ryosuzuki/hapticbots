using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Robots : MonoBehaviour
{
  public GameObject robotPrefab;

  public static GameObject[] robotObjects;
  public static GameObject[] targetObjects;

  public static RobotJSON[] robotsJSON;
  public static Vector3[] targetPositions;
  public static bool[] targetFixed;

  GameObject mat;
  GameObject robot;
  float max = 450f;
  float min = 50f;

  int count = 0;
  Vector3 prev;
  Vector3 offset = new Vector3(0, 0, 0);

  void Start()
  {
    mat = GameObject.Find("Mat");
    // robot = Instantiate(robotPrefab);

    // robotObjects = new GameObject[0];
    // targetObjects = new GameObject[0];
  }

  void Update()
  {
    // UpdateRobotsPos();
    // UpdateRobotsHeight();

    TargetJSON[] targetsJSON = new TargetJSON[targetPositions.Length];
    try {
      Vector3 fingerPos = Mat.fingerPos;
      float heightAtFinger = GetHeight(fingerPos);
      if (heightAtFinger >= 0) {
        float max = 10000;
        int closestIndex = -1;
        for (int i = 0; i < targetPositions.Length; i++) {
          Vector3 targetPos = targetPositions[i];
          float dist = Mathf.Pow(fingerPos.x - targetPos.x, 2) + Mathf.Pow(fingerPos.z - targetPos.z, 2);
          if (dist < max) {
            max = dist;
            closestIndex = i;
          }
        }
        bool isFixed = targetFixed[closestIndex];
        if (isFixed != true) {
          targetPositions[closestIndex] = fingerPos;
        }
      }
    } catch (Exception e) {
      Debug.Log(e);
    }

    for (int i = 0; i < targetPositions.Length; i++) {
      Vector3 targetPos = targetPositions[i];
      float height = GetHeight(targetPos);
      int tick = ConvertFromUnity(height);
      Vector3 origin = mat.transform.position;
      Vector3 originAngle = mat.transform.localEulerAngles;
      Vector3 matTargetPos = new Vector3();
      matTargetPos.x = targetPos.x - origin.x;
      matTargetPos.z = targetPos.z - origin.z;
      matTargetPos = Quaternion.Euler(new Vector3(0, -originAngle.y, 0)) * matTargetPos;
      Vector3 pos = ConvertTargetPos(matTargetPos);
      TargetJSON targetJSON = new TargetJSON();

      float angle = 0;
      // For house
      /*
      bool isFixed = targetFixed[i];
      if (!isFixed) {
        angle = 180;
      }
      */
      targetJSON.x = pos.x;
      targetJSON.y = pos.y;
      targetJSON.angle = angle;
      targetJSON.height = height;
      targetJSON.tick = tick;
      targetsJSON[i] = targetJSON;

      if (Mat.showTargets) {
        if (i >= targetObjects.Length) {
          continue;
        }
        if (height >= 0) {
          GameObject targetObject = targetObjects[i];
          Vector3 scale = targetObject.transform.localScale;
          targetPos.y = mat.transform.position.y + 0.01f;
          scale.y = height - 0.02f;
          targetPos.y = targetPos.y + height/2;
          targetObject.transform.position = targetPos;
          targetObject.transform.localScale = scale;
          targetObjects[i] = targetObject;
        }
      }
    }
    string json = Robots.ToJson<Robots.TargetJSON>(targetsJSON);
    if (Mat.calibrated) {
      NetworkManager.socket.Emit("move", json);
    }

  }

  public void UpdateRobotsPos()
  {
    // robotsJSON = FromJson<RobotJSON>(data.ToString());
    // { robotsJSON: [ robot_1, robot_2 ], targets: [target_1, target_2] }
    if (robotsJSON == null) return;
    for (int i = 0; i < robotsJSON.Length; i++) {
      RobotJSON robotJSON = robotsJSON[i];
      Vector3 origin = mat.transform.position;
      Vector3 originAngle = mat.transform.localEulerAngles;

      Vector3 pos = new Vector3();
      Vector3 angle = new Vector3();
      float size = max - min;
      float mid = (max - min) / 2 + min;
      pos.x = - (robotJSON.x - mid) / (max - mid) * 0.28f;
      pos.z =  (robotJSON.y - mid) / (max - mid) * 0.28f;
      pos.y = origin.y + 0.01f;

      pos = Quaternion.Euler(new Vector3(0, originAngle.y, 0)) * pos;
      pos.x = pos.x + origin.x;
      pos.z = pos.z + origin.z;
      angle.y = robotJSON.angle;

      robot.transform.position = pos;
      robot.transform.localEulerAngles = angle;
    }
  }

  public void UpdateRobotsHeight()
  {
    Transform current = robot.transform;
    Vector3 pos = current.position;
    Vector3 angle = current.localEulerAngles;
    Vector3 origin = mat.transform.position;
    Vector3 scale = current.localScale;
    float height = GetHeight(pos);
    // print(height);
    if (height >= 0) {
      scale.y = height - 0.02f;
      pos.y = pos.y + height/2;
      current.localScale = scale;
      current.position = pos;

      int target = ConvertFromUnity(height);
      ReelJSON reel = new ReelJSON();
      reel.id = 0; // i;
      reel.target = target;
      string json = JsonUtility.ToJson(reel);
      bool calibrated = Mat.calibrated;
      if (calibrated) {
        NetworkManager.socket.Emit("reel-move", json);
      }
    }
  }

  float GetHeight(Vector3 pos)
  {
    Vector3 origin = mat.transform.position;
    float x = pos.x;
    float z = pos.z;

    float[] xs = { x-0.01f, x, x+0.01f };
    float[] zs = { z-0.01f, z, z+0.01f };

    float height = -1.0f;
    string key = "";
    foreach (float xc in xs) {
      foreach (float zc in zs) {
        key = string.Format("{0:F2}-{1:F2}", xc, zc);
        if (Mat.hash[key] != null) {
          height = (float) Mat.hash[key];
        }
        if (height > 0)
          break;
      }
      if (height > 0)
        break;
    }
    // string key1 = string.Format("{0:F2}-{1:F2}", x, z);
    // print(Mat.hash[key]);
    // print(key);
    return height;
    // if (Mat.hash[key] != null) {
    //   float height = (float) Mat.hash[key];
    //   return height;
    // } else {
    //   return -1.0f;
    // }
  }

  Vector3 ConvertTargetPos(Vector3 targetPos) {
    offset = targetPos - prev;
    if (0f < offset.x && offset.x < 0.02f) {
      offset.x = 0.02f;
      offset.z = 0;
    }
    if (offset.x >= 0.02f) {
      prev = targetPos;
    }
    if (0f > offset.x && offset.x > -0.02f) {
      offset.x = -0.02f;
      offset.z = 0;
    }
    if (offset.x <= -0.02f) {
      prev = targetPos;
    }
    /*
    if (Mathf.Abs(offset.x) > Mathf.Abs(offset.z)) {
      if (Mathf.Abs(offset.x) < 0.02) {
        offset.x = 0.02f;
        offset.z = 0;
      }
    } else {
      if (Mathf.Abs(offset.z) < 0.02) {
        offset.x = 0;
        offset.z = 0.02f;
      }
    }
    // offset.x = 0;
    // offset.z = 0;
    */

    // pos.x =  (robotJSON.x - mid) / (max - mid) * 0.28f + origin.x;
    // pos.z = - (robotJSON.y - mid) / (max - mid) * 0.28f + origin.z;
    offset = new Vector3();
    float size = max - min;
    float mid = (max - min) / 2 + min;
    Vector3 pos;
    pos.x = - (targetPos.x + offset.x) / 0.28f * (max - mid) + mid;
    pos.y =  (targetPos.z + offset.z) / 0.28f * (max - mid) + mid;
    pos.z = 0;
    return pos;
  }

  int ConvertFromUnity(float height)
  {
    /*
    0: 8cm, 1000: +4cm, 5000: 28cm (8cm + 4cm x 5)
    -  0 80
    - 1000 120
    - 2100 160
    - 3100 200
    - 4000 240
    - 5000 280
    h = 0.12 -> (0.12 - 0.08) = 0.04
    + 4cm -> 4 cm * 1000 / 4 -> 1000
    0.04 * 1000 / 0.04
    */
    int min = 200;
    int max = 6000;
    int target = (int) ((height - 0.08f) * 1.2 * 1000f / 0.04f);
    // target = target * 2;
    if (target < min) {
      target = min;
    }
    if (target > max) {
      target = max;
    }
    return target; // return rotation count
  }

  float ConvertFromRotation(int height)
  {
    return (float) height * 0.04f / 1000f + 0.08f; // return meter
  }


  public static T[] FromJson<T>(string json)
  {
    Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
    return wrapper.robots;
  }

  public static string ToJson<T>(T[] array)
  {
    Wrapper<T> wrapper = new Wrapper<T>();
    wrapper.robots = array;
    return JsonUtility.ToJson(wrapper);
  }

  // public static string ToJson<T>(T[] array)
  // {
  //   Wrapper<T> wrapper = new Wrapper<T>;
  //   wrapper.robots = array;
  //   return JsonUtility.ToJson(wrapper);
  // }

  [Serializable]
  private class Wrapper<T>
  {
    public T[] robots;
  }

  [Serializable]
  public class RobotJSON
  {
    public int numId;
    public float x;
    public float y;
    public float angle;

    public static RobotJSON CreateFromJSON(string data)
    {
      return JsonUtility.FromJson<RobotJSON>(data);
    }
  }

  [Serializable]
  public class TargetJSON
  {
    public float x;
    public float y;
    public float angle;
    public float height;
    public int tick;

    public static TargetJSON CreateFromJSON(string data)
    {
      return JsonUtility.FromJson<TargetJSON>(data);
    }

    // public static TargetJSON CreateFromData(TargetJSON targetJSON)
    // {
    //   return JsonUtility.ToJson<TargetJSON>(targetJSON);
    // }
  }

  [Serializable]
  public class ReelJSON
  {
    public int id;
    public int target;

    public static ReelJSON CreateFromJSON(string data)
    {
      return JsonUtility.FromJson<ReelJSON>(data);
    }
  }



}
