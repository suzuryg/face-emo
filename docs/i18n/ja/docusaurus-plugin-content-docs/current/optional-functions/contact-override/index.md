---
sidebar_position: 6
sidebar_label: 撫でられたときの表情を設定する
---

# 撫でられたときの表情を設定する

下記の手順で、特定の場所に触れられたときの表情を設定できます。

Projectビューで右クリックして「Create」→「FaceEmo_EmoteOverrideExample」を選ぶと、PrefabとAnimatorControllerが作成されます。

![FaceEmo_EmoteOverrideExample実行](create_prefab.png)

![PrefabとAnimatorController](create_prefab2.png)

作成されたAnimatorControllerを開き、「Active」をダブルクリックします。

![Activeをダブルクリック](select_active.png)

「NadeNade」をクリックし、「Motion」に好きな表情アニメーションをセットします。

![表情アニメーションをセット](select_nadenade.png)

Prefabをヒエラルキーに置き、「FaceEmoPrefab」の中に配置します。  
この状態でアバターをアップロードすると、頭に他プレイヤーの手が触れたときに表情が上書きされます。

![Prefabを配置](deploy_prefab.png)

:::tip
「FaceEmoPrefab」がアバター内に存在しない場合は、先に表情メニューをアバターに適用してください。
:::

:::tip
FaceEmo_EmoteOverrideExampleをベースにして改変することにより、頭以外にもContactを設定することができます。
:::
