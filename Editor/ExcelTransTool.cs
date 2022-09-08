using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExcelTransTool : EditorWindow
{
    
    private bool groupEnabled_1;
    private bool groupEnabled_2;
    private List<List<string>> jsonArrayList = new List<List<string>>();
    [MenuItem("DesignTools/Data/ExcelTransTool")]
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(ExcelTransTool));
    }

    void OnGUI()
    {
        ///////////////////////////////////////////////////////////////////ExcelToJsonTool
        groupEnabled_1 = EditorGUILayout.BeginToggleGroup("ExcelToJson（Excel转Json工具）", groupEnabled_1);
        if (groupEnabled_1)
        {
            groupEnabled_2 = false;
            if (GUI.Button(new Rect(14, 20, 120, 30), "ChooseExcelFile"))
            {
                ExcelDataTrans.DataTransIns().ExcelUtility(EditorUtility.OpenFilePanel("Choose Excel File", Application.dataPath, "xlsx"));
            }

            if (GUI.Button(new Rect(150, 20, 120, 30), "ExcelFileToJson"))
            {
                ExcelDataTrans.DataTransIns().ConvertToJson(EditorUtility.OpenFilePanel("Choose Target Json's File", Application.dataPath, "json"),
                    1,Encoding.UTF8);
                
            }

            EditorGUILayout.Space(50);
            EditorGUILayout.LabelField("Please Check Your FileURL Is Right (请确认以下Excel文件地址是正确的)",
                EditorStyles.whiteLargeLabel);
            EditorGUILayout.LabelField("ExcelFileUrl >> " , EditorStyles.whiteLabel);
        }

        EditorGUILayout.EndToggleGroup();
        ///////////////////////////////////////////////////////////////////
        
        
        ///////////////////////////////////////////////////////////////////JsonToExcelTool
        groupEnabled_2 = EditorGUILayout.BeginToggleGroup("JsonToExcel（Json转Excel工具）", groupEnabled_2);
        if (groupEnabled_2)
        {
            groupEnabled_1 = false;
            if (GUI.Button(new Rect(14, 50, 120, 30), "ChooseJsonFile"))
            {
                jsonArrayList = ExcelDataTrans.DataTransIns().JsonToDataSet(EditorUtility.OpenFilePanel("Choose Json File", Application.dataPath, "json"));
            }

            if (GUI.Button(new Rect(150, 50, 120, 30), "JsonToExcel"))
            {
                ExcelDataTrans.DataTransIns().ArrayWriteToExcel(jsonArrayList,EditorUtility.OpenFilePanel("Choose Target Excel File", Application.dataPath, "xlsx"),0);
            }
            EditorGUILayout.Space(50);
            EditorGUILayout.LabelField("Please Check Your FileURL Is Right (请确认以下Json文件地址是正确的)",
                EditorStyles.whiteLargeLabel);
            EditorGUILayout.LabelField("JsonFileUrl >> " , EditorStyles.whiteLabel);
        }

        EditorGUILayout.EndToggleGroup();
        ///////////////////////////////////////////////////////////////////
    }
}
