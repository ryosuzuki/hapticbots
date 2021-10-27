using UnityEngine;
using Socket.Quobject.SocketIoClientDotNet.Client;

public class NetworkManager : MonoBehaviour
{
  // github.com/Rocher0724/socket.io-unity
  public static QSocket socket;
  string serverURL = "http://10.0.1.5:3000/";

  void Start()
  {
    socket = IO.Socket(serverURL);

    socket.On(QSocket.EVENT_CONNECT, () => {
      Debug.Log ("Connected");
    });

    socket.On("pos", data => {
      Robots.robotsJSON = Robots.FromJson<Robots.RobotJSON>(data.ToString());
    });

    socket.On("calibrate", data => {
      Debug.Log(data.ToString());
      Mat.calibrateCommand = data.ToString();
    });
  }

  void Update()
  {
  }

  private void OnDestroy () {
    socket.Disconnect ();
  }
}
