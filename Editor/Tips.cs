using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tips : EditorWindow
{
    /// <summary>
    /// EnumADD >> 1;EnumCopy >> 1;
    /// </summary>
    public int ShowIndex = 0;

    /// <summary>
    /// EnumADDNum
    /// </summary>
    private bool isRaw = false;
    private bool isLine = false;

    public bool IsRaw => isRaw;
    public bool IsLine => isLine;
    
    private string enumAddNum;
    public int EnumAddNum => Convert.ToInt32(enumAddNum);
    private string rawNum;
    public int RawNum => Convert.ToInt32(rawNum);

    private string lineNum;
    public int LineNum => Convert.ToInt32(lineNum);
    /// <summary>
    /// CreateFile
    /// </summary>
    private string fileUrl = "";
    private string fileName = "";
    private string foldName = "";

    public string FileUrl => fileUrl;

    public delegate void CallBack();

    private CallBack _cb;

    public CallBack CB
    {
        set => _cb = value;
    }

    private Tips() { }
    private static Tips tips = null;
    public static Tips GTI()
    {
        if (tips == null)
        {

            tips = new Tips();
        }
        return tips;
    }
    
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(Tips));

    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    void OnGUI()
    {
        switch (ShowIndex)
        {
            case 1:
                enumAddNum = EditorGUILayout.TextField("AddNum", enumAddNum);
                if (EditorGUILayout.DropdownButton(new GUIContent("ADD"), FocusType.Keyboard))
                {
                    _cb.Invoke();
                    Tips.GTI().Close();
                }
                break;
            case 2:
                enumAddNum = EditorGUILayout.TextField("CopyNum", enumAddNum);
                if (EditorGUILayout.DropdownButton(new GUIContent("Copy"), FocusType.Keyboard))
                {
                    _cb.Invoke();
                    Tips.GTI().Close();
                }
                break;
            case 3:

                isRaw = EditorGUILayout.Toggle("RawAddNum", isRaw);
                isLine = EditorGUILayout.Toggle("LineAddNum", isLine);
                if (EditorGUILayout.DropdownButton(new GUIContent("RawLineAdd"), FocusType.Keyboard))
                {
                    _cb.Invoke();
                    //Tips.GTI().Close();
                }
                break;
            case 4:
                rawNum = EditorGUILayout.TextField("RawNum", rawNum);
                lineNum = EditorGUILayout.TextField("LineNum", lineNum);
                if (EditorGUILayout.DropdownButton(new GUIContent("CreateTable"), FocusType.Keyboard))
                {
                    _cb.Invoke();
                    Tips.GTI().Close();
                }
                break;
            case 5:
                fileName = EditorGUILayout.TextField("NewFileName",fileName);
                if (EditorGUILayout.DropdownButton(new GUIContent("ChooseFold"), FocusType.Keyboard))
                {
                    foldName = EditorUtility.OpenFolderPanel("Choose Fold", Application.dataPath, "");
                    fileUrl = foldName + "/" + fileName + ".json";
                }
                EditorGUILayout.LabelField("WillCraeteFile >> "+fileUrl);
                if (EditorGUILayout.DropdownButton(new GUIContent("CreateFile"), FocusType.Keyboard)&&_cb != null)
                {
                    _cb.Invoke();
                    Tips.GTI().Close();
                }
                break;
        }
    }
}
