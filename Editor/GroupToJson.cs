using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using LitJson;
using UnityEditor;
using UnityEngine;

public class GroupToJson : EditorWindow
{

    private bool groupEnabled_1;
    private bool groupEnabled_2;
    private bool groupEnabled_3;
    private bool groupEnabled_4;
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
        public int RotationX;//X
        [DataMember]
        public int RotationY; //Y
        [DataMember]
        public int RotationZ; //Z
    }

    
    public GameObject Reference;
    public GameObject InsParentObj;
    private string JsonPath;
    private string JsonFilePath;
    private JsonData jsonData;
    private GameObject ItemParentNode;
    private List<GroupToJson.Data> TarPointList;
    private List<List<GroupToJson.Data>> AllTarPointList;


    private GameObject CreatePoint;
    private GameObject ReferenceObj;

    private int CreateLine;
    private int CreateColumn;
    private float CreateDistance;
    private int CreateDirestion;

    [MenuItem("DesignTools/Model/GroupToJson")]
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(GroupToJson));
    }

    void OnGUI()
    {
        groupEnabled_1 = EditorGUILayout.BeginToggleGroup("PositionToJson Is Open（模型转表工具）", groupEnabled_1);

        /////////////////////////////////////////////////PositionToJson
        if (groupEnabled_1)
        {
            groupEnabled_2 = false;
            groupEnabled_3 = false;
            groupEnabled_4 = false;

            
            if (GUI.Button(new Rect(14, 35, 120, 30), "ChooseJsonFile"))
            {
                //FileUrl = EditorUtility.OpenFilePanel("Choose Excel File", Application.dataPath, "xlsx");
                ObjToJson(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }


            EditorGUILayout.Space(50);
            EditorGUILayout.LabelField("ItemParent(物体父节点)", EditorStyles.whiteLargeLabel);
            ItemParentNode = (GameObject) EditorGUILayout.ObjectField("ItemParentNode", ItemParentNode, typeof(GameObject));

            EditorGUILayout.LabelField("JsonFileUrl >> " + JsonPath);
        }

        EditorGUILayout.EndToggleGroup();
        
        /////////////////////////////////////////////////ReverseJsonToScene
        groupEnabled_2 = EditorGUILayout.BeginToggleGroup("ReverseJsonToScene Is Open（表转模型工具）", groupEnabled_2);
        
        if (groupEnabled_2)
        {
            groupEnabled_1 = false;
            groupEnabled_3 = false;
            groupEnabled_4 = false;

            if (GUI.Button(new Rect(20, 60, 160, 30), "ReverseJsonToScene"))
            {
                ReverseJsonTpMap(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }

            EditorGUILayout.Space(50);
            EditorGUILayout.LabelField("参考物体",EditorStyles.whiteLargeLabel);
            ReferenceObj = (GameObject) EditorGUILayout.ObjectField("Reference", ReferenceObj, typeof(GameObject));
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("生成节点",EditorStyles.whiteLargeLabel);
            ItemParentNode = (GameObject) EditorGUILayout.ObjectField("ItemParentNode", ItemParentNode, typeof(GameObject));
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndToggleGroup();
        /////////////////////////////////////////////////
        /// /////////////////////////////////////////////////ReverseJsonToScene
        groupEnabled_3 = EditorGUILayout.BeginToggleGroup("ReverseJsonToScene Is Open（全部物体转json组工具）", groupEnabled_3);
        
        if (groupEnabled_3)
        {
            groupEnabled_1 = false;
            groupEnabled_2 = false;
            groupEnabled_4 = false;

            if (GUI.Button(new Rect(14, 70, 120, 30), "ChooseJsonFile"))
            {
                //FileUrl = EditorUtility.OpenFilePanel("Choose Excel File", Application.dataPath, "xlsx");
                AllObjToJson(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }


            EditorGUILayout.Space(50);
            EditorGUILayout.LabelField("AllItemParent(所有物体父节点)", EditorStyles.whiteLargeLabel);
            ItemParentNode = (GameObject) EditorGUILayout.ObjectField("ItemParentNode", ItemParentNode, typeof(GameObject));

            EditorGUILayout.LabelField("AllItemJsonFileUrl >> " + JsonPath);
        }
        EditorGUILayout.EndToggleGroup();
        /////////////////////////////////////////////////
        /// /// /////////////////////////////////////////////////CreateGroup
        groupEnabled_4 = EditorGUILayout.BeginToggleGroup("CreateGroupModel（创建一组标准模型）", groupEnabled_4);
        
        if (groupEnabled_4)
        {
            groupEnabled_1 = false;
            groupEnabled_2 = false;
            groupEnabled_3 = false;

            if (GUI.Button(new Rect(14, 90, 120, 30), "Create"))
            {
                CreateGroupModel();
                //FileUrl = EditorUtility.OpenFilePanel("Choose Excel File", Application.dataPath, "xlsx");
                //AllObjToJson(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
            }


            EditorGUILayout.Space(50);
            EditorGUILayout.LabelField("LineNum(行数)", EditorStyles.whiteLargeLabel);
            CreateLine = EditorGUILayout.IntField("CreateLine,X", CreateLine);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("ColumnNum(列数)", EditorStyles.whiteLargeLabel);
            CreateColumn = EditorGUILayout.IntField("CreateColumn,Y", CreateColumn);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("CreateDistance(间距)", EditorStyles.whiteLargeLabel);
            CreateDistance = EditorGUILayout.FloatField("CreateDistance", CreateDistance);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("CreateCreateDirestion(方向，正1为左下角，负1为右上角)", EditorStyles.whiteLargeLabel);
            CreateDirestion = EditorGUILayout.IntField("CreateCreateDirestion", CreateDirestion);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("参考物体",EditorStyles.whiteLargeLabel);
            ReferenceObj = (GameObject) EditorGUILayout.ObjectField("Reference", ReferenceObj, typeof(GameObject));
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("生成节点",EditorStyles.whiteLargeLabel);
            ItemParentNode = (GameObject) EditorGUILayout.ObjectField("ItemParentNode", ItemParentNode, typeof(GameObject));

        }
        EditorGUILayout.EndToggleGroup();
        /////////////////////////////////////////////////
    }

    private void ObjToJson(string jsonFileUrl)
     {
        if (jsonFileUrl == String.Empty)
        {
            Debug.Log("Export Fail(地址无效)");
            return;
        }
        ///ListName

        JsonPath = jsonFileUrl;//JsonFile +"\\"+ JsonName;
        Debug.Log(JsonPath);
        //FileSetting
        if (!File.Exists(JsonPath))
        {
            File.Create(JsonPath);
        }
        ///ArraySetting

             TarPointList = new List<GroupToJson.Data>();
             
            foreach (Transform child in ItemParentNode.transform)
            {

                GroupToJson.Data TargetData;
                if (child.name.IndexOf("(") == -1)
                {
                    TargetData.Name = child.name;
                }
                else
                {
                    TargetData.Name = child.name.Substring(0, child.name.IndexOf(" "));

                }


                TargetData.X = Convert.ToInt32(child.transform.position.x * 100);
                TargetData.Y = Convert.ToInt32(child.transform.position.y * 100);
                TargetData.Z = Convert.ToInt32(child.transform.position.z * 100);
                TargetData.RotationX = Convert.ToInt32(GetInpectorEulers(child.transform).x * 100);
                TargetData.RotationY = Convert.ToInt32(GetInpectorEulers(child.transform).y * 100);
                TargetData.RotationZ = Convert.ToInt32(GetInpectorEulers(child.transform).z * 100);



                TarPointList.Add(TargetData);
            }
            
        
        SaveToFile();
    }
     
     private Vector3 GetInpectorEulers(Transform mTransform)
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


    
    
    public void SaveToFile()
    {
        string jsonData;
        /*if (IsContainDepict)
        {
           jsonData  = JsonMapper.ToJson(DataList);
        }
        else
        {
            jsonData  = JsonMapper.ToJson(AllTarPointList);

        }*/
        jsonData  = JsonMapper.ToJson(TarPointList);

        Debug.Log(jsonData);
        File.WriteAllText(JsonPath, jsonData);
        Debug.Log("保存成功");
        /*if (IsOpenFolder)
        {
            JsonFilePath = JsonPath.Substring(0, JsonPath.LastIndexOf("/") + 1);
            JsonFilePath = JsonFilePath.Replace("/", "\\");
            System.Diagnostics.Process.Start("explorer", JsonFilePath);
        }*/

    }
    
    /////////////////////////////////////////////////
    ///
    //////////////////////////////////////////ReverseJsonToScene
    
    public void ReverseJsonTpMap(string File_Url)
    {
        JsonPath = File_Url;
        //JsonPath = JsonUrl;
        UpdateRankJson();
        
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
                
                Debug.Log("ReverseJsonData>>  "+json);
                    
                GameObject obj = new GameObject();
                obj.name = Path.GetFileNameWithoutExtension(JsonPath);
                obj.transform.SetParent(ItemParentNode.transform);
                    for (int n = 0; n < jsonData.Count; n++)
                    {
                        Data ItemData = new Data();
                        ItemData.Name = (string) jsonData[n]["Name"];
                        ItemData.X = (int) jsonData[n]["X"];
                        ItemData.Y = (int) jsonData[n]["Y"];
                        ItemData.Z = (int) jsonData[n]["Z"];
                        ItemData.RotationX = (int) jsonData[n]["RotationX"];
                        ItemData.RotationY = (int) jsonData[n]["RotationY"];
                        ItemData.RotationZ = (int) jsonData[n]["RotationZ"];
                        GameObject item = GameObject.Instantiate(ReferenceObj);
                        item.name = ItemData.Name;
                        item.transform.position = new Vector3((float)ItemData.X/100,(float)ItemData.Y/100,(float)ItemData.Z/100);
                        item.transform.eulerAngles = new Vector3((float)ItemData.RotationX/100,(float)ItemData.RotationY/100,(float)ItemData.RotationZ/100);
                        item.transform.SetParent(obj.transform);
                    }
                    Debug.Log("ReverseJsonToScene Success");
            }
        }
    }
    
    ////////////////////////////////////////////////////////
    ///
    private void AllObjToJson(string jsonFileUrl)
     {
        if (jsonFileUrl == String.Empty)
        {
            Debug.Log("Export Fail(地址无效)");
            return;
        }
        ///ListName

        JsonPath = jsonFileUrl;//JsonFile +"\\"+ JsonName;
        Debug.Log(JsonPath);
        //FileSetting
        if (!File.Exists(JsonPath))
        {
            File.Create(JsonPath);
        }
        ///ArraySetting

        AllTarPointList = new List<List<GroupToJson.Data>>();

            foreach (Transform child in ItemParentNode.transform)
            {
                Debug.Log(child.childCount);
                TarPointList = new List<Data>();

                foreach (Transform childs in child)
                {
                    GroupToJson.Data TargetData;
                    if (childs.name.IndexOf("(") == -1)
                    {
                        TargetData.Name = childs.name;
                    }
                    else
                    {
                        TargetData.Name = childs.name.Substring(0, childs.name.IndexOf(" "));

                    }
                    TargetData.X = Convert.ToInt32(childs.transform.position.x * 100);
                    TargetData.Y = Convert.ToInt32(childs.transform.position.y * 100);
                    TargetData.Z = Convert.ToInt32(childs.transform.position.z * 100);
                    TargetData.RotationX = Convert.ToInt32(GetInpectorEulers(childs.transform).x * 100);
                    TargetData.RotationY = Convert.ToInt32(GetInpectorEulers(childs.transform).y * 100);
                    TargetData.RotationZ = Convert.ToInt32(GetInpectorEulers(childs.transform).z * 100);
                    TarPointList.Add(TargetData);
                }
                AllTarPointList.Add(TarPointList);
            }
        
            SaveAllToFile();
    }
     
    public void SaveAllToFile()
    {
        string jsonData;
        /*if (IsContainDepict)
        {
           jsonData  = JsonMapper.ToJson(DataList);
        }
        else
        {
            jsonData  = JsonMapper.ToJson(AllTarPointList);

        }*/
        jsonData  = JsonMapper.ToJson(AllTarPointList);

        Debug.Log(jsonData);
        File.WriteAllText(JsonPath, jsonData);
        Debug.Log("保存成功");
        /*if (IsOpenFolder)
        {
            JsonFilePath = JsonPath.Substring(0, JsonPath.LastIndexOf("/") + 1);
            JsonFilePath = JsonFilePath.Replace("/", "\\");
            System.Diagnostics.Process.Start("explorer", JsonFilePath);
        }*/

    }

    private void CreateGroupModel()
    {
        ItemParentNode.transform.position = new Vector3(0,0,0);
        for (int i = 0;i<CreateLine;i++)
        {
            for (int j = 0; j < CreateColumn; j++)
            {
                GameObject item = GameObject.Instantiate(ReferenceObj, ItemParentNode.transform, true);
                item.name = String.Format("Box_{0}_{1}",i,j);
                item.transform.position = new Vector3((float)CreateDistance*i*CreateDirestion,0,(float)CreateDistance*j*CreateDirestion);
            }


        }
        Debug.Log(string.Format("<color=#ffff00ff>CreateModelLine{0},CreateModelColumn{1}</color>", CreateLine,CreateColumn));

    }





}
