using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NetTools : EditorWindow
{
    private bool groupEnabled_1;
    private bool groupEnabled_2;
    private bool groupEnabled_3;
    private string Name = String.Empty;
    private string IP =  String.Empty;
    private List<string> IPList;
    private int IpIndex;
    private string ChatData;
    [MenuItem("DesignTools/Data/NetTools")]
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(NetTools));
        
    }

    private void OnEnable()
    {
        IpIndex = 0;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 40, 100, 30), "NextIP"))
        {
            IPList = PPTool.GetIns().ReturnLocalIp();
            IpIndex++;
            if (IpIndex > IPList.Count - 1)
            {
                IpIndex = 0;
            }
            IP = IPList[IpIndex];
                
        }
        EditorGUILayout.LabelField("IP >>> "+IP,EditorStyles.whiteLargeLabel); 
        Name = EditorGUILayout.TextField("Name", Name);
        EditorGUILayout.Space(30);
        groupEnabled_1 = EditorGUILayout.BeginToggleGroup("NetInit", groupEnabled_1);
        if (groupEnabled_1)
        {
            groupEnabled_2 = false;
            if (Name == String.Empty||IP ==  String.Empty)
            {
                groupEnabled_1 = false;
            }
            if (GUI.Button(new Rect(20, 90, 100, 30), "OpenNet"))
            {
            
                PPTool.GetIns().Begin(IP,Name);
            }
            if (GUI.Button(new Rect(150, 90, 100, 30), "CloseNet"))
            {
            
                PPTool.GetIns().UDPClose();
            }
            
            
            
            EditorGUILayout.Space(30);
        }
        
        EditorGUILayout.EndToggleGroup();
        groupEnabled_2 = EditorGUILayout.BeginToggleGroup("Chat", groupEnabled_2);
        if (groupEnabled_2)
        {
            groupEnabled_1 = false;
            if (Name == String.Empty||IP ==  String.Empty)
            {
                groupEnabled_2 = false;
            }

            if (ChatData != String.Empty)
            {
                if (GUI.Button(new Rect(20, 120, 100, 30), "Send"))
                {
            
                    PPTool.GetIns().ChatSedData(ChatData);
                }
            }

            
            
            EditorGUILayout.Space(30);
            ChatData = EditorGUILayout.TextField("Data", ChatData);

            
        }
        EditorGUILayout.EndToggleGroup();
        groupEnabled_3 = EditorGUILayout.BeginToggleGroup("SendFile", groupEnabled_3);

        if (groupEnabled_3)
        {
            if (GUI.Button(new Rect(20, 140, 100, 30), "Send"))
            {
            
                FileLink.GetIns().FileLinkInit(EditorUtility.OpenFilePanel("Choose File", Application.dataPath,""));
            }
        }

        EditorGUILayout.EndToggleGroup();
    }
}
