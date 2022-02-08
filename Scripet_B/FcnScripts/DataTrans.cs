using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataTrans
{
  
    private DataTrans() { }
    private static DataTrans _DataTrans = null;
    public static DataTrans DataTransIns()
    {
        if (_DataTrans == null)
        {

            _DataTrans = new DataTrans();
        }
        return _DataTrans;
    }
    /// <summary>
    /// No DataTrans ,just write to File
    /// </summary>
    /// <param name="data"></param>
    /// <param name="filePath"></param>
    public void JsonWriteToFile(string data,string filePath)
    {
        Debug.Log(data);
        using (FileStream fileStream=new FileStream(filePath,FileMode.Create,FileAccess.Write)) {
            using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.UTF8)) {
                textWriter.Write (data);
            }
        }
        Debug.Log("保存成功");
    }
    /// <summary>
    /// DataTrans and write to File
    /// </summary>
    /// <param name="data"></param>
    /// <param name="filePath"></param>
    public void JsonDataSaveToFile(string data,string filePath)
    {
        string jsonData;
        jsonData  = JsonMapper.ToJson(data);
        Debug.Log(jsonData);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("保存成功");
    }
    /// <summary>
    /// DataTrans and write to File,And choice is show File's Fold
    /// </summary>
    /// <param name="data"></param>
    /// <param name="filePath"></param>
    public void JsonDataSaveToFile(string data,string filePath,bool isShowFileFold)
    {
        string jsonData;
        jsonData  = JsonMapper.ToJson(data);
        Debug.Log(jsonData);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("保存成功");
        string JsonFilePath;
        if (isShowFileFold)
        {
            JsonFilePath = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
            JsonFilePath = JsonFilePath.Replace("/", "\\");
            System.Diagnostics.Process.Start("explorer", JsonFilePath);
        }
    }


    
    
    /// <summary>
    /// Angle Normalize Trans
    /// </summary>
    /// <param name="mTransform"></param>
    /// <returns></returns>
    public Vector3 AngleNormalizeTrans(Transform mTransform)
    {
        Vector3 angle = mTransform.eulerAngles;
        float x = angle.x;
        float y = angle.y;
        float z = angle.z;
 
        if (Vector3.Dot(mTransform.up, Vector3.up) >= 0f)
        {
            if (angle.x >= 0f && angle.x <= 90f)
            {
                x = angle.x;
            }
            if (angle.x >= 270f && angle.x <= 360f)
            {
                x = angle.x - 360f;
            }
        }
        if (Vector3.Dot(mTransform.up, Vector3.up) < 0f)
        {
            if (angle.x >= 0f && angle.x <= 90f)
            {
                x = 180 - angle.x;
            }
            if (angle.x >= 270f && angle.x <= 360f)
            {
                x = 180 - angle.x;
            }
        }
 
        if (angle.y > 180)
        {
            y = angle.y - 360f;
        }
 
        if (angle.z > 180)
        {
            z = angle.z - 360f;
        }
        Vector3 vector3 = new Vector3(Mathf.Round(x), Mathf.Round(y), Mathf.Round(z));
        //Debug.Log(" Inspector Euler:  " + Mathf.Round(x) + " , " + Mathf.Round(y) + " , " + Mathf.Round(z));
        return vector3;
    }
    
    /// <summary>
    ///  DownLoad File From Internal
    /// </summary>
    /// <param name="HttpUrl"></param> Internal FileUrl
    /// <param name="LocalFileUrl"></param> Local FileURL
    public void GetInternetJson(string HttpUrl,string LocalFileUrl)
    {
        string Httpjson = HttpUrl;
        
        System.Net.HttpWebRequest Request = (HttpWebRequest)(WebRequest.Create(new Uri(Httpjson)));           
        Request.Method = "GET";
        Request.MaximumAutomaticRedirections = 4;
        Request.MaximumResponseHeadersLength = 4;            
        Request.ContentLength = 0;          
        StreamReader ReadStream = null;
        HttpWebResponse Response = null;
        string ResponseText = string.Empty;                                                                
        Response = (HttpWebResponse)(Request.GetResponse());
        Stream ReceiveStream = Response.GetResponseStream();
        ReadStream = new StreamReader(ReceiveStream, System.Text.Encoding.UTF8);
        ResponseText = ReadStream.ReadToEnd();
        Response.Close();
        ReadStream.Close();
        File.WriteAllText(LocalFileUrl, ResponseText);
        Debug.Log("保存成功>>"+Httpjson);
        
    }

    public void EmptyCheck<T>(T obj)
    {
        if (obj == null)
        {
            
        }
        
    }
}
