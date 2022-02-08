using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using LitJson;
using OBS;
using OBS.Model;
using qiniu;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataTransTools : EditorWindow
{
    private bool groupEnabled_1;
    private bool groupEnabled_2;
    private bool groupEnabled_3;
    private bool groupEnabled_4;
    private bool groupEnabled_5;
    //Editor
    public GameObject Reference;
    public List<GameObject> TargetGameObjectArray;
    public bool IsContainsClone;
    public bool IsOpenFolder;
    public bool IsContainDepict;
    [SerializeField]//必须要加
    protected List<GameObject> TargetObjectList = new List<GameObject>();
    //序列化对象
    protected SerializedObject _serializedObject;
    //序列化属性
    protected SerializedProperty _assetLstProperty;


    
    /////////////////////////////////////////////////PositionToJson
    private string JsonPath;
    private string JsonFilePath;
    private struct Data
    {
        [DataMember]
        public string Name;//Name
        [DataMember]
        public int X;//X
        [DataMember]
        public int Y; //Y
        [DataMember]
        public int Z; //Z
        [DataMember]
        public int IndexNum; //IndexNum
        [DataMember]
        public int RotationX;//X
        [DataMember]
        public int RotationY; //Y
        [DataMember]
        public int RotationZ; //Z
        [DataMember]
        public int ReturnNum; //Z
        [DataMember]
        public int ReturnIndexNum; //Z

    }
    private List<List<Data>> AllTarPointList;
    private List<string> ReferenceNameList;
    private List<object> DataList;
    ////////////////////////////////////////////
 
    ////////////////////////////////////////////ReverseJsonToScene
    private List<GameObject> ReferenceObjList;
    private JsonData jsonData;//MapData
    private List<Data> jsonReverseList;
    private GameObject ItemParentNode;
    protected void OnEnable()
    {
        
        //使用当前类初始化
        _serializedObject = new SerializedObject(this);
        //获取当前类中可序列话的属性
        _assetLstProperty = _serializedObject.FindProperty("TargetObjectList");
    }
    ////////////////////////////////////////////
    
    //////////////////////////////////////////////UploadToQNY
    private string Bucket;
    private string QNY_url;
    private Thread QNY_Thread;
    private ThreadStart QNYDoing;
    
    //////////////////////////////////////////////RefreshLoaclFile
    private string HttpUrl;

    //////////////////////////////////////////////UploadToHWCloud
    private string HW_Bucket;
    private string HW_url;
    private ThreadStart HwDoing;
    private Thread HW_Thread;

    [MenuItem("DesignTools/Data/DataTransTools")]
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(DataTransTools));
        
    }
    

    void OnGUI()
    {
        
        EditorGUILayout.Space(10);
        
        groupEnabled_1 = EditorGUILayout.BeginToggleGroup("PositionToJson Is Open（地图转表工具）", groupEnabled_1);
        /////////////////////////////////////////////////PositionToJson
        if (groupEnabled_1)
        {
            DataTrans.DataTransIns();
            groupEnabled_2 = false;
            groupEnabled_3 = false;
            groupEnabled_4 = false;
            groupEnabled_5 = false;

            if (GUI.Button(new Rect(20, 40, 160, 30), "PositionToJson"))
            {

                ObjToJson(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }

            EditorGUILayout.Space(50);
            Reference = (GameObject) EditorGUILayout.ObjectField("Reference（参考数据节点）", Reference, typeof(Object));
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("是否忽略Clone后缀，默认不勾选，特殊情况勾选",EditorStyles.whiteLargeLabel);
            IsContainsClone = EditorGUILayout.Toggle("IsContainsClone", IsContainsClone);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Is Open Folder of Json Export's Folder（转换结束时是否打开Json文件所在的文件夹）",EditorStyles.whiteLargeLabel);
            IsOpenFolder = EditorGUILayout.Toggle("IsOpenFolder", IsOpenFolder);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Is Cantain The Reference Group's Data（是否将参考排序数据导出到Json表）",EditorStyles.whiteLargeLabel);
            IsContainDepict = EditorGUILayout.Toggle("IsContainDepict", IsContainDepict);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("关卡数据");


            //更新
            _serializedObject.Update();
            //开始检查是否有修改
            EditorGUI.BeginChangeCheck();
            //显示属性
            //第二个参数必须为true，否则无法显示子节点即List内容
            EditorGUILayout.PropertyField(_assetLstProperty, true);
            TargetGameObjectArray = TargetObjectList;

            //结束检查是否有修改
            if (EditorGUI.EndChangeCheck())
            {
                //提交修改
                _serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.EndToggleGroup();
        /////////////////////////////////////////////////
        
        /////////////////////////////////////////////////ReverseJsonToScene
        groupEnabled_2 = EditorGUILayout.BeginToggleGroup("ReverseJsonToScene Is Open（表转地图工具）", groupEnabled_2);
        
        if (groupEnabled_2)
        {
            groupEnabled_1 = false;
            groupEnabled_3 = false;
            groupEnabled_4 = false;
            groupEnabled_5 = false;

            if (GUI.Button(new Rect(20, 60, 160, 30), "ReverseJsonToScene"))
            {
                ReverseJsonTpMap(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }

            EditorGUILayout.Space(50);
            EditorGUILayout.LabelField("参考数据节点",EditorStyles.whiteLargeLabel);
            Reference = (GameObject) EditorGUILayout.ObjectField("Reference", Reference, typeof(Object));
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("地图生成节点",EditorStyles.whiteLargeLabel);
            ItemParentNode = (GameObject) EditorGUILayout.ObjectField("ItemParentNode", ItemParentNode, typeof(Object));
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("The JsonData Is Include Reference's Data（Json数据中是否包含参考排序数据）",EditorStyles.whiteLargeLabel);
            IsContainDepict = EditorGUILayout.Toggle("IsIncludeReference", IsContainDepict);
        }
        EditorGUILayout.EndToggleGroup();
        /////////////////////////////////////////////////
        
        /////////////////////////////////////////////////UPLoadToQNY
        groupEnabled_3 = EditorGUILayout.BeginToggleGroup("UpLoadFileToHWY（上传Json文件至华为云）", groupEnabled_3);
        if (groupEnabled_3)
        {
            groupEnabled_2 = false;
            groupEnabled_1 = false;
            groupEnabled_4 = false;
            groupEnabled_5 = false;
            EditorGUILayout.Space(40);
            //Bucket = EditorGUILayout.TextField("FileUploadSpace", Bucket);
            HW_url = EditorGUILayout.TextField("FileUploadURL", HW_url);
            if (GUI.Button(new Rect(20, 80, 160, 30), "UpLoadFileToHWY"))
            {
                HWUpLoad(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }

            

            EditorGUILayout.Space(10);

        }
        EditorGUILayout.EndToggleGroup();
        ////////////////////////////////////////////////////

        
        ////////////////////////////////////////////////////
        groupEnabled_4 = EditorGUILayout.BeginToggleGroup("RefreshLocalJsonFile（刷新本地Json文件）", groupEnabled_4);
        if (groupEnabled_4)
        {
            groupEnabled_2 = false;
            groupEnabled_1 = false;
            groupEnabled_3 = false;
            groupEnabled_5 = false;

            EditorGUILayout.Space(40);
            EditorGUILayout.LabelField("HttpUrl,Format:xxxx/ProjectName/v1(文件地址，格式：xxxx/ProjectName/v1)",EditorStyles.whiteLargeLabel);
            HttpUrl = EditorGUILayout.TextField("FileUploadURL", HttpUrl);
            if (GUI.Button(new Rect(20, 100, 160, 30), "RefreshLocalJsonFile"))
            {
                RefreshLocalJson(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }

            

            EditorGUILayout.Space(10);

        }
        EditorGUILayout.EndToggleGroup();
        /////////////////////////////////////////////////UPLoadToQNY
        groupEnabled_5 = EditorGUILayout.BeginToggleGroup("UpLoadFileToQNY（上传Json文件至七牛云）", groupEnabled_5);
        if (groupEnabled_5)
        {
            groupEnabled_2 = false;
            groupEnabled_1 = false;
            groupEnabled_4 = false;
            groupEnabled_3 = false;

            EditorGUILayout.Space(40);
            //Bucket = EditorGUILayout.TextField("FileUploadSpace", Bucket);
            QNY_url = EditorGUILayout.TextField("FileUploadURL", QNY_url);
            if (GUI.Button(new Rect(20, 120, 160, 30), "UpLoadFileToQNY"))
            {
                QNyUpLoad(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }

            

            EditorGUILayout.Space(10);

        }
        EditorGUILayout.EndToggleGroup();
        ////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////PositionToJson
     private void ObjToJson(string jsonFileUrl)
     {
        if (jsonFileUrl == String.Empty)
        {
            Debug.Log("Export interrupt");
            return;
        }
        DataList = new List<object>();
        AllTarPointList = new List<List<Data>>();
        ///ListName
        ReferenceNameList = new List<string>();
        foreach (Transform child in Reference.transform)
        {
            ReferenceNameList.Add(child.name);
            
        }

        JsonPath = jsonFileUrl;//JsonFile +"\\"+ JsonName;
        Debug.Log(JsonPath);
        //FileSetting
        if (!File.Exists(JsonPath))
        {
            File.Create(JsonPath);
        }
        ///ArraySetting
        for (int i = 0; i < TargetGameObjectArray.Count; i++)
        {
            List<Data> TarPointList = new List<Data>();
            foreach (Transform child in TargetGameObjectArray[i].transform)
            {
                Data TargetData;
                if (child.name.IndexOf("(") == -1||IsContainsClone)
                {
                    TargetData.Name = child.name;
                    TargetData.IndexNum = this.SetIndexData(child.name);
                }
                else
                {
                    TargetData.Name = child.name.Substring(0, child.name.IndexOf(" "));
                    TargetData.IndexNum = this.SetIndexData(child.name.Substring(0, child.name.IndexOf(" ")));

                }


                TargetData.X = Convert.ToInt32(child.transform.position.x * 100);
                TargetData.Y = Convert.ToInt32(child.transform.position.y * 100);
                TargetData.Z = Convert.ToInt32(child.transform.position.z * 100);
                TargetData.RotationX = Convert.ToInt32(DataTrans.DataTransIns().AngleNormalizeTrans(child.transform).x * 100);
                TargetData.RotationY = Convert.ToInt32(DataTrans.DataTransIns().AngleNormalizeTrans(child.transform).y * 100);
                TargetData.RotationZ = Convert.ToInt32(DataTrans.DataTransIns().AngleNormalizeTrans(child.transform).z * 100);
                TargetData.ReturnNum = child.GetComponent<ReturnImageNum>() != null ? child.GetComponent<ReturnImageNum>().ReturnNum : 0;
                TargetData.ReturnIndexNum = child.GetComponent<ReturnItemIndex>() != null ? child.GetComponent<ReturnItemIndex>().ReturnNum : 0;


                TarPointList.Add(TargetData);
            }
            AllTarPointList.Add(TarPointList);
        }
        DataList.Add(ReferenceNameList);
        DataList.Add(AllTarPointList);
        SaveToFile();
    }
     
     
    private int SetIndexData(string ChildName)
    {
        for (int j = 0; j < ReferenceNameList.Count; j++)
        {
            if (String.CompareOrdinal(ChildName, ReferenceNameList[j]) == 0)
            {
                return j;
            }
        }
        Debug.LogError(" May be name is error");
        return -1;
    }
    
    public void SaveToFile()
    {
        string jsonDataString;
        if (IsContainDepict)
        {
            jsonDataString  = JsonMapper.ToJson(DataList);
        }
        else
        {
            jsonDataString  = JsonMapper.ToJson(AllTarPointList);

        }
        Debug.Log("保存成功");
        if (IsOpenFolder)
        {
            DataTrans.DataTransIns().JsonDataSaveToFile(jsonDataString,JsonPath,true);
        }
        else
        {
            DataTrans.DataTransIns().JsonDataSaveToFile(jsonDataString,JsonPath);

        }

    }
    //////////////////////////////////////////////////////    
    
    //////////////////////////////////////////ReverseJsonToScene
    
    public void ReverseJsonTpMap(string File_Url)
    {
        JsonPath = File_Url;
        //JsonPath = JsonUrl;
        jsonReverseList = new List<Data>();
        ReferenceObjList = new List<GameObject>();
        SetReference();
        UpdateRankJson();
        
    }


    private void SetReference()
    {
        foreach (Transform child in Reference.transform)
        {
            ReferenceObjList.Add(child.gameObject);
            
        }
        
    }

    private void UpdateRankJson()
    {
        if (!File.Exists(JsonPath))
        {
                
            Debug.Log("读取的文件不存在！");
            File.Create(JsonPath);
        }
        else
        {
            string json = File.ReadAllText(JsonPath);
            if (json != "")
            {
                jsonData = JsonMapper.ToObject(json);
                if (IsContainDepict)
                {
                    jsonData = jsonData[1];
                    
                }

                Debug.Log("ReverseJsonData>>  "+json);
                for (int i = 0; i < jsonData.Count; i++)
                {
                    GameObject obj = new GameObject();
                    obj.name = "Ins" + (i + 1);
                    obj.transform.SetParent(ItemParentNode.transform);
                    for (int n = 0; n < jsonData[i].Count; n++)
                    {
                        Data ItemData = new Data();
                        ItemData.Name = (string) jsonData[i][n]["Name"];
                        ItemData.X = (int) jsonData[i][n]["X"];
                        ItemData.Y = (int) jsonData[i][n]["Y"];
                        ItemData.Z = (int) jsonData[i][n]["Z"];
                        ItemData.RotationX = (int) jsonData[i][n]["RotationX"];
                        ItemData.RotationY = (int) jsonData[i][n]["RotationY"];
                        ItemData.RotationZ = (int) jsonData[i][n]["RotationZ"];
                        ItemData.IndexNum = (int) jsonData[i][n]["IndexNum"];
                        jsonReverseList.Add(ItemData);
                        GameObject item = GameObject.Instantiate(ReferenceObjList[ItemData.IndexNum]);
                        item.name = ItemData.Name;
                        item.transform.position = new Vector3((float)ItemData.X/100,(float)ItemData.Y/100,(float)ItemData.Z/100);
                        item.transform.eulerAngles = new Vector3((float)ItemData.RotationX/100,(float)ItemData.RotationY/100,(float)ItemData.RotationZ/100);
                        item.transform.SetParent(obj.transform);

                    }    

                    
                }
                Debug.Log("ReverseJsonToScene Success");
            }
        }
    }
    //////////////////////////////////////////


    private void QNyUpLoad(string fileUrl)
    {
        QNYCloud.GetIns().QNYThreadInit("","","","");
        QNYCloud.GetIns().AddFileUrlToUpLoad(fileUrl);
    }



    //////////////////////////////////////////////////////////////////////////////


    private void RefreshLocalJson(string fileUrl)
    {
        string Get_Url = ""+HttpUrl+"/"+System.IO.Path.GetFileName(fileUrl);
        DataTrans.DataTransIns().GetInternetJson(Get_Url, fileUrl);
    }
    
        
    
    //////////////////////////////////////////////////////////////////////////////
    
    
    private void HWUpLoad(string fileUrl)
    {
        HWCloudIns.GetIns().HwThreadInit(HW_url,"","","","") ;
        HWCloudIns.GetIns().AddFileUrlToUpLoad(fileUrl);
    }
    
    
}
