using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OBS;
using OBS.Model;
using UnityEngine;

public class HWCloudIns 
{
    

    private HWCloudIns() { }
    private static HWCloudIns _HWCloud = null;
    
    private string HW_Bucket;
    private string HW_url;
    private ThreadStart HwDoing;
    private Thread HW_Thread;
    
    private ManualResetEvent ThreadCtrl;
    private bool IsUpload;
    private List<string> UnLoadFileList_URL;

    private string AK;
    private string SK;
    private string EndPoint;
    public static HWCloudIns GetIns()
    {
        if (_HWCloud == null)
        {

            _HWCloud = new HWCloudIns();
        }
        return _HWCloud;
    }

/// <summary>
/// hw_url cloud's File-URL(Begin of the bucket's next);
/// hwBucket target's Bucket
/// </summary>
/// <param name="hw_url "></param>
/// <param name="hwBucket target's Bucket">cloud's Bucket name</param>
    public void HwThreadInit(string hw_url,string hwBucket,string ak,string sk,string endpoint)
    {
        HW_url = hw_url;
        HW_Bucket = hwBucket;
        AK = ak;
        SK = sk;
        EndPoint = endpoint;
        
        
        if (HW_Thread != null || HwDoing != null)
        {
            HW_Thread.Abort();
            HW_Thread = null;
            HwDoing = null;
        }
        if (HW_Thread == null && HwDoing == null)
        {
            ThreadCtrl = new ManualResetEvent(true);
            HwDoing = new ThreadStart(() => ThreadManager());
            HW_Thread = new Thread(HwDoing);
            HW_Thread.Start();
        }
        else
        {
            HW_Thread.Abort();
            HW_Thread = null;
            HwDoing = null;
            Debug.Log("Clear UpLoad Thread");
            HwThreadInit(HW_url,HW_Bucket,AK,SK,EndPoint);
        }
    }

    public void AddFileUrlToUpLoad(string file_url)
    {
        if (UnLoadFileList_URL == null)
        {
            UnLoadFileList_URL = new List<string>();
        }
        UnLoadFileList_URL.Add(file_url);
    }

    private void ThreadManager()
    {
        while (UnLoadFileList_URL.Count>0)
        {
            UploadFileHW(UnLoadFileList_URL[0]);
            ThreadCtrl.WaitOne();
            UnLoadFileList_URL.RemoveAt(0);
        }
    }

    private void UploadFileHW(string FileUrl)
    {
        ThreadCtrl.Reset();
        if (string.IsNullOrEmpty(FileUrl))
        {
            Debug.Log("Cancel Upload File ");
            return;
        }
        if (string.IsNullOrEmpty(HW_url))
        {
            Debug.Log("Not Upload Target Url");
            return;
        }
        string LocalHW_url = HW_url + "/" + System.IO.Path.GetFileName(FileUrl);
        ObsClient client = new ObsClient(AK, SK, EndPoint);
// 上传文件

        try
        {
            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = HW_Bucket,
                ObjectKey = LocalHW_url,
                FilePath = FileUrl,// 上传的本地文件路径，需要指定到具体的文件名
            };
            /*// 以传输字节数为基准反馈上传进度
            request.ProgressType = ProgressTypeEnum.ByBytes;
            // 每上传1KB数据反馈上传进度
            request.ProgressInterval = 1024;

            // 注册上传进度回调函数
            request.UploadProgress += delegate(object sender, TransferStatus status){
                // 获取上传平均速率
                Debug.Log("Upload Completed");
                Debug.Log("AverageSpeed: >> "+status.AverageSpeed / 1024  + "KB/S");
                // 获取上传进度百分比
                Debug.Log("TransferPercentage: >> "+ status.TransferPercentage);
                if (status.TransferPercentage == 100)
                {
                    HW_Thread.Abort();
                    Debug.Log("Thread Close");
                    HW_Thread = null;
                    HwDoing = null;

                }
            };*/
            PutObjectResponse response = client.PutObject(request);
            Debug.Log("<color=#00EEEE> put object response: >> "+ response.ObjectUrl+"</color>");
            Debug.Log("<color=#85FF00> FileURL: >> "+""+HW_url + "/" + System.IO.Path.GetFileName(FileUrl)+"</color>");
            ThreadCtrl.Set();
        }
        catch (ObsException ex)
        {
            Debug.Log("UpLoad Fail");
            Debug.Log("ErrorCode: >> ");
            Debug.Log(ex.ErrorCode);
            Debug.Log("ErrorMessage: >> ");
            Debug.Log( ex.ErrorMessage);
            Debug.Log("Thread Close");
            ThreadCtrl.Set();
        }
        
        
    }
}
