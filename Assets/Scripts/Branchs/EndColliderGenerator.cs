using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class EndColliderGenerator : MonoBehaviour
{
    public Vector3 boxSize = new Vector3(4f, 4f, 2f); // Width, Height, Depth
    public string connectorName = "Connector";
    MeshFilter filter; 
    BoxCollider collider;

    public void GenerateLevelEnd(Branch branch)
    {
        if (branch == null)
        {
            Debug.LogError("No branch provided!");
            return;
        }

        Transform branchTransform = branch.transform;
        Transform connector = branch.transform.Find(connectorName);

        if (connector == null)
        {
            Debug.LogError($"Branch is missing a child named '{connectorName}'.");
            return;
        }

        Vector3 direction = (connector.position - branchTransform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 center = Vector3.Lerp(branchTransform.position, connector.position, 0.5f);

        // Create box mesh
        Mesh mesh = CreateBoxMesh(boxSize); 
        filter.mesh = mesh;

        // Set transform
        transform.position = center;
        transform.rotation = lookRotation;

        // Set collider 
        collider.size = boxSize;
        collider.center = Vector3.zero; // Since transform is centered
    }

    private Mesh CreateBoxMesh(Vector3 size)
    {
        float w = size.x * 0.5f;
        float h = size.y * 0.5f;
        float d = size.z * 0.5f;

        Vector3[] vertices = {
            new Vector3(-w, -h, -d), new Vector3(w, -h, -d), new Vector3(w, h, -d), new Vector3(-w, h, -d),
            new Vector3(-w, -h, d),  new Vector3(w, -h, d),  new Vector3(w, h, d),  new Vector3(-w, h, d)
        };

        int[] triangles = {
            // Front
            0, 2, 1, 0, 3, 2,
            // Back
            5, 6, 4, 6, 7, 4,
            // Left
            4, 7, 0, 7, 3, 0,
            // Right
            1, 2, 5, 2, 6, 5,
            // Top
            3, 7, 2, 7, 6, 2,
            // Bottom
            4, 0, 5, 0, 1, 5
        };

        Mesh mesh = new Mesh();
        mesh.name = "EndColliderBox";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
    public void Clean()
    {
        // Clean up the mesh 
        if (filter != null)
        { 
            DestroyImmediate(filter.sharedMesh); 
            filter.mesh = null;
        }
    
        // Reset collider 
        if (collider != null)
        {
            collider.size = Vector3.zero;
            collider.center = Vector3.zero;
        }
    
        // Optionally reset transform (position/rotation), if desired
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

}
