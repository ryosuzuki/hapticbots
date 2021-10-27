using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
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
    if (collider.tag == "Block") {
      Debug.Log("Destory");
      Destroy(collider.gameObject);
      NetworkManager.socket.Emit("minecraft", "destroy");
    }

  }

}
