using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;


public class ExcelDataTrans
{
    ///////////////////////////ExcelToJsonTool
    private DataSet mResultSet;
    /// //////////////////////////////

    ///////////////////////////JsonToExcelTool
    private string JsonFileUrl;
    private DataSet JsonFileDataSet;
    private ArrayList jsonArrayList;

    /// //////////////////////////////
    private ExcelDataTrans() { }
    private static ExcelDataTrans _ExcelDataTrans = null;
    public static ExcelDataTrans DataTransIns()
    {
        if (_ExcelDataTrans == null)
        {

            _ExcelDataTrans = new ExcelDataTrans();
        }
        return _ExcelDataTrans;
    }
    
    public void ExcelUtility (string fileUrl)
    {
        
        if (fileUrl == null)
        {
            return;
        }

        FileStream mStream = File.Open (fileUrl, FileMode.Open, FileAccess.Read);
        IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader (mStream);
        mResultSet = mExcelReader.AsDataSet();

    }
    
    public void ConvertToJson (string JsonPath,int sheetIndex, Encoding encoding)
    {

        if (JsonPath == null)
        {
            Debug.Log("Your Not Choose File");
            return;
        }
        //判断Excel文件中是否存在数据表
        if (mResultSet.Tables.Count < 1)
            return;
        
        DataTable mSheet = mResultSet.Tables [sheetIndex -1];
        if (mSheet == null)
        {
            Debug.Log("is not sheet ");
            return;
        }

        //判断数据表内是否存在数据
        if (mSheet.Rows.Count < 1)
            return;

        //读取数据表行数和列数
        int rowCount = mSheet.Rows.Count;
        int colCount = mSheet.Columns.Count;
        //准备一个列表存储整个表的数据
        List<Dictionary<string, object>> table = new List<Dictionary<string, object>> ();

        //读取数据
        for (int i = 1; i < rowCount; i++) {
            if (mSheet.Rows[i][0].ToString() == "")
            {
                continue;
            }
            //准备一个字典存储每一行的数据
            Dictionary<string, object> row = new Dictionary<string, object> ();
            for (int j = 0; j < colCount; j++) {
                //读取第1行数据作为表头字段
                string field = mSheet.Rows [0] [j].ToString ();
                if (field != "")
                {
                    //Key-Value对应
                    row[field] = mSheet.Rows[i][j];
                }
            }

            

            //添加到表数据中
            table.Add (row);
        }
        
        //生成Json字符串
        string json = JsonConvert.SerializeObject (table, Newtonsoft.Json.Formatting.Indented);
        DataTrans.DataTransIns().JsonWriteToFile(json, JsonPath);
        
        
        /*Debug.Log("Json Data >>"+json);
        //写入文件
        using (FileStream fileStream=new FileStream(JsonPath,FileMode.Create,FileAccess.Write)) {
            using (TextWriter textWriter = new StreamWriter(fileStream, encoding)) {
                textWriter.Write (json);
            }
        }*/
    }

    

    ///////////////////////////////////////////////////////////////////
    
     
    ///////////////////////////////////////////////////////////////////
    public List<List<string>> JsonToDataSet(string JsonUrl)
    {

        StreamReader sr = new StreamReader(JsonUrl, Encoding.UTF8);
        string linee = sr.ReadToEnd();
        DataSet ds = new DataSet();
    
        object obj = JsonConvert.DeserializeObject(linee);


        return ConvertToDesiredType(obj);
        
    }
    
    private  List<List<string>> ConvertToDesiredType(object list)
    {
        List<List<string>> DataList = new  List<List<string>>();
        int Index = 0;
        foreach (var item in (IEnumerable<object>)list)
        {
            if (Index == 0)
            {
                List<string> KeyList = new List<string>();
                foreach (JProperty datas in (IEnumerable<object>)item)  
                {  
                    KeyList.Add(datas.Name);
                }

                DataList.Add(KeyList);
            }

            Index++;
            List<string> ItemList = new List<string>();
            foreach (JProperty datas in (IEnumerable<object>)item)  
            {
                ItemList.Add((string)datas.Value);
            }
            DataList.Add(ItemList);

        }
        Debug.Log(" DateLine>> "+DataList.Count);

        
        return DataList;
    }

    public void ArrayWriteToExcel(List<List<string>> JsonOutData,string fileUrl,int sheetIndex)
    {
        FileInfo _excelName = new FileInfo(fileUrl);
        ExcelPackage package = new ExcelPackage(_excelName);

        ExcelWorksheet worksheet = package.Workbook.Worksheets["CloudData"];
        if (worksheet == null)
        {
            worksheet = package.Workbook.Worksheets.Add("CloudData");
        }

        //在 excel 空文件添加新 sheet，并设置名称。
        //ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CloudData");
        for (int i = 0; i < JsonOutData.Count; i++)
        {
            for (int j = 0; j < ((List<string>)JsonOutData[i]).Count; j++)
            {
                WriteToExcel(j, i, ((List<string>)JsonOutData[i])[j], worksheet);
                
            }

        }
        
        //保存excel
        package.Save();
        Debug.Log("FileSaveSuccess");
    }


    private static void WriteToExcel(int RowNum,int LineNum,string Data,ExcelWorksheet worksheet)
    {
        //添加列名
        
        worksheet.Cells[LineNum+1, RowNum+1].Value = Data;
        
    }
}
