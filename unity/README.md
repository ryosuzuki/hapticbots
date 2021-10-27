# Oculus Quest Basics

[How to get started with Oculus Quest and Unity on macOS](https://medium.com/@sofaracing/how-to-develop-for-oculus-quest-on-macos-with-unity-5aa487b80d13)

[Unity Tutorial-Grabbing an object in VR(Oculus Quest) with Unity](https://medium.com/@shailendra2011991/grabbing-an-object-in-vr-oculus-quest-with-unity-1cc20510bde6)

[Hand Tracking in Unity
](https://developer.oculus.com/documentation/unity/unity-handtracking/?locale=en_US)

[TCG VR #1 | Oculus Quest Hand Tracking | Pinch, Grab, Poke](https://www.youtube.com/watch?v=fSDejIg2emU)

## Build Settings

```
Import Oculus Integration

Build Settings > Android

Player Settings > XR Settings > Virtual Reality Supported (Oculus)

Other Settings > Swap OpenGLSE3 and Vulkan (for Mac)

Build
```


## Camera and Controller

```
Delete Main Camera

Add OVRCameraRig

Add LocalAvatar in TrackingSpace
```


## Camera and Hand Tracking

```
OVRCameraRig > OVR Manager > Check Quest

OVRCameraRig > OVR Manager > Hand Tracking Support > Controllers and Hands

OVRCameraRig > TrackingSpace > LeftHandAnchor > OVRHandPrefab

OVRHandPrefab > OVRHand, OVRSkeleton, OVRMesh > Left/Right hand
```



## Grasping

```
Cube > Add Rigid Body

Cube > Add OVR Grabbable
```

```
LeftHandAnchor/RightHandAnchor

Add a Sphere Collider

Set the radius to 0.05

Check the Is Trigger box

Add an OVR Grabber
```


```
Error: The type or namespace name 'Label' does not exist in the namespace 'System.Reflection.Emit'

Build Settings > Player Settings > API Compatibility Level > .NET 4.x
```


# Hand Tracking Grabbing
See [TCG VR #1 | Oculus Quest Hand Tracking | Pinch, Grab, Poke](https://www.youtube.com/watch?v=fSDejIg2emU), but make sure to uncheck `gravity`

```
OVRCameraRig > LeftHandAnchor > OVRHandPrefab > Add Sphere Collider (IsTrigger: On, Radius: 0.2)

OVRCameraRig > LeftHandAnchor > OVRHandPrefab > Rigidbody (Use Grabity: Off)

OVRCameraRig > LeftHandAnchor > OVRHandPrefab > Add HandGrabber Script (below)

OVRCameraRig > LeftHandAnchor > OVRHandPrefab > HandGrabber (Parent Held Object: On, Grip Transform: OVRHandPrefab, Controller: L_Hand, Grab Volumes: Size: 1, OVRHandPrefab)
```


```
Sphere > Add OVRGrabbable (Size: 1, Element: Sphere)
```


```cs
// HandGrabber.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class HandGrabber : OVRGrabber
{
  private OVRHand m_hand;
  private float pinchThreshold = 0.7f;

  protected override void Start()
  {
    base.Start();
    m_hand = GetComponent<OVRHand>();
  }

  // Update is called once per frame
  public override void Update()
  {
    base.Update();
    CheckIndexPinch();
  }

  void CheckIndexPinch()
  {
    float pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

    if(!m_grabbedObj && pinchStrength > pinchThreshold && m_grabCandidates.Count > 0)
    {
      GrabBegin();
    }
    else if(m_grabbedObj && ! (pinchStrength > pinchThreshold))
    {
      GrabEnd();
    }
  }
}
```


```
Error in Newtonsoft.JSON

Download https://github.com/JamesNK/Newtonsoft.Json/releases/tag/8.0.3 and replace

Assets/PolyToolkit/ThirdParty/Json-NET-for-Unity/Assemblies/AOT/Newtonsoft.Json.dll
and
Assets/PolyToolkit/ThirdParty/Json-NET-for-Unity/Assemblies/Standalone/Newtonsoft.Json.dll
```


# Hammer

```
RigidBody

Mesh Collider

OVR Grabbable

Size: 1
Element: node_MeshObject
```


# Hand Pushing and Poking
https://raspberly.hateblo.jp/entry/OVRFist

```
Attach HandPusher.cs
```


