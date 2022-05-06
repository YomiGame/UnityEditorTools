using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class JsonEditTool : EditorWindow
{
    private List<List<string>> jsonList = new List<List<string>>();
    private List<string[]> tableJsonList;
    
    private bool BeginRefresh = false;
    [MenuItem("DesignTools/Data/JsonEditTool")]
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(JsonEditTool));
        
    }

    private void OnEnable()
    {
        BeginRefresh = false;
    }
    private void OnDisable()
    {
        BeginRefresh = false;
    }
    
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 20, 100, 30), "ReadJsonFile"))
        {
            jsonList = ExcelDataTrans.DataTransIns().JsonToDataSet(EditorUtility.OpenFilePanel("Choose Json File", Application.dataPath, "json"));
            ReadFileData(jsonList);
            BeginRefresh = true;
        }
        if (GUI.Button(new Rect(130, 20, 130, 30), "SaveToJsonFile"))
        {
            SaveDataAndWriteToJson(EditorUtility.OpenFilePanel("Choose Json File", Application.dataPath, "json"));
        }
        EditorGUILayout.Space(70);
        EditorGUILayout.BeginHorizontal();
        AddLine();
        AddRaw();
        EditorGUILayout.EndHorizontal();
        if (BeginRefresh)
        {
            UpdateShow();
        }

    }
    /// <summary>
    /// table refresh
    /// </summary>
    private void UpdateShow()
    {
        EditorGUILayout.BeginVertical();
        
        for (int i = 0; i < tableJsonList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < tableJsonList[0].Length; j++)
            {
                if (tableJsonList[i][j] != null)
                {
                    tableJsonList[i][j] = EditorGUILayout.TextField(tableJsonList[i][j].ToString());
                }
                else
                {
                    tableJsonList[i][j] = EditorGUILayout.TextField("");

                }
                Debug.Log(tableJsonList.Count);
            }
            EditorGUILayout.EndHorizontal();
            
        }
        EditorGUILayout.EndVertical();
        
    }
    /// <summary>
    /// ReadFile
    /// </summary>
    /// <param name="jsonList"></param>
    private void ReadFileData(List<List<string>> jsonList)
    {
        tableJsonList = new List<string[]>();
        
        for (int i = 0; i < jsonList.Count; i++)
        {
            string[] tempStrArray = new string[jsonList[0].Count];
            for (int j = 0; j < jsonList[0].Count; j++)
            {
                if (jsonList[i][j] != null)
                {
                    tempStrArray[j] = jsonList[i][j].ToString();
                }
                else
                {
                    tempStrArray[j] = "";
                }

            }
            tableJsonList.Add(tempStrArray);
            
        }
        
    }

    /// <summary>
    /// table save to json data
    /// </summary>
    private void SaveToJsonData()
    {
        jsonList = new List<List<string>>();
        for (int i = 0; i < tableJsonList.Count; i++)
        {
            List<string> strList = new List<string>();
            foreach (string data in tableJsonList[i])
            {
                strList.Add(data);
                
            }
            jsonList.Add(strList);
        }
        
    }

    private void AddRaw()
    {
        if (EditorGUILayout.DropdownButton(new GUIContent("AddRaw →→→→→→"), FocusType.Keyboard))
        {
            List<string[]> tempJL = new List<string[]>();
            for (int i = 0; i < tableJsonList.Count; i++)
            {
                string[] tempStrArray = new string[tableJsonList[0].Length+1];
                Debug.Log(tempStrArray.Length);
                for (int j = 0; j < tableJsonList[0].Length; j++)
                {
                    if (tableJsonList[i][j] != null)
                    {
                        tempStrArray[j] = tableJsonList[i][j].ToString();
                    }
                    else
                    {
                        tempStrArray[j] = "";
                    }

                }
                tempJL.Add(tempStrArray);

                if (i == tableJsonList.Count -1)
                {
                    tableJsonList = tempJL;
                }
            }
            
        }
    }
    
    private void AddLine()
    {
        if (EditorGUILayout.DropdownButton(new GUIContent("AddLine ↓↓↓↓↓↓"), FocusType.Keyboard))
        {
            string[] td = new string[tableJsonList.Count];
            for (int i = 0; i < tableJsonList.Count - 1; i++)
            {
                td[i] = "";
            }
            tableJsonList.Add(td);
        }
    }
    
    private void SaveDataAndWriteToJson(string JsonPath)
    {
        List<Dictionary<string, object>> table = new List<Dictionary<string, object>> ();

        //读取数据
        for (int i = 1; i < tableJsonList.Count; i++) {
            if (tableJsonList[i][0].ToString() == "")
            {
                continue;
            }
            //准备一个字典存储每一行的数据
            Dictionary<string, object> row = new Dictionary<string, object> ();
            for (int j = 0; j < tableJsonList[0].Length; j++) {
                //读取第1行数据作为表头字段
                string field = tableJsonList [0] [j].ToString ();
                if (field != "")
                {
                    //Key-Value对应
                    row [field] = tableJsonList [i] [j];
                }

            }

            

            //添加到表数据中
            table.Add (row);
        }
        
        //生成Json字符串
        string json = JsonConvert.SerializeObject (table, Newtonsoft.Json.Formatting.Indented);
        DataTrans.DataTransIns().JsonWriteToFile(json, JsonPath);
    }
}
