using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
  void Start()
  {

  }

  void Update()
  {

  }

  void OnTriggerEnter(Collider collider)
  {
    Debug.Log(collider.tag);
    if (collider.tag == "hammer") {
      print("collide block");
    }

  }

}
