using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Suzuryg.FaceEmo.Detail
{
    internal sealed class ReadableErrorWindow : EditorWindow
    {
        private string _message;
        private string _stackTrace;
        private ScrollView _scroll;

        public static void Open(string title, string message, string stackTrace)
        {
            var w = CreateInstance<ReadableErrorWindow>();
            w._message = message;
            w._stackTrace = stackTrace;
            w.titleContent = new GUIContent(title);
            w.minSize = new Vector2(720, 720);
            w.ShowModal();
        }

        private void CreateGUI()
        {
            var bar = new Toolbar();
            var copyBtn = new ToolbarButton(() => GUIUtility.systemCopyBuffer = _stackTrace) { text = "Copy" };
            bar.Add(copyBtn);
            rootVisualElement.Add(bar);

            _scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1
                }
            };
            rootVisualElement.Add(_scroll);

            var label = new Label(_message ?? "(no error)")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.UpperLeft,
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 6,
                    paddingBottom = 6
                }
            };
            label.AddToClassList("code");
            _scroll.Add(label);

            var tf = new TextField
            {
                multiline = true, value = _stackTrace ?? "(no stack trace)",
                isReadOnly = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal
                }
            };
            _scroll.Add(tf);

            var footer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginTop = 6
                }
            };

            var close = new Button(Close)
            {
                text = "Close",
                style =
                {
                    width = 100,
                    height = 30,
                }
            };
            footer.Add(close);
            rootVisualElement.Add(footer);
        }
    }
}
