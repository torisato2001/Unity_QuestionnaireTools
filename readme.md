# Unity質問紙ツール
### 【概要】
Unityの実験プログラムで、質問紙による主観評定を簡単に行えるPrefabです。以下の機能を実装しています。
- リッカート尺度(1~10件法を自由に変更可能)
- ビジュアルアナログスケール
- 質問順のランダム化
- 質問と回答のcsv書き出し

### 【使い方】
使い方は、unitypackage内にあるデモシーンを参考にしてください。
1. UnityにQuestionnaireTools.unitypackageをインポートしてください。
2. TextMeshProがインポートされていない場合、インストールを促されます。すべてインポートしてください。
3. Inspectorで質問項目、ランダマイズなどを設定してください。

### 【その他】
- デフォルトではマウス、キーボード(矢印キー)の入力に対応しています。他のコントローラに対応させる場合、スクリプト(Questionnaire_Manager)内にある、以下の関数を呼び出すことで、UIの操作が可能です。  
    - 「次へ」: Next_Question()
    - 「戻る」: Prev_Question()
    - 「←」: Move_Left()
    - 「→」: Move_Right()