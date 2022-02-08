using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEditor;
using UnityEngine;

public class CreateMapByJson : EditorWindow
{
    public GameObject Reference;
    public GameObject InsParentObj;
    private List<GameObject> ReferenceObjList;
    private string JsonPath;
    private JsonData jsonData;
    
    [MenuItem("DesignTools/Model/CreateMapByJson")]
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(CreateMapByJson));
    }

    void OnGUI()
    {

        if (GUI.Button(new Rect(14, 20, 120, 30), "ChooseJsonFile"))
        {
            //FileUrl = EditorUtility.OpenFilePanel("Choose Excel File", Application.dataPath, "xlsx");
            SetMap(EditorUtility.OpenFilePanel("Choose JsonFile", Application.dataPath, "json"));
        }
        EditorGUILayout.Space(50);
        EditorGUILayout.LabelField("JsonFileUrl >> " + JsonPath);
    }

    private void SetMap(string jsonFileUrl)
    {
        JsonPath = jsonFileUrl;
        //JsonPath = JsonUrl;
        
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

                
                Debug.Log("ReverseJsonData>>  " + json);
                CreateItemMap();
            }
        }
    }


    private void CreateItemMap()
    {
        //setParents(InsParentObj)
    }


}
