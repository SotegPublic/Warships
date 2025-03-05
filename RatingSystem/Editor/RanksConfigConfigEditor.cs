using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RanksConfig))]
public class RanksConfigConfigEditor : Editor
{
    private RanksConfig _target;
    private GUIStyle _labelStyle;
    private GUIStyle _errorStyle;
    private SerializedObject _serializedObject;
    private bool _isRepLevelsShow;

    private void OnEnable()
    {
        _target = (RanksConfig)target;
        _serializedObject = new SerializedObject(_target);

        _labelStyle = new GUIStyle();
        _labelStyle.fixedHeight = 10f;
        _labelStyle.fixedWidth = 200f;
        _labelStyle.font = EditorStyles.boldFont;
        _labelStyle.normal.textColor = Color.white;

        _errorStyle = new GUIStyle();
        _errorStyle.fixedHeight = 10f;
        _errorStyle.fixedWidth = 200f;
        _errorStyle.font = EditorStyles.boldFont;
        _errorStyle.normal.textColor = Color.red;
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();
        EditorGUILayout.Space(10f);
        EditorGUI.BeginDisabledGroup(true);
        _target.MaxRanksCount = EditorGUILayout.IntField("Max Ranks Count", _target.MaxRanksCount, GUILayout.Height(20));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space(5f);

        EditorGUILayout.BeginHorizontal();
        _isRepLevelsShow = EditorGUILayout.Foldout(_isRepLevelsShow, "List Of Reputation Per Rank");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10f);

        if (_isRepLevelsShow)
        {
            for (int i = 0; i < _target.RanksList.Count; i++)
            {
                GUILayout.Label("Rank " + i, _labelStyle);
                _target.RanksList[i].Rank = EditorGUILayout.TextField("Rank Name", _target.RanksList[i].Rank, GUILayout.Height(20));
                _target.RanksList[i].MaxRatingForRank = EditorGUILayout.FloatField("Rating Count for Up", _target.RanksList[i].MaxRatingForRank, GUILayout.Height(20));
            }
            EditorGUILayout.Space(10f);
            ShowButtons();
        }


        _serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(_target);

        EditorGUILayout.Space();

        if (CheckForCorrectInput())
        {
            var saveButton = GUILayout.Button(new GUIContent("Save Changes", "Save"), GUILayout.Width(100f));

            if (saveButton)
            {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
        else
        {
            GUILayout.Label("Ошибка ввода данных\nОдин из уровней ранга выше предыдущего", _errorStyle);
        }
    }

    private bool CheckForCorrectInput()
    {
        bool isError = false;

        for (int i = 0; i < _target.RanksList.Count; i++)
        {
            if (i == 0)
            {
                continue;
            }

            if (_target.RanksList[i].MaxRatingForRank <= _target.RanksList[i - 1].MaxRatingForRank)
            {
                isError = true;
            }
        }

        return !isError;
    }

    private void ShowButtons()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        var addButton = GUILayout.Button(new GUIContent("+", "Add"), EditorStyles.miniButtonLeft, GUILayout.Width(20));
        var deleteButton = GUILayout.Button(new GUIContent("-", "Delete"), EditorStyles.miniButtonMid, GUILayout.Width(20));
        var clearButton = GUILayout.Button(new GUIContent("clear", "Clear"), EditorStyles.miniButtonRight, GUILayout.Width(40));

        if (addButton)
        {
            _target.AddLevel();
            _target.MaxRanksCount = _target.RanksList.Count;
        }

        if (deleteButton)
        {
            _target.RemoveLastLevel();
            _target.MaxRanksCount = _target.RanksList.Count;
        }

        if (clearButton)
        {
            _target.ClearList();
            _target.MaxRanksCount = _target.RanksList.Count;
        }
        EditorGUILayout.EndHorizontal();
    }
}
