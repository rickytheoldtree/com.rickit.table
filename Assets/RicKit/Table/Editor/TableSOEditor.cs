using UnityEditor;
using UnityEngine;

namespace RicKit.Table.Extensions.Editor
{
    [CustomEditor(typeof(TableScriptableObject))]
    public class TableScriptableObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty tableName;
        private SerializedProperty columnInfos;
        private SerializedProperty rows;

        private void OnEnable()
        {
            tableName = serializedObject.FindProperty("tableName");
            columnInfos = serializedObject.FindProperty("columnInfos");
            rows = serializedObject.FindProperty("rows");
        }

        private int currentPage = 1;
        private const int RowsPerPage = 20; // 每页显示的行数，根据需要调整
        private int totalPages; // 总页数

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(tableName);
            EditorGUILayout.PropertyField(columnInfos, true);

            //根据columnInfos绘制表头
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            var headerStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };
            for (var i = 0; i < columnInfos.arraySize; i++)
            {
                var columnInfo = columnInfos.GetArrayElementAtIndex(i);
                var n = columnInfo.FindPropertyRelative("name").stringValue;
                //和输入框一样宽，自动拉伸
                EditorGUILayout.LabelField(n, headerStyle, GUILayout.Width(50),
                    GUILayout.ExpandWidth(true));
            }

            if (columnInfos.arraySize > 0 && GUILayout.Button("", GUILayout.Width(20)))
            {
            }
            EditorGUILayout.EndHorizontal();
            
            // 计算总页数
            totalPages = (rows.arraySize + RowsPerPage - 1) / RowsPerPage;

            // 绘制当前页的行
            var startRow = (currentPage - 1) * RowsPerPage;
            var endRow = Mathf.Min(startRow + RowsPerPage, rows.arraySize);
            for (var i = startRow; i < endRow; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(i.ToString());
                var row = rows.GetArrayElementAtIndex(i);
                var values = row.FindPropertyRelative("values");

                for (var j = 0; j < values.arraySize; j++)
                {
                    EditorGUILayout.PropertyField(values.GetArrayElementAtIndex(j), GUIContent.none,
                        GUILayout.Width(50), GUILayout.ExpandWidth(true));
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    rows.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            // 分页控制
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = currentPage > 1;
            if (GUILayout.Button("Prev"))
            {
                currentPage--;
            }

            GUI.enabled = currentPage < totalPages;
            if (GUILayout.Button("Next"))
            {
                currentPage++;
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // 显示当前页码和总页数
            EditorGUILayout.LabelField($"Page {currentPage} of {totalPages}");
            if (GUILayout.Button("Add Row"))
            {
                //跳到最后一页
                rows.arraySize++;
                totalPages = (rows.arraySize + RowsPerPage - 1) / RowsPerPage;
                currentPage = Mathf.Max(1, totalPages);
                var newRow = rows.GetArrayElementAtIndex(rows.arraySize - 1);
                var values = newRow.FindPropertyRelative("values");
                values.arraySize = columnInfos.arraySize;
            }

            if (GUILayout.Button("Save CSV"))
            {
                var tableSO = (TableScriptableObject)target;
                var table = ScriptableObjectExtensions.FromTableSO(tableSO);
                var csv = table.ToCsv();
                var path = AssetDatabase.GetAssetPath(target);
                path = EditorUtility.SaveFilePanel("Save CSV", System.IO.Path.GetFullPath(path),
                    System.IO.Path.GetFileNameWithoutExtension(path), "csv");
                if (path.Length != 0)
                {
                    System.IO.File.WriteAllText(path, csv);
                    AssetDatabase.Refresh();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Assets/Create/RicKit/Table/From CSV")]
        public static void CsvToTableScriptableObject()
        {
            var selected = Selection.activeObject;
            if (!(selected is TextAsset)) return;
            var path = AssetDatabase.GetAssetPath(selected);
            var csv = System.IO.File.ReadAllText(path);
            var table = CsvExtensions.FromCsv(csv);
            var tableSO = table.ToTableSO();
            var savePath = EditorUtility.SaveFilePanel("Save TableScriptableObject",
                System.IO.Path.GetDirectoryName(path),
                System.IO.Path.GetFileNameWithoutExtension(path), "asset");
            if (savePath.Length == 0) return;
            savePath = savePath.Replace(Application.dataPath, "Assets");
            AssetDatabase.CreateAsset(tableSO, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/RicKit/Table/Create TableScriptableObject")]
        public static void CreateTableScriptableObject()
        {
            var tableSO = CreateInstance<TableScriptableObject>();
            var selected = Selection.activeObject;
            var path = "Assets";
            if (selected)
            {
                path = AssetDatabase.GetAssetPath(selected);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                }
            }

            path = AssetDatabase.GenerateUniqueAssetPath(path + "/NewTableScriptableObject.asset");
            AssetDatabase.CreateAsset(tableSO, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}