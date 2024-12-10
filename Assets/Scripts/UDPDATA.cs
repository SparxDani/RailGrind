using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPDATA
{
    public AppControlField mAppControlField;
    public AppWhoField mAppWhoField;
    public AppDataField mAppDataField;
    public UDPDATA()
    {
        mAppControlField = new AppControlField();
        mAppWhoField = new AppWhoField();
        mAppDataField = new AppDataField();
    }
    public string GetToString()
    {
        return mAppControlField.GetToString() + mAppWhoField.GetToString() + mAppDataField.GetToString();
    }
}
public class AppWhoField
{
    public string AcceptCode { get; set; }
    public string ReplyCode { get; set; }
    public AppWhoField()
    {
        AcceptCode = "ffffffff";
        ReplyCode = "00000000";
    }
    public string GetToString()
    {

        return AcceptCode + ReplyCode;
    }
}

public class AppDataField
{
    public string RelaTime { get; set; }
    public string RegStartAddress { get; set; }
    public string RegNum { get; set; }
    public string ExtData { get; set; }
    public string PlayMotor { get; set; }
    public string PlayMotorA { get; set; }
    public string PlayMotorB { get; set; }
    public string PlayMotorC { get; set; }
    public string PortOut { get; set; }
    public string PlayDAC { get; set; }
    public AppDataField()
    {
        RelaTime = "00000064";
        RegStartAddress = "00000000";
        RegNum = "";
        ExtData = "";
        PlayMotor = "";
        PortOut = "12345678";
        PlayDAC = "abcd";
    }
    public string GetToString()
    {
        return RelaTime + RegStartAddress + RegNum + PlayMotorA + PlayMotorB + PlayMotorC + PortOut + PlayDAC;
    }
}
public class AppControlField
{
    public string ConfirmCode { get; set; }
    public string PassCode { get; set; }
    public string FunctionCode { get; set; }
    public string ObjectChannel { get; set; }
    public AppControlField()
    {
        ConfirmCode = "55aa";
        PassCode = "0000";
        FunctionCode = "1401";
        ObjectChannel = "0000";
    }
    public string GetToString()
    {

        return ConfirmCode + PassCode + FunctionCode + ObjectChannel;
    }
}