using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Text;
using System;

public class csv_File
{
    //csvファイルを書き込むためのクラス

    string FilePath; //保存先パス

    StreamWriter sw; //ファイル
    Encoding enc; //文字コード

    //コンストラクタ
    public csv_File(string FilePath)
    {
        this.FilePath = FilePath;
        this.enc = Encoding.GetEncoding("Shift_JIS");

        //ファイルを開く
        this.sw = new StreamWriter(FilePath, true, enc);
    }

    //ファイルにDataを追記
    public void WriteData(string Data)
    {
        sw.WriteLine(Data);
    }

    //ファイルを閉じる(忘れるとデータが保存されない)
    public void Close_File()
    {
        sw.Close();
    }
}
