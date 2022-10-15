using UnityEditor;
using Newtonsoft.Json;
using UniRx;

public class ExampleEditorScript
{
    public ReactiveProperty<string> ExampleProperty { get; }

    [MenuItem("Example Editor Script/Test")]
    static void Test()
    {
        EditorUtility.DisplayDialog("Example Script", "Opened This Dialog", "OK");
    }
}
