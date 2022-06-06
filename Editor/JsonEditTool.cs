using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class JsonEditTool : EditorWindow
{
    private List<List<string>> jsonList = new List<List<string>>();
    private List<string[]> tableJsonList;

    private string DefaultTableRaw = "";
    private string DefaultTableLine = "";

    private bool BeginRefresh = false;

    /// <summary>
    /// AutoSave
    /// </summary>
    private bool IsAutoSave = false;

    private string AutoSaveFileUrl = "";
    private int AutoSaveCheckNum = 0;

    /// <summary>
    /// ShowPattern
    /// </summary>
    private int patternIndex = 0;

    private int EnumSelectRaw;
    private int EnumSelectLine;



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
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (EditorGUILayout.DropdownButton(new GUIContent("File"), FocusType.Keyboard, GUILayout.Width(50)))
        {
            string[] alls = new string[5]
            {
                "ReadJsonFile", "CreateNewJsonFile", "SaveJsonFile/SaveFile", "SaveJsonFile/AutoSaveJsonFile",
                "PatternChange"
            };
            GenericMenu menu = new GenericMenu();
            int HandleIndex = 0;
            foreach (var item in alls)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                switch (HandleIndex)
                {
                    case 2:
                        if (tableJsonList != null)
                        {
                            menu.AddItem(new GUIContent(item), false, FileEditor, HandleIndex);
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(item + " >> not data"));
                        }

                        break;
                    case 3:
                        if (tableJsonList != null)
                        {
                            menu.AddItem(new GUIContent(item), false, FileEditor, HandleIndex);
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(item + " >> not data"));
                        }

                        break;
                    case 4:
                        if (tableJsonList != null)
                        {
                            menu.AddItem(new GUIContent(item), IsAutoSave, FileEditor, HandleIndex);
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(item + " >> not data"));
                        }

                        break;
                    default:
                        menu.AddItem(new GUIContent(item), false, FileEditor, HandleIndex);
                        break;
                }

                //添加菜单
                HandleIndex++;
            }

            menu.ShowAsContext();
        }

        if (EditorGUILayout.DropdownButton(new GUIContent("Edit"), FocusType.Keyboard, GUILayout.Width(50)))
        {
            string[] alls = new string[2] {"AddLineOrAddRaw", "CreateTable"};
            GenericMenu menu = new GenericMenu();
            int HandleIndex = 0;
            foreach (var item in alls)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                switch (HandleIndex)
                {
                    case 0:
                        if (tableJsonList != null)
                        {
                            menu.AddItem(new GUIContent(item), false, EditEditor, HandleIndex);
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(item + " >> not data"));
                        }

                        break;
                    default:
                        menu.AddItem(new GUIContent(item), false, EditEditor, HandleIndex);
                        break;
                }
                //添加菜单

                HandleIndex++;
            }

            menu.ShowAsContext();
        }

        EditorGUILayout.EndHorizontal();
        if (IsAutoSave)
        {
            EditorGUI.BeginChangeCheck();
            if (AutoSaveFileUrl != "")
            {
                AutoSaveCheckNum++;
                if (AutoSaveCheckNum > 1000)
                {
                    AutoSaveCheckNum = 0;
                    SaveDataAndWriteToJson(AutoSaveFileUrl);

                }

            }

            EditorGUI.EndChangeCheck();
        }

        if (BeginRefresh)
        {
            switch (patternIndex)
            {
                case 0:
                    UpdateTextShow();
                    break;
                case 1:
                    UpdateEnumButtonShow();
                    break;
            }

        }

    }

    //FileEditor
    private void FileEditor(object index)
    {
        switch (Convert.ToInt32(index))
        {
            case 0:
                jsonList = ExcelDataTrans.DataTransIns()
                    .JsonToDataSet(EditorUtility.OpenFilePanel("Choose Json File", Application.dataPath, "json"));
                ReadFileData(jsonList);
                BeginRefresh = true;
                break;
            case 1:
                Tips.GTI().ShowIndex = 5;
                Tips.GTI().CB = () => { CreateJsonFile(Tips.GTI().FileUrl); };
                Tips.GTI().Show();

                break;
            case 2:
                SaveDataAndWriteToJson(EditorUtility.OpenFilePanel("Choose Json File", Application.dataPath, "json"));
                break;
            case 3:
                IsAutoSave = !IsAutoSave;
                if (IsAutoSave)
                {
                    AutoSaveFileUrl = EditorUtility.OpenFilePanel("Choose Json File", Application.dataPath, "json");
                }

                break;
            case 4:
                patternIndex++;
                if (patternIndex > 1)
                {
                    patternIndex = 0;
                }

                break;

        }
    }

    private void EditEditor(object index)
    {
        switch (Convert.ToInt32(index))
        {
            case 0:
                Tips.GTI().ShowIndex = 3;
                Tips.GTI().CB = () =>
                {
                    if (Tips.GTI().RawNum > 0)
                    {
                        AddRaw();
                    }

                    if (Tips.GTI().LineNum > 0)
                    {
                        AddLine();
                    }

                };
                Tips.GTI().Show();
                break;
            case 1:
                Tips.GTI().ShowIndex = 4;
                Tips.GTI().CB = () =>
                {
                    if (Tips.GTI().RawNum > 0 && Tips.GTI().LineNum > 0)
                    {
                        CreateDefaultTable(Convert.ToInt32(Tips.GTI().RawNum), Convert.ToInt32(Tips.GTI().LineNum));
                    }


                };
                Tips.GTI().Show();
                break;
            case 2:
                break;

        }
    }

    /// <summary>
    /// table refresh
    /// </summary>
    private void UpdateTextShow()
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
            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();

    }

    private void UpdateEnumButtonShow()
    {
        EditorGUILayout.BeginVertical();

        for (int i = 0; i < tableJsonList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < tableJsonList[0].Length; j++)
            {
                if (tableJsonList[i][j] != null)
                {
                    ContentCreate(tableJsonList[i][j], i, j);
                }
                else
                {
                    tableJsonList[i][j] = EditorGUILayout.TextField("");
                }

            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();
    }

    private void ContentCreate(string num, int line, int raw)
    {
        if (EditorGUILayout.DropdownButton(new GUIContent(num), FocusType.Keyboard, GUILayout.Width(100)))
        {
            EnumSelectRaw = raw;
            EnumSelectLine = line;
            string[] alls = new string[4] {"AddInsert", "CopyInsert","AddTableRaw","AddTableLine"};
            GenericMenu menu = new GenericMenu();
            int HandleIndex = 0;
            foreach (var item in alls)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                //添加菜单
                menu.AddItem(new GUIContent(item), false, OnValueSelected, HandleIndex);
                HandleIndex++;
            }

            menu.ShowAsContext();
        }
    }



    private void OnValueSelected(object HandleIndex)
    {
        switch ((int) HandleIndex)
        {
            case 0:
                Tips.GTI().ShowIndex = 1;
                Tips.GTI().CB = () =>
                {
                    int tempNum = 0;
                    for (int i = 1; i < tableJsonList.Count - EnumSelectLine; i++)
                    {
                        if (Tips.GTI().EnumAddNum > tempNum)
                        {
                            tempNum++;
                            tableJsonList[EnumSelectLine + i][EnumSelectRaw] =
                                (Convert.ToInt32(tableJsonList[EnumSelectLine + i - 1][EnumSelectRaw]) + 1).ToString();
                        }

                    }
                };
                Tips.GTI().Show();
                break;
            case 1:
                Tips.GTI().ShowIndex = 2;
                Tips.GTI().CB = () =>
                {
                    int tempNum = 0;
                    for (int i = 1; i < tableJsonList.Count - EnumSelectLine; i++)
                    {
                        if (Tips.GTI().EnumAddNum > tempNum)
                        {
                            tempNum++;
                            tableJsonList[EnumSelectLine + i][EnumSelectRaw] =
                                tableJsonList[EnumSelectLine + i - 1][EnumSelectRaw];
                        }

                    }
                };
                Tips.GTI().Show();
                break;
            case 2:
                ChooseAddRaw(EnumSelectRaw);
                break;
            case 3:
                ChooseAddLine(EnumSelectLine);
                break;
        }
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
                    tempStrArray[j] = jsonList[i][j];
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
        List<string[]> tempJL = new List<string[]>();
        for (int i = 0; i < tableJsonList.Count; i++)
        {
            string[] tempStrArray = new string[tableJsonList[0].Length + 1];
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

            if (i == tableJsonList.Count - 1)
            {
                tableJsonList = tempJL;
            }
        }
    }
    private void ChooseAddRaw(int RawIndex)
    {
        List<string[]> tempJL = new List<string[]>();
        for (int i = 0; i < tableJsonList.Count; i++)
        {
            string[] tempStrArray = new string[tableJsonList[0].Length + 1];
            Debug.Log(tempStrArray.Length);
            for (int j = 0; j <= tableJsonList[0].Length; j++)
            {
                if (j < RawIndex)
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
                else if(j == RawIndex)
                {
                    tempStrArray[j] = "";
                }else
                {
                    if (tableJsonList[i][j-1] != null)
                    {
                        tempStrArray[j] = tableJsonList[i][j-1].ToString();
                    }
                    else
                    {
                        tempStrArray[j] = "";
                    }
                }
            }

            tempJL.Add(tempStrArray);

            if (i == tableJsonList.Count - 1)
            {
                tableJsonList = tempJL;
            }
        }
    }

    private void AddLine()
    {
        string[] td = new string[tableJsonList.Count];
        for (int i = 0; i < tableJsonList.Count - 1; i++)
        {
            td[i] = "";
        }

        tableJsonList.Add(td);
    }
    private void ChooseAddLine(int lineIndex)
    {
        string[] td = new string[tableJsonList.Count];
        for (int i = 0; i < tableJsonList.Count - 1; i++)
        {
            td[i] = "";
        }
        tableJsonList.Insert(lineIndex,td);
    }
    

    private void SaveDataAndWriteToJson(string JsonPath)
    {
        List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

        //读取数据
        for (int i = 1; i < tableJsonList.Count; i++)
        {
            if (tableJsonList[i][0].ToString() == "")
            {
                continue;
            }

            //准备一个字典存储每一行的数据
            Dictionary<string, object> row = new Dictionary<string, object>();
            for (int j = 0; j < tableJsonList[0].Length; j++)
            {
                //读取第1行数据作为表头字段
                string field = tableJsonList[0][j].ToString();
                if (field != "")
                {
                    //Key-Value对应
                    row[field] = tableJsonList[i][j];
                }

            }



            //添加到表数据中
            table.Add(row);
        }

        //生成Json字符串
        string json = JsonConvert.SerializeObject(table, Newtonsoft.Json.Formatting.Indented);
        DataTrans.DataTransIns().JsonWriteToFile(json, JsonPath);
    }

    ////////////CreateDefaultTable
    private void CreateDefaultTable(int raw, int line)
    {
        if (raw > 0 && line > 0)
        {
            List<string[]> tempJL = new List<string[]>();
            for (int i = 0; i < line; i++)
            {
                string[] tempStrArray = new string[raw];
                for (int j = 0; j < raw; j++)
                {
                    tempStrArray[j] = "";
                }

                tempJL.Add(tempStrArray);

                if (i == raw - 1)
                {
                    tableJsonList = tempJL;
                    BeginRefresh = true;
                }
            }
        }
    }

    /// <summary>
    /// CreateFile
    /// </summary>
    private void CreateJsonFile(string fileUrl)
    {
        FileStream fs = null;
        if (!System.IO.File.Exists(fileUrl))
        {
            //没有则创建这个文件
            fs = new FileStream(fileUrl, FileMode.Create, FileAccess.Write); //创建
            fs.Close();
        }
    }
}
