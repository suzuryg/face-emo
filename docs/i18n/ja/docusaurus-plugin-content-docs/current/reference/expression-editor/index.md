---
sidebar_position: 3
sidebar_label: 表情エディタ
---

# 表情エディタ

表情エディタでは、表情アニメーションの編集を行うことができます。

## 表情エディタを開く

表情サムネイルにマウスカーソルを合わせると、表情アニメーションに関する操作を行うボタンが表示されます。  
ここで、「開く」以外のボタンを押すと表情エディタが開きます。

![シェイプキー追加](thumbnail_mouseover.png)

|<center>アクション</center>|<center>ボタン位置</center>|<center>説明</center>|
|:-:|:-:|:-|
|新規作成|左上|新しいアニメーションクリップを作成します。<br/>表情エディタを開き、新しく作成されたアニメーションクリップを編集します。|
|開く|右上|既存のアニメーションクリップを開きます。|
|合成|左下|アニメーションクリップを2つ選んで合成します。<br/>同じシェイプキーが使われている場合、値が大きい方を優先します。|
|編集|右下|表情エディタを開き、セットされているアニメーションクリップを編集します。|

:::tip
アニメーションを合成するときに片方を「None」にして合成を実行すると、もう片方のアニメーションがそのままコピーされます。

![合成](combine.png)
:::

---

## シェイプキーを追加する

表情アニメーションに追加したいシェイプキーをクリックするとシェイプキーが追加され、プレビューが更新されます。  
シェイプキーの値を変更したい場合、各シェイプキーのスライダーを動かすか、テキストボックスに数値を入力します

![シェイプキー追加](add_blendshape.png)

:::caution
シェイプキーを追加したとき、下記の警告が出ていないか確認してください。

|<center>警告メッセージ</center>|<center>説明</center>|
|:-|:-|
|「まばたき用シェイプキーが含まれています！」|まばたきアニメーションに上書きされ、<br/>表情アニメーションが正しく再生されない可能性があります。|
|「リップシンク用シェイプキーが含まれています！」|リップシンクが正しく動かなくなる可能性があります。|
:::

---

## シェイプキーをカテゴリごとに表示する

シェイプキーの区切り文字を入力することで、シェイプキーをカテゴリごとに表示することができます。

シェイプキーの区切り文字はアバターごとに異なります。
- 「=====Eye=====」のようなカテゴリ名となっている場合、「=====」と入力してください
- 「\*\*\*\*\*Eye\*\*\*\*\*」のようなカテゴリ名となっている場合、「\*\*\*\*\*」と入力してください

![シェイプキー区切り文字](blendshape_delimiter.png)

---

## プレビュー画面を操作する

プレビュー画面は、Unityのシーンビューと同様の操作で移動・回転・拡大縮小することが可能です。

プレビュー画面のカメラ設定はシーンビューの設定を引き継ぎます。  
たとえば、シーンビューでFOVを変更して作業している場合、プレビュー画面にも変更したFOVが適用されます。

![表情エディタプレビュー](preview_expression_editor.png)

---

## レイアウトを変更する

表情エディタのウインドウ配置は自由に変更することができます。
変更したウインドウ配置は、次回起動時にも反映されます。

![レイアウト変更](change_layout.png)