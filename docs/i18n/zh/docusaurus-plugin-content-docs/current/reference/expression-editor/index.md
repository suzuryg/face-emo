---
sidebar_position: 3
sidebar_label: 表情编辑器
---

# 表情编辑器

表情编辑器用于编辑表情动画。

## 打开表情编辑器

将鼠标悬停在表情缩略图上时，会显示用于操作表情动画的按钮。  
此时，点击「打开」以外的任意按钮，都会进入表情编辑器。

![添加 Shape Key](thumbnail_mouseover.png)

|<center>操作</center>|<center>按钮位置</center>|<center>说明</center>|
|:-:|:-:|:-|
|新建|左上|创建一个新的动画剪辑。<br/>打开表情编辑器并编辑新建的动画剪辑。|
|打开|右上|打开已有的动画剪辑。|
|合成|左下|选择两个动画剪辑并将其合成。<br/>若两个动画都包含相同的 Shape Key，将优先采用数值较大的那一个。|
|编辑|右下|打开表情编辑器，编辑当前设置的动画剪辑。|

:::tip
在合成动画时，如果其中一个设置为「None」，执行合成后，另一个动画会被直接复制。

![合成](combine.png)
:::

---

## 添加 Shape Key

点击想添加到表情动画中的 Shape Key 后，该 Shape Key 会被加入动画中，并立即更新预览。  
若要修改 Shape Key 的数值，可拖动滑块或直接在文本框中输入数值。

![添加 Shape Key](add_blendshape.png)

:::caution
添加 Shape Key 时，请确认是否出现以下警告：

|<center>警告信息</center>|<center>说明</center>|
|:-|:-|
|「まばたき用シェイプキーが含まれています！」|该 Shape Key 可能会被眨眼动画覆盖，导致表情动画无法正确播放。|
|「リップシンク用シェイプキーが含まれています！」|该 Shape Key 可能会影响嘴型同步功能。|
:::

---

## 按类别显示 Shape Key

通过输入 Shape Key 的**分隔符**，可以将 Shape Key 按类别分组显示。

Shape Key 的分隔符因 Avatar 不同而异：
- 若格式为「=====Eye=====」，请输入「=====」
- 若格式为「*****Eye*****」，请输入「*****」

![Shape Key 分隔符](blendshape_delimiter.png)

---

## 操作预览窗口

预览窗口的操作方式与 Unity 的 Scene 视图相同，可进行移动、旋转、缩放等操作。

预览窗口的相机设置会继承 Scene 视图的参数。  
例如，在 Scene 视图中修改了 FOV（视场角），预览窗口也会同步应用该更改。

![表情编辑器预览](preview_expression_editor.png)

---

## 修改布局

表情编辑器的窗口布局可以自由调整。  
修改后的布局会在下次启动时自动保留。

![修改布局](change_layout.png)
