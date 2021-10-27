using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{

  public GameObject treePrefab;

  // Start is called before the first frame update
  void Start()
  {
    Vector3 origin = new Vector3();
    for (int i = 0; i < 3; i++) {
      for (int j = 0; j < 3; j++) {
        float x = origin.x + (float) i * 0.05f - 0.05f;
        float z = origin.z + (float) j * 0.05f - 0.05f;
        float y = origin.y + 0.05f;
        GameObject treeObject = Instantiate(treePrefab);
        treeObject.transform.position = new Vector3(x, y ,z);
      }
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
