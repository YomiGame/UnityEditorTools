using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OBS;
using OBS.Model;
using qiniu;
using UnityEngine;




public class QNYCloud
{
    private QNYCloud() { }
    private static QNYCloud _QNYCloud = null;
    
    private string Qny_Bucket;
    private string QNY_url;
    private string AK;
    private string SK;
    private Thread QNY_Thread;
    private ThreadStart QNYDoing;

    
    private ManualResetEvent ThreadCtrl;
    private bool IsUpload;
    private List<string> UnLoadFileList_URL;
    
    
    public static QNYCloud GetIns()
    {
        if (_QNYCloud == null)
        {

            _QNYCloud = new QNYCloud();
        }
        return _QNYCloud;
    }
    /// <summary>
    /// qny_Url :cloud's File-URL(Begin of the bucket's next);qnyBucket:target's Bucket;ak:ACCESS_KEY;sk:SECRET_KEY
    /// </summary>
    /// <param name="qny_Url"></param>
    /// <param name="qnyBucket"></param>
    /// <param name="ak"></param>
    /// <param name="sk"></param>
    public void QNYThreadInit(string qny_Url,string qnyBucket,string ak,string sk)
    {
        QNY_url = qny_Url;
        Qny_Bucket = qnyBucket;
        AK = ak;
        SK = sk;
        if (QNY_Thread != null || QNYDoing != null)
        {
            QNY_Thread.Abort();
            QNY_Thread = null;
            QNYDoing = null;
        }
        if (QNY_Thread == null && QNYDoing == null)
        {
            ThreadCtrl = new ManualResetEvent(true);
            QNYDoing = new ThreadStart(() => ThreadManager());
            QNY_Thread = new Thread(QNYDoing);
            QNY_Thread.Start();
        }
        else
        {
            QNY_Thread.Abort();
            QNY_Thread = null;
            QNYDoing = null;
            Debug.Log("Clear UpLoad Thread");
            QNYThreadInit(QNY_url,Qny_Bucket,AK,SK);
        }
    }
    private void ThreadManager()
    {
        while (UnLoadFileList_URL.Count>0)
        {
            UploadFileQNY(UnLoadFileList_URL[0]);
            ThreadCtrl.WaitOne();
            UnLoadFileList_URL.RemoveAt(0);
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
    


    private void UploadFileQNY(string FileUrl)
    {
        if (FileUrl == null || FileUrl == "")
        {
            Debug.Log("Cancel Upload File ");
            return;
        }
        if (QNY_url == null || QNY_url == "")
        {
            Debug.Log("Not Upload Target Url");
            return;
        }

        //string LocalBucket = Bucket + ":" + QNY_url;
        qiniu.Config.ACCESS_KEY = AK;
        qiniu.Config.SECRET_KEY = SK;
        string LocalQNY_url = QNY_url + "/" + System.IO.Path.GetFileName(FileUrl);
        QiniuFile qfile = new QiniuFile (Qny_Bucket, LocalQNY_url, FileUrl);
        
        qfile.UploadCompleted += (sender, e) =>
        {
            Debug.Log("Upload Completed");
            Debug.Log("-><color=#00EEEE>"+e.key+"</color>");
            Debug.Log(e.Hash);
            //done.Set ();
        };
        qfile.UploadFailed += (sender, e) => {
            Debug.Log("UpLoad Fail");
            Debug.Log(e.Error.ToString ());
//					puttedCtx.Save();
            QNY_Thread.Abort();
            Debug.Log("Thread Close");
            QNY_Thread = null;
            QNYDoing = null;

        };

        qfile.UploadProgressChanged += (sender, e) => {
            int percentage = (int)(100 * e.BytesSent / e.TotalBytes);
            Debug.Log("loading>>" + percentage);
        };
        qfile.UploadBlockCompleted += (sender, e) => {
//					puttedCtx.Add(e.Index,e.Ctx);
//					puttedCtx.Save();
        };
        qfile.UploadBlockFailed += (sender, e) => {
            //

        };


        qfile.Upload ();
        
    }
    
    


    public void DleteQNYFile(string FileUrl)
    {
        string LocalQNY_url = QNY_url + "/" + System.IO.Path.GetFileName(FileUrl);

        try {
            QiniuFile qfile_D = new QiniuFile (Qny_Bucket, LocalQNY_url);
            QiniuFileInfo finfo = qfile_D.Stat ();
            if (finfo != null) {
                qfile_D.Move("cloudcomment","movetest");
                //删除七牛云空间的文件
                //qfile.Delete ();
            }
        } catch (QiniuWebException e) {
            Console.WriteLine (e.Error.HttpCode);
            Console.WriteLine (e.Error.ToString ());
        }
    }
}
