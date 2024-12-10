using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Slider = UnityEngine.UI.Slider;

public class UDPManager : MonoBehaviour
{
    IPEndPoint remoteEndPoint;
    UDPDATA mUDPDATA = new UDPDATA();
    [SerializeField] private float rotSpeed = 5f; // Puedes ajustar este valor en el Inspector


    private string IP;  
    public int port;
    
    public TMP_Text engineA;
    public TMP_Text engineAHex;
    public Slider sliderA;
    public TMP_Text engineB;
    public TMP_Text engineBHex;
    public Slider sliderB;
    public TMP_Text engineC;
    public TMP_Text engineCHex;
    public Slider sliderC;

    public TMP_Text Data;

    UdpClient client;

    public bool active = false;

    public float SmoothEngine = 0.5f;

    public float A = 0, B = 0, C = 0, longg;

    public Transform vehicle;

    public void Start()
    {
        init();
    }
    public void init()
    {

        // define
        IP = "192.168.15.201";
        port = 7408;

        // ----------------------------
        // Senden
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient(53342);


        // AppControlField
        mUDPDATA.mAppControlField.ConfirmCode = "55aa";
        mUDPDATA.mAppControlField.PassCode = "0000";
        mUDPDATA.mAppControlField.FunctionCode = "1301";
        // AppWhoField
        mUDPDATA.mAppWhoField.AcceptCode = "ffffffff";
        mUDPDATA.mAppWhoField.ReplyCode = "";//"00000001";
                                             // AppDataField
        mUDPDATA.mAppDataField.RelaTime = "00000064";
        mUDPDATA.mAppDataField.PlayMotorA = "00000000";
        mUDPDATA.mAppDataField.PlayMotorB = "00000000";
        mUDPDATA.mAppDataField.PlayMotorC = "00000000";

        mUDPDATA.mAppDataField.PortOut = "12345678";

        A = 125;
        B = 125;
        C = 125;

        sliderA.value = A;
        sliderB.value = B;
        sliderC.value = C;

        string HexA = DecToHexMove(A);
        string HexB = DecToHexMove(B);
        string HexC = DecToHexMove(C);

        engineAHex.text = "Engine A: " + HexA;
        engineBHex.text = "Engine B: " + HexB;
        engineCHex.text = "Engine C: " + HexC;

        mUDPDATA.mAppDataField.PlayMotorC = HexC;
        mUDPDATA.mAppDataField.PlayMotorA = HexA;
        mUDPDATA.mAppDataField.PlayMotorB = HexB;


        engineA.text = ((int)sliderA.value).ToString();
        engineB.text = ((int)sliderB.value).ToString();
        engineC.text = ((int)sliderC.value).ToString();

        Data.text = "Data: " + mUDPDATA.GetToString();

        sendString(mUDPDATA.GetToString());

        StartCoroutine(UpMovePlatform(3));
    }
    public void ActiveSend()
    {
        active = true;

    }
    public void ResertPositionEngine()
    {

        mUDPDATA.mAppDataField.RelaTime = "00001F40";

        mUDPDATA.mAppDataField.PlayMotorA = "00000000";
        mUDPDATA.mAppDataField.PlayMotorB = "00000000";
        mUDPDATA.mAppDataField.PlayMotorC = "00000000";

        sendString(mUDPDATA.GetToString());

        mUDPDATA.mAppDataField.RelaTime = "00000064";

    }

    IEnumerator UpMovePlatform(float wait)
    {
        active = false;

        yield return new WaitForSeconds(3f);

        active = true;
    }

    void CalcularRotacion()
    {
        // Calcula el input basado en A y B (adelante/atrás y derecha/izquierda)
        Vector3 targetDir = Vector3.zero;
        targetDir += transform.forward * (A - 125); // Asume que 125 es el valor neutral
        targetDir += transform.right * (B - 125); // Asume que 125 es el valor neutral

        // Normaliza la dirección para evitar movimientos erráticos
        targetDir.Normalize();

        // Asegura que no se modifique la dirección en el eje Y
        targetDir.y = 0;

        // Si no hay input, mantén la dirección actual
        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        // Calcula la rotación objetivo
        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        // Interpola suavemente hacia la rotación objetivo
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
    }



    void FixedUpdate()
    {
        if (active)
        {

            CalcularRotacion();

            sliderA.value = A;
            sliderB.value = B;
            sliderC.value = C;

            string HexA = DecToHexMove(A);
            string HexB = DecToHexMove(B);
            string HexC = DecToHexMove(C);

            engineAHex.text = "Engine A: " + HexA;
            engineBHex.text = "Engine B: " + HexB;
            engineCHex.text = "Engine C: " + HexC;

            mUDPDATA.mAppDataField.PlayMotorC = HexC;
            mUDPDATA.mAppDataField.PlayMotorA = HexA;
            mUDPDATA.mAppDataField.PlayMotorB = HexB;


            engineA.text = ((int)sliderA.value).ToString();
            engineB.text = ((int)sliderB.value).ToString();
            engineC.text = ((int)sliderC.value).ToString();

            Data.text = "Data: " + mUDPDATA.GetToString();

            sendString(mUDPDATA.GetToString());
        }
    }

    void OnApplicationQuit()
    {

        ResertPositionEngine();



        if (client != null)
            client.Close();
        Application.Quit();
    }

    byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    string DecToHexMove(float num)
    {
        int d = (int)((num / 5f) * 10000f);
        return "000" + d.ToString("X");
    }

    private void sendString(string message)
    {

        try
        {
            // Bytes empfangen.
            if (message != "")
            {

                //byte[] data = StringToByteArray(message);
                print(message);
                // Den message zum Remote-Client senden.
                //client.Send(data, data.Length, remoteEndPoint);

            }


        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    void OnDisable()
    {

        if (client != null)
            client.Close();
    }

    private void OnDrawGizmos()
    {

        // rotate left or Right
        Vector3 FG1 = vehicle.position + Vector3.forward * longg;
        Vector3 FG2 = vehicle.position + vehicle.forward * longg;
        Gizmos.color = Color.black;
        Gizmos.DrawLine(FG1, FG2);
        float d = (FG1 - FG2).magnitude;
        float dMax = 5;
        float dN = d / dMax;
        float Increment = dN * 100;
        Vector3 cross = Vector3.Cross(vehicle.forward, Vector3.forward);
        if (cross.x < 0)
            Increment *= -1;
        float FinalValue = 100 + Increment;
        B = Mathf.Lerp(B, FinalValue, Time.deltaTime * 20f);
        B = Mathf.Clamp(B, 0, 200);

        Debug.Log(B);

        #region Axis WordSpace
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Vector3.forward * longg, 0.5f);
        Gizmos.DrawLine(Vector3.zero, Vector3.forward * longg);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Vector3.right * longg, 0.5f);
        Gizmos.DrawLine(Vector3.zero, Vector3.right * longg);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Vector3.up * longg, 0.5f);
        Gizmos.DrawLine(Vector3.zero, Vector3.up * longg);
        #endregion


        #region Axis Vechicle
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(vehicle.position + vehicle.forward * longg, 0.5f);
        Gizmos.DrawLine(vehicle.position, vehicle.position + vehicle.forward * longg);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(vehicle.position + vehicle.right * longg, 0.5f);
        Gizmos.DrawLine(vehicle.position, vehicle.position + vehicle.right * longg);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(vehicle.position + vehicle.up * longg, 0.5f);
        Gizmos.DrawLine(vehicle.position, vehicle.position + vehicle.up * longg);
        #endregion


    }

}

