using UnityEngine;
using UnityEditor;

public class HierarchyLogger
{
    [MenuItem("Dash Frenzy/Log Track Hierarchy")]
    public static void LogHierarchy()
    {
        string[] tiles = { "Tile_001", "Tile_002", "Tile_003" };
        foreach (string tName in tiles)
        {
            GameObject tile = GameObject.Find(tName);
            if (tile == null)
            {
                Debug.LogWarning("HierarchyLogger: " + tName + " not found in scene!");
                continue;
            }

            Debug.Log(string.Format("TILE: {0} | Active: {1} | Scale: {2} | Position: {3} | Renderer Enabled: {4}",
                tile.name, tile.activeSelf, tile.transform.localScale, tile.transform.position,
                tile.GetComponent<Renderer>() ? tile.GetComponent<Renderer>().enabled.ToString() : "No Renderer"));

            for (int i = 0; i < tile.transform.childCount; i++)
            {
                Transform child = tile.transform.GetChild(i);
                Debug.Log(string.Format("  -> CHILD: {0} | Active: {1} | Local Scale: {2} | Local Position: {3} | Mesh: {4}",
                    child.name, child.gameObject.activeSelf, child.localScale, child.localPosition,
                    child.GetComponent<MeshFilter>() ? child.GetComponent<MeshFilter>().sharedMesh.name : "No Mesh"));
            }
        }
    }
}
