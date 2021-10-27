using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HandPusher : MonoBehaviour
{

  public const float THRESH_COLLISION_FLEX = 0.9f;
  private OVRGrabber m_grabber;
  [SerializeField]
  protected OVRInput.Controller m_controller;
  protected Collider[] m_colliders = null;
  protected bool firstUpdate = false;
  protected bool collisionEnabled;
  protected short m_grabCandidatesNum = 0;

  void Awake ()
  {
    m_grabber = GetComponent<OVRGrabber>();
    m_colliders = this.GetComponentsInChildren<Collider>().Where(childCollider => !childCollider.isTrigger).ToArray();
    foreach(var c in m_colliders) {
      c.isTrigger = true;
    }
  }

  void Start ()
  {
    firstUpdate = true;
  }

  void Update ()
  {
    if(firstUpdate) {
      foreach(var c in m_colliders) {
        c.isTrigger = false;
      }
      firstUpdate = false;
    }

    float flex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);

    if(m_grabber) {
      if(m_grabber.grabbedObject != null) {
        if(m_grabCandidatesNum != 0) {
          m_grabCandidatesNum = 0;
        }
        return;
      }
      collisionEnabled = m_grabber.grabbedObject == null && flex >= THRESH_COLLISION_FLEX && m_grabCandidatesNum == 0;
    } else {
      collisionEnabled = flex >= THRESH_COLLISION_FLEX && m_grabCandidatesNum == 0;
    }

    if(m_colliders.Length > 0 && collisionEnabled != m_colliders[0].enabled) {
      foreach(var c in m_colliders) {
        c.enabled = collisionEnabled;
      }
    }
  }

  // Dsiable when using OVRGrabber and OVRGrabbable
  void OnTriggerEnter (Collider otherCollider)
  {
    if(collisionEnabled) return;
    OVRGrabbable grabbable = otherCollider.GetComponent<OVRGrabbable>() ?? otherCollider.GetComponentInParent<OVRGrabbable>();
    if(grabbable == null) return;

    m_grabCandidatesNum++;
  }

  void OnTriggerExit (Collider otherCollider)
  {
    OVRGrabbable grabbable = otherCollider.GetComponent<OVRGrabbable>() ?? otherCollider.GetComponentInParent<OVRGrabbable>();
    if(grabbable == null) return;

    m_grabCandidatesNum--;
    if(m_grabCandidatesNum < 0) {
      m_grabCandidatesNum = 0;
    }
  }

}
