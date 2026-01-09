using UnityEngine;
using UnityEditor;

public class ReplaceToPrefab : EditorWindow
{
    public GameObject prefab;
    
    [MenuItem("Tools/Remplacer par Prefab")]
    static void CreateWindow()
    {
        ReplaceToPrefab window = (ReplaceToPrefab)GetWindow(typeof(ReplaceToPrefab));
        window.titleContent = new GUIContent("Remplacer par Prefab");
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Remplacer les objets sélectionnés par un Prefab", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab de remplacement", prefab, typeof(GameObject), false);
        
        GUILayout.Space(10);
        
        if (Selection.gameObjects.Length > 0)
        {
            GUILayout.Label($"Objets sélectionnés : {Selection.gameObjects.Length}");
        }
        else
        {
            EditorGUILayout.HelpBox("Sélectionne les objets à remplacer dans la Hierarchy", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        GUI.enabled = prefab != null && Selection.gameObjects.Length > 0;
        
        if (GUILayout.Button("Remplacer la sélection"))
        {
            ReplaceSelectedObjects();
        }
        
        GUI.enabled = true;
        
        GUILayout.Space(20);
        EditorGUILayout.HelpBox(
            "Instructions :\n" +
            "1. Glisse ton prefab 'Mur' dans le champ ci-dessus\n" +
            "2. Sélectionne tous les murs à remplacer dans la Hierarchy\n" +
            "3. Clique sur 'Remplacer la sélection'\n\n" +
            "Les positions, rotations et échelles seront conservées.", 
            MessageType.None);
    }
    
    void ReplaceSelectedObjects()
    {
        if (prefab == null)
        {
            Debug.LogError("Aucun prefab sélectionné !");
            return;
        }
        
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Aucun objet sélectionné !");
            return;
        }
        
        Undo.SetCurrentGroupName("Remplacer par Prefab");
        int undoGroup = Undo.GetCurrentGroup();
        
        int replacedCount = 0;
        
        foreach (GameObject obj in selectedObjects)
        {
            // Sauvegarder le transform
            Transform parent = obj.transform.parent;
            Vector3 position = obj.transform.position;
            Quaternion rotation = obj.transform.rotation;
            Vector3 scale = obj.transform.localScale;
            int siblingIndex = obj.transform.GetSiblingIndex();
            string originalName = obj.name;
            
            // Appliquer les valeurs fixes du prefab pour Y
            position.y = 1.25f;  // Position Y à 1.25 (moitié de la hauteur pour être au sol)
            scale.y = 2.5f;      // Scale Y fixe à 2.5
            
            // Créer l'instance du prefab
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(newObject, "Créer prefab");
            
            // Appliquer le transform
            newObject.transform.SetParent(parent);
            newObject.transform.position = position;
            newObject.transform.rotation = rotation;
            newObject.transform.localScale = scale;
            newObject.transform.SetSiblingIndex(siblingIndex);
            newObject.name = originalName; // Garder le nom original
            
            // Supprimer l'ancien objet
            Undo.DestroyObjectImmediate(obj);
            
            replacedCount++;
        }
        
        Undo.CollapseUndoOperations(undoGroup);
        
        Debug.Log($"<color=green>✓ {replacedCount} objet(s) remplacé(s) avec succès !</color>");
    }
}
