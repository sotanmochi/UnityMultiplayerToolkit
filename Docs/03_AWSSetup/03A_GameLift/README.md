# Setup GameLift Server
## Build Server

- GameRoomContextのIsLocalServerをfalseに設定

<img src="./ServerSetup_AWS_01.png">

- Linuxビルド

<img src="./ServerSetup_AWS_02.png">
<img src="./ServerSetup_AWS_03.png">

## Setup GameLift Server
- AWS CLIを使ってビルドをアップロードする

<img src="./ServerSetup_AWS_04.png">
<img src="./ServerSetup_AWS_05.png">
<img src="./ServerSetup_AWS_06.png">

## Create Fleet
- フリートを作成する

<img src="./ServerSetup_AWS_07.png">
<img src="./ServerSetup_AWS_08.png">
<img src="./ServerSetup_AWS_09.png">

### Settings
- ポート番号を設定する
- 1つのサーバーで複数プロセスの起動が可能

<img src="./ServerSetup_AWS_10.png">
<img src="./ServerSetup_AWS_11.png">
<img src="./ServerSetup_AWS_12.png">

## Created Fleet
<img src="./ServerSetup_AWS_13.png">
<img src="./ServerSetup_AWS_14.png">
