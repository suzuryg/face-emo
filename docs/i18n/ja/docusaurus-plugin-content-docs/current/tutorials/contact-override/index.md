---
sidebar_position: 9
sidebar_label: Contactで表情を上書きする
---

# Contactで表情を上書きする

- Projectビューで右クリックし、CreateからFaceEmo_EmoteOverrideExampleを選ぶと、PrefabとAnimatorControllerが作成される
- AnimatorControllerを開き、Active → NadeNade に表情アニメーションを割り当てる
- Prefabをヒエラルキーに置き、FaceEmoPrefabの子として配置する
- この状態でアバターをアップロードすると、頭に他プレイヤーの手が触れたとき表情が上書きされる
- FaceEmo_EmoteOverrideExampleをベースにして改変することにより、頭以外にもContactを設定できる