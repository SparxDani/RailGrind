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
    [SerializeField] private float rotSpeed = 5f;


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
        Vector3 rotation = vehicle.transform.eulerAngles;

        //Debug.Log($"Rotación Original - X: {rotation.x}, Y: {rotation.y}, Z: {rotation.z}");

        rotation.x = NormalizeAngle(rotation.x);
        rotation.z = NormalizeAngle(rotation.z);

        //Debug.Log($"Rotación Normalizada - X: {rotation.x}, Y: {rotation.y}, Z: {rotation.z}");

        A = Mathf.Clamp(100 - Mathf.Abs(rotation.x), 0, 200);

        B = Mathf.Clamp(100 - Mathf.Abs(rotation.z), 0, 200);
        C = Mathf.Clamp(100 + Mathf.Abs(rotation.z), 0, 200);

        float diffA = A - 100;
        B = Mathf.Clamp(B - diffA, 0, 200);
        C = Mathf.Clamp(C - diffA, 0, 200);

        Debug.Log($"Valores Calculados - A: {A}, B: {B}, C: {C}");
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360; 
        if (angle > 180) angle -= 360; 
        return angle;
    }


    //void CalcularRotacion()
    //{


    //    Debug.Log("vehicle euler " + vehicle.eulerAngles);

    //    if (vehicle.eulerAngles.z > 0 && vehicle.eulerAngles.z < 180)
    //    {
    //        B = Mathf.Lerp(B, 150, Time.deltaTime * SmoothEngine); 
    //        C = Mathf.Lerp(C, 50, Time.deltaTime * SmoothEngine);
    //    }
    //    else if (vehicle.eulerAngles.z >= 180 && vehicle.eulerAngles.z <= 360)
    //    {
    //        B = Mathf.Lerp(B, 50, Time.deltaTime * SmoothEngine); 
    //        C = Mathf.Lerp(C, 150, Time.deltaTime * SmoothEngine); 
    //    }

    //    if (vehicle.eulerAngles.x > 0 && vehicle.eulerAngles.x < 180)
    //    {
    //        A = Mathf.Lerp(A, 150, Time.deltaTime * SmoothEngine); 
    //    }
    //    else if (vehicle.eulerAngles.x >= 180 && vehicle.eulerAngles.x <= 360)
    //    {
    //        A = Mathf.Lerp(A, 50, Time.deltaTime * SmoothEngine); 
    //    }

    //    if (A > 0)
    //    {
    //        if (B > 0 || C > 0)
    //        {
    //            float decrement = Time.deltaTime * SmoothEngine * (A / 100f);

    //            if (B > 0)
    //                B = Mathf.Max(B - decrement, 0);

    //            if (C > 0)
    //                C = Mathf.Max(C - decrement, 0);
    //        }
    //    }
    //}




    //Vector3 targetDir = Vector3.zero;
    //targetDir += transform.forward * (A - 125);
    //targetDir += transform.right * (B - 125);
    //targetDir.Normalize();
    //targetDir.y = 0;
    //if (targetDir == Vector3.zero)
    //{
    //    targetDir = transform.forward;
    //}
    //Quaternion targetRot = Quaternion.LookRotation(targetDir);
    //transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);

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


            engineA.text = ((int)A).ToString();
            engineB.text = ((int)B).ToString();
            engineC.text = ((int)C).ToString();

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

    public void sendString(string message)
    {

        //try
        //{
        //    // Bytes empfangen.
        //    if (message != "")
        //    {

        //        //byte[] data = StringToByteArray(message);
        //        //print(message);
        //        // Den message zum Remote-Client senden.
        //        //client.Send(data, data.Length, remoteEndPoint);

        //    }


        //}
        //catch (Exception err)
        //{
        //    print(err.ToString());
        //}
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

        //Debug.Log(B);

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

