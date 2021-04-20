# SimpleMultiplayer Client Connection to Server on AWS

## 1. 設定ファイルの作成
- "Create -> UnityMultiplayerToolkit -> Create AWS Config"からコンフィグファイルを作成する
- リージョン、Cognito ID PoolのID、Lambdaの関数名をセットする

<img src="./ClientSetup_AWS_01.png">
<img src="./ClientSetup_AWS_02.png">
<img src="./ClientSetup_AWS_03.png">


## 2. シーン更新
- AWS ConfigをAWSConnectionConfigProviderにセットする
- MultiplayerContextのConnectionConfigProvidrにAWSConnectionConfigProviderをセットする

<img src="./ClientSetup_AWS_04.png">
<img src="./ClientSetup_AWS_05.png">


## 3. クライアントシーンの実行（Unity Editor）
- ルーム名を入力してJoinボタンを押す
- プレイヤーが表示されればOK

<img src="./ClientSetup_AWS_06.png">
