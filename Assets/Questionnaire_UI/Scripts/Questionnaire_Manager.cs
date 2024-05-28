using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class Questionnaire_Manager : MonoBehaviour
{
    /// <summary>
    /// 質問紙のUIを管理するスクリプト
    /// 
    /// 以下の機能を実装
    /// ・質問内容をInspectorから自由に変更
    /// ・質問の順番のランダム化(選択可能)
    /// ・リッカートスケール(1-10件法から選択可能)
    /// ・アナログスケールに対応(0-1)
    /// 
    /// 任意のコントローラから入力したい場合、対応する関数を呼び出してください
    /// </summary>

    //質問順をランダムにするか
    [SerializeField, Tooltip("質問順をランダムにする")] bool Randomization;

    //質問データクラスの配列
    [SerializeField, Tooltip("質問項目データ")] public Question_Data[] Question_Datas;

    //回答データの記録先パス(回答終了時までに変更してください)
    [SerializeField, Tooltip("回答データの保存先パス")] public string Save_Path;


    //UIコンポーネント
    [Header("以下は基本的にそのままでOK")]
    [SerializeField] GameObject Question_Text_Obj;
    TMPro.TMP_Text Question_Text;

    [SerializeField] GameObject Description_Text_Obj;
    TMPro.TMP_Text Description_Text;

    [SerializeField] GameObject Likert_Scale_Obj;
    ToggleGroup Likert_Toggle_Group;
    List<GameObject> Likert_Scale_Toggle_List = new List<GameObject>();

    [SerializeField] GameObject Analog_Scale_Obj;
    Slider Analog_Scale;

    [SerializeField] GameObject Back_Button_Obj;
    TMPro.TMP_Text Back_Button_Text;
    [SerializeField] GameObject Next_Button_Obj;
    TMPro.TMP_Text Next_Button_Text;

    public int Current_Question = 1; //現在の質問番号
    public float Current_Answer = 1; //現在の回答値

    csv_File Csv_File;


    // Start is called before the first frame update
    void Start()
    {
        //すべての回答データをリセット、質問形式をチェック
        int temp_ID = 1;
        foreach (Question_Data temp_data in Question_Datas)
        {
            temp_data.Answer = 1;
            temp_data.Question_ID = temp_ID;
            temp_ID++;
            temp_data.Check_Question_Style();
        }

        Question_Text = Question_Text_Obj.GetComponent<TMP_Text>();
        Description_Text = Description_Text_Obj.GetComponent<TMP_Text>();
        Likert_Toggle_Group = Likert_Scale_Obj.GetComponent<ToggleGroup>();
        Analog_Scale = Analog_Scale_Obj.GetComponent<Slider>();
        Back_Button_Text = Back_Button_Obj.transform.Find("Text (TMP)").gameObject.GetComponent<TMPro.TMP_Text>();
        Next_Button_Text = Next_Button_Obj.transform.Find("Text (TMP)").gameObject.GetComponent<TMPro.TMP_Text>();

        //Toggleのリストを取得
        foreach(Transform temp in Likert_Scale_Obj.transform) Likert_Scale_Toggle_List.Add(temp.gameObject);

        //ランダム化がオンの場合、質問データ配列をシャッフル
        if (Randomization)
        {
            System.Random random = new System.Random();
            Question_Datas = Question_Datas.OrderBy(x => random.Next()).ToArray();
        }

        //Canvasを初期化
        Update_Canvas(Current_Question);
    }

    // Update is called once per frame
    void Update()
    {
        //矢印キー入力
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Move_Left();
        if (Input.GetKeyDown(KeyCode.RightArrow)) Move_Right();
        if (Input.GetKeyDown(KeyCode.UpArrow)) Next_Question();
        if (Input.GetKeyDown(KeyCode.DownArrow)) Prev_Question();
    }

    //回答選択「←」の処理
    public void Move_Left()
    {
        //Likertの場合
        if (Question_Datas[Current_Question - 1].Likert)
        {
            if (Current_Answer > 1) Current_Answer--; //回答値を1減らす
            Likert_Scale_Toggle_List[(int)Current_Answer - 1].GetComponent<Toggle>().isOn = true;
        }
        //AnalogScaleの場合
        if(Question_Datas[Current_Question - 1].VisualAnalogScale)
        {
            Analog_Scale.value -= Analog_Scale.maxValue / 30f;
        }
    }

    //回答選択「→」の処理
    public void Move_Right()
    {
        //Likertの場合
        if(Question_Datas[Current_Question - 1].Likert)
        {
            if (Current_Answer < Question_Datas[Current_Question - 1].Likert_Points) Current_Answer++; //回答値を1増やす
            Likert_Scale_Toggle_List[(int)Current_Answer - 1].GetComponent<Toggle>().isOn = true;
        }
        //AnalogScaleの場合
        if (Question_Datas[Current_Question - 1].VisualAnalogScale)
        {
            Analog_Scale.value += Analog_Scale.maxValue / 30f;
        }
    }


    //「戻る」の処理
    public void Prev_Question()
    {
        //1番目の質問の時
        if(Current_Question == 1)
        {
            return;
        }

        //回答をクラスに記録
        if(Question_Datas[Current_Question - 1].Likert)
        {
            foreach (Toggle temp_toggle in Likert_Toggle_Group.ActiveToggles())
            {
                Question_Datas[Current_Question - 1].Answer = Convert.ToSingle(temp_toggle.name);
            }
        }
        if(Question_Datas[Current_Question - 1].VisualAnalogScale)
        {
            Question_Datas[Current_Question - 1].Answer = Analog_Scale.value;
        }

        if (Current_Question > 1 && Current_Question <= Question_Datas.Length) Current_Question--; //現在の質問番号を変更
        Current_Answer = Question_Datas[Current_Question - 1].Answer; //回答値をリセット

        Next_Button_Text.text = "次へ";

        Update_Canvas(Current_Question);
    }

    //「次へ」の処理
    public void Next_Question()
    {
        //回答をクラスに記録
        if(Question_Datas[Current_Question - 1].Likert)
        {
            foreach (Toggle temp_toggle in Likert_Toggle_Group.ActiveToggles())
            {
                Question_Datas[Current_Question - 1].Answer = Convert.ToSingle(temp_toggle.name);
            }
        }
        if(Question_Datas[Current_Question - 1].VisualAnalogScale)
        {
            Question_Datas[Current_Question - 1].Answer = Analog_Scale.value;
        }

        //最後の質問の場合、回答終了処理
        if (Current_Question == Question_Datas.Count())
        {
            //ヘッダーの書き込み
            Csv_File = new csv_File(Save_Path);
            Csv_File.WriteData("ID,Question,Description,Likert,Likert_Point,Analog_Scale,Answer");

            foreach (Question_Data q in Question_Datas)
            {
                String text = q.Question_ID + "," + q.Question_Text + "," + q.Description_Text + "," + q.Likert + "," + q.Likert_Points + "," + q.VisualAnalogScale + "," + q.Answer;
                Csv_File.WriteData(text);
            }
            Csv_File.Close_File();

            Destroy(gameObject);
            return;
        }

        if (Current_Question >= 1 && Current_Question < Question_Datas.Count())Current_Question++; //現在の質問番号を更新

        Current_Answer = Question_Datas[Current_Question - 1].Answer; //回答値をリセット

        Update_Canvas(Current_Question);

        //質問番号更新後、最後の質問番号なら
        if(Current_Question == Question_Datas.Count()) Next_Button_Text.text = "回答終了";

    }

    //指定された質問番号のデータでCanvasを更新する関数
    private void Update_Canvas(int Question_num)
    {
        //テキストを更新
        Question_Text.text = Regex.Unescape(Question_Datas[Question_num - 1].Question_Text);
        Description_Text.text = Regex.Unescape(Question_Datas[Question_num - 1].Description_Text);

        //各回答UIの表示を設定
        if (Question_Datas[Question_num - 1].Likert) Likert_Scale_Obj.SetActive(true);
        else Likert_Scale_Obj.SetActive(false);
        if (Question_Datas[Question_num - 1].VisualAnalogScale) Analog_Scale_Obj.SetActive(true);
        else Analog_Scale_Obj.SetActive(false);

        //Toggleを更新
        if(Question_Datas[Question_num - 1].Likert)
        {
            //指定された数を超えるToggleを非アクティブに変更
            for(int x = 0; x < Likert_Scale_Toggle_List.Count; x++)
            {
                if (x + 1 <= Question_Datas[Question_num - 1].Likert_Points) Likert_Scale_Toggle_List[x].SetActive(true);
                else Likert_Scale_Toggle_List[x].SetActive(false);
            }
            Likert_Scale_Toggle_List[(int)Question_Datas[Question_num - 1].Answer - 1].GetComponent<Toggle>().isOn = true; //保存された回答値を再現

            //Toggleの位置を変更
            int Toggle_Offset = 140; //Toggle間の距離
            int Toggle_Pos;
            if (Question_Datas[Question_num - 1].Likert_Points % 2 == 0)
            {
                Toggle_Pos = Toggle_Offset * (-1) * (Question_Datas[Question_num - 1].Likert_Points / 2);
                Toggle_Pos += Toggle_Offset / 2;
            }
            else Toggle_Pos = Toggle_Offset * (-1) * (Question_Datas[Question_num - 1].Likert_Points / 2);
            foreach (GameObject temp in Likert_Scale_Toggle_List)
            {
                RectTransform temp_rect = temp.GetComponent<RectTransform>();
                temp_rect.localPosition = new Vector3(Toggle_Pos, temp_rect.localPosition.y, temp_rect.localPosition.z);
                Toggle_Pos += Toggle_Offset;
            }
        }

        //AnalogScaleを更新
        if(Question_Datas[Question_num - 1].VisualAnalogScale)
        {
            Analog_Scale.value = Question_Datas[Question_num - 1].Answer; //保存された回答値を再現
        }
    }

    //質問クラス
    [System.Serializable]
    public class Question_Data
    {
        [Tooltip("質問項目ID(自動で割り当てされます)")] public int Question_ID; //自動で番号が割り当てされる
        [Tooltip("質問文")] public string Question_Text;
        [Tooltip("説明文")] public string Description_Text;
        [Tooltip("リッカート尺度")] public bool Likert;
        [Tooltip("リッカート尺度の段階(min:1~max:10)")] public int Likert_Points;
        [Tooltip("ビジュアルアナログスケール")] public bool VisualAnalogScale;

        public float Answer; //質問の回答値

        //コンストラクタ
        public Question_Data(int Question_ID, string Question_Text,bool Likert = false, int Likert_Points = 3, bool Analog_Scale = false)
        {
            //質問データを初期化
            this.Question_ID = Question_ID;
            this.Question_Text = Question_Text;
            this.Likert = Likert;
            this.Likert_Points = Likert_Points;
            this.VisualAnalogScale = Analog_Scale;
            this.Answer = 1;

            Check_Question_Style(); //質問形式を確認
        }

        //質問項目が複数選択されているかチェック
        public void Check_Question_Style()
        {
            //質問形式が2つ以上Trueの場合、エラー
            int count = Convert.ToInt32(this.Likert) + Convert.ToInt32(this.VisualAnalogScale);

            if (count >= 2) Debug.LogError("質問形式が複数選択されています");
            else if (count <= 0) Debug.LogError("質問が選択されていません");
            else {; }
        }
    }
}
