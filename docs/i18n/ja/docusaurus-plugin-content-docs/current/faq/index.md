---
sidebar_position: 4
sidebar_label: よくある質問
---

# よくある質問

## ハンドジェスチャーを変えても表情が変わりません。

下記のポイントを確認してください。

|<center>確認ポイント</center>|<center>対処方法</center>|
|:-|:-|
| Expression Menuに「FaceEmo」が追加されていない|[チュートリアル](../tutorials/)の手順に沿って、表情メニューをアバターに適用してください。|
| 「FaceEmo」メニューで表情パターンが正しく選択されていない|適切な表情パターンを選択してください。|
| 「FaceEmo」→「表情選択」メニューが開かれている|「表情選択」メニューを「Back」で閉じてください。<br/>（左手と右手の両方のExpression Menuを確認してください）|
| 「FaceEmo」→「表情選択」→「表情固定ON」が有効になっている|「表情固定ON」を無効にしてください。|
| 「FaceEmo」→「設定」→「表情固定ON」が有効になっている|「表情固定ON」を無効にしてください。|

参考：[オプション機能の紹介 > 表情を固定する](../optional-functions/emote-lock/)

## アバターにもともと設定されていた表情と併用できますか？

表情の干渉を防ぐため、アバターにもともと設定されていた表情は無効化されます。  
お手数ですが、同様の表情設定を本ツールの表情メニューに追加してください。

## アバターから表情メニューを削除するにはどうすればいいですか？

アバターの中に追加されている「FaceEmoPrefab」を削除してください。  
参考：[チュートリアル > 簡単な表情メニューを作成する](../tutorials/simple-menu/)

## 「次回から表示しない」をチェックしたダイアログを再び表示することはできますか？

インスペクタの「エディタ設定」からダイアログの表示有無を変更できます。

## UIの表示がおかしくなります。

下記のような不具合が発生している場合、いったんPlayモードに移行してからEditモードに戻ることで不具合が解消されることがあります。

- 各ビューの幅が正しく調整されない
- 表情のサムネイルが描画されない
