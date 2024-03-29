---
sidebar_position: 7
sidebar_label: Expression Menuで表情の設定を変更する
---

# Expression Menuで表情の設定を変更する

Expression Menuから「FaceEmo」→「設定」を開き、表情の設定を変更することができます。

![設定メニューを開く](select_setting.png)
![設定メニューの内容](setting_menu.png)

|<center>項目名</center>|<center>設定内容</center>|
|:-:|:-|
|表情固定ON|ONにしている間、表情を固定できます。|
|まばたきOFF|ONにしている間、まばたきを無効にできます。|
|ダンスギミック|ONにしている間、ダンスギミック中に表情が連動して動くようになります。<br/> FXレイヤーの機能が無効化されるため、衣装切り替え等を使用している場合はデフォルトの状態に戻ります。|
|Contact表情固定|ONにしている間、Contactによる表情固定機能が使用できるようになります。 <br/>両手を頭の上に2秒間置くと効果音が鳴り、表情が固定されます。 <br/>表情が固定された状態で同様の操作をすると、表情固定が解除されます。|
|表情上書き|ONにしている間、Contactによる表情上書き機能が使用できるようになります。<br/> FaceEmo_EmoteOverrideExampleをアバターに組み込んだ状態で使用してください。<br/>[Contactによる表情上書き機能についてはこちらで説明しています。](../contact-override)|
|発話中は表情遷移しない|ONにしている間、発話中に表情が変化しなくなります。 <br/>リップシンク無効の表情がある場合、これをONにすることで口が開いたまま固定されることが無くなります。|
|ジェスチャー設定|表情変更に使用するジェスチャーの設定を変更できます。<br/>左右入れ替え：左右のジェスチャーを入れ替えて表情変更を行います<br/>左手を無効化：左手のジェスチャーによる表情変更を無効にします（Neutralとして扱います）<br/>右手を無効化：右手のジェスチャーによる表情変更を無効にします（Neutralとして扱います）|
|コントローラ切り替え|各コントローラで誤って入力されやすいジェスチャーを無効化できます<br/>Questコントローラ：Openのジェスチャーによる表情変更を無効にします（Neutralとして扱います）<br/>Indexコントローラ：Fistのジェスチャーによる表情変更を無効にします（Neutralとして扱います）|

設定メニューに追加する項目と初期値は、インスペクタの「Expression Menu設定項目」で設定します。

![Expression Menu設定項目](setting_items.png)
