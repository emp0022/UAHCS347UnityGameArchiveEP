using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using EzySlice;

/* 
 * Evan Pagani CS347 Project: Under Siege!
 * This script handles the destruction mechanic characterzation of different types of structures (Wall, Square, Slab) made of different materials (Wood, Stone). 
 * It manages their health, destruction behavior, and the generation of debris when the structure is destroyed. 
 * The debris inherits properties from the original structure and can further split into smaller pieces if damaged enough. This logic is in DebrisPiece.cs though. 
 * The script uses the EzySlice library to perform slicing operations for generating debris. Thank you EzySlice ! ! !
 */

// Define the material types for the structure
public enum MaterialType
{
    Wood, // Material type Wood
    Stone // Material type Stone
}

// Define the structure types
public enum StructureType
{
    Wall,
    Square,
    Slab,
    RightTriangle
}


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class damageCal : MonoBehaviour
{
    [Header("Structure Properties")]
    public float health = 100f;  // Initial health of the structure
    public float damageFactor = 1f; // Damage multiplier
    public StructureType structureType = StructureType.Wall; // The type of structure

    [Header("Material Settings")]
    public MaterialType materialType = MaterialType.Wood; // The type of material (currently Wood or Stone)

    [Header("Materials")]
    public Material crossSectionMaterial; // Material for internal cross sections (for debug purposes currently)
    public Material woodMaterial; // Material for wood structures
    public Material stoneMaterial; // Material for stone structures

    [Header("Destruction Properties")]
    public int minDebrisPieces; // Minimum number of debris pieces generated upon destruction
    public int maxDebrisPieces; // Maximum number of debris pieces generated upon destruction
    public float explosionForce; // Force applied to debris explosion

    [Header("Debris Health Settings")]
    public float baseDebrisHealth = 50f; // Base health value for debris pieces
    public float debrisHealthMultiplier = 1.5f; // Multiplier for health of debris pieces
    public int maxSplitLevel = 10; // Maximum level of splitting for debris pieces

    [Header("Cost")]
    public float cost; // The total cost of the structure

    private MeshFilter meshFilter; // Reference to the MeshFilter component
    private MeshRenderer meshRenderer; // Reference to the MeshRenderer component
    private bool isDestroyed = false; // Flag to check if the structure is already destroyed
    private float originalVolume; // Store the original object's volume
    private float originalMass; // Store the original object's mass

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        SetName(); // Set the initial name based on material type and structure type
        SetMaterialDependentProperties(); // Set properties based on material type

        // Calculate and store the original volume and mass
        originalVolume = CalculateMeshVolume(meshFilter.sharedMesh);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            originalMass = rb.mass;
        }
        else
        {
            // Set default mass if Rigidbody is missing
            originalMass = materialType == MaterialType.Wood ? 2f : 5f;
        }
    }

    // Called when properties are modified in the Inspector
    private void OnValidate()
    {
        SetName(); // Update the name whenever the material type or structure type is changed
        SetMaterialDependentProperties(); // Update properties when material type changes
    }

    // Set the name of the object based on its material type and structure type
    private void SetName()
    {
        string materialName = materialType == MaterialType.Wood ? "Wood" : "Stone";
        string structureName = "";

        switch (structureType)
        {
            case StructureType.Wall:
                structureName = "Wall";
                break;
            case StructureType.Square:
                structureName = "Square";
                break;
            case StructureType.Slab:
                structureName = "Slab";
                break;
            case StructureType.RightTriangle:
                structureName = "RightTriangle";
                break;
            default:
                structureName = "Unknown";
                break;
        }

        gameObject.name = $"{materialName}_{structureName}";
    }


    // Set properties based on the selected material type
    private void SetMaterialDependentProperties()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        switch (materialType)
        {
            case MaterialType.Wood:
                meshRenderer.material = woodMaterial;
                health = 100f;
                damageFactor = 1f;
                minDebrisPieces = 2;
                maxDebrisPieces = 9;
                explosionForce = 3f;
                baseDebrisHealth = 50f;
                maxSplitLevel = 7;
                debrisHealthMultiplier = 3f;
                break;
            case MaterialType.Stone:
                meshRenderer.material = stoneMaterial;
                health = 300f;
                damageFactor = 1f;
                minDebrisPieces = 2;
                maxDebrisPieces = 3;
                explosionForce = 1f;
                baseDebrisHealth = 100f;
                maxSplitLevel = 6;
                debrisHealthMultiplier = 2f;
                break;
        }

        // Calculate the cost based on structure type and material type
        cost = GetBaseCost(structureType) * GetMaterialMultiplier(materialType);

        UpdateRigidbodyProperties();
    }

    // Get the base cost for a structure type
    private float GetBaseCost(StructureType structureType)
    {
        switch (structureType)
        {
            case StructureType.Wall:
                return 100f;
            case StructureType.Square:
                return 150f;
            case StructureType.Slab:
                return 100f;
            case StructureType.RightTriangle:
                return 200f;
            default:
                return 100f;
        }
    }


    // Get the cost multiplier for a material type
    private float GetMaterialMultiplier(MaterialType materialType)
    {
        switch (materialType)
        {
            case MaterialType.Wood:
                return 1f;
            case MaterialType.Stone:
                return 2f;
            default:
                return 1f;
        }
    }
    // Update Rigidbody properties based on the material type
    private void UpdateRigidbodyProperties()
    {
        Rigidbody StructRigidbody = GetComponent<Rigidbody>();
        if (StructRigidbody != null)
        {
            switch (materialType)
            {
                case MaterialType.Wood:
                    SetRigidbodyProperties(StructRigidbody, mass: 8f, drag: 0.09f, angularDrag: 0.05f);
                    break;
                case MaterialType.Stone:
                    SetRigidbodyProperties(StructRigidbody, mass: 20f, drag: 0.28f, angularDrag: 1f);
                    break;
            }
        }
    }

    // Set properties for Rigidbody
    private void SetRigidbodyProperties(Rigidbody rb, float mass, float drag, float angularDrag)
    {
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
    }

    // Handle collision events and apply damage accordingly
    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        // Check if the game is in BUILD phase
        if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.GamePhase.BUILD)
        {
            // Do not process damage during the BUILD phase
            return;
        }

        // damage calculation
        float damage = CalculateDamage(collision);
        health -= damage;

        Debug.Log($"{gameObject.name} hit by {collision.gameObject.name} with damage: {damage}. Remaining Health: {health}");

        if (health <= 0f)
        {
            DestroyStructure();
        }
    }

    // Calculate the damage based on the collision momentum
    private float CalculateDamage(Collision collision)
    {
        if (collision.rigidbody == null) return 0f;

        float momentum = collision.rigidbody.mass * collision.relativeVelocity.magnitude;
        return momentum * damageFactor;
    }

    // Destroy the structure and generate debris
    private void DestroyStructure()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        // Attempt to slice the structure
        if (!AttemptToSlice())
        {
            // If slicing failed, destroy the object
            Debug.LogWarning($"Slicing failed for {gameObject.name}. Destroying object.");
            Destroy(gameObject);
        }
    }

    // Attempt to slice the structure into debris
    // Had to redo this because I did not like how initial debris could be really small, and the initial split was weird
    private bool AttemptToSlice()
    {
        // Determine the number of debris pieces
        int numberOfDebrisPieces = Random.Range(minDebrisPieces, maxDebrisPieces);

        // Initialize a list to keep track of the current pieces to be sliced
        List<GameObject> currentPieces = new List<GameObject> { gameObject };
        List<GameObject> newPieces = new List<GameObject>();

        int maxRetries = 200; // Maximum number of retry attempts for slicing
        int attempt = 0;

        while (currentPieces.Count < numberOfDebrisPieces && attempt < maxRetries)
        {
            attempt++;

            // Filter out pieces that are too small to slice
            List<GameObject> sliceablePieces = new List<GameObject>();
            foreach (var piece in currentPieces)
            {
                MeshFilter meshFilter = piece.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    Debug.LogError("MeshFilter or mesh is missing on piece. Cannot perform slicing.");
                    continue;
                }

                float pieceVolume = CalculateMeshVolume(meshFilter.sharedMesh);
                if (pieceVolume >= originalVolume * 0.15f) // Only include pieces that are >= 15% of the original volume
                {
                    sliceablePieces.Add(piece);
                }
            }

            // If no pieces are large enough to slice, break the loop
            if (sliceablePieces.Count == 0)
            {
                Debug.LogWarning($" attempt: {attempt} No more pieces large enough to slice.");
                continue;
            }

            // Choose a random piece from the sliceable pieces
            GameObject pieceToSlice = sliceablePieces[Random.Range(0, sliceablePieces.Count)];

            // Generate a random slicing plane within the piece bounds
            Vector3 sliceNormal = Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)) * Vector3.up;

            Renderer objRenderer = pieceToSlice.GetComponent<Renderer>();
            if (objRenderer == null)
            {
                Debug.LogError($"Renderer is missing on {pieceToSlice.name}. Cannot perform slicing.");
                currentPieces.Remove(pieceToSlice);
                continue;
            }

            Bounds bounds = objRenderer.bounds;
            Vector3 pointOnPlane = bounds.center + new Vector3(
                Random.Range(-bounds.extents.x * 0.5f, bounds.extents.x * 0.5f),
                Random.Range(-bounds.extents.y * 0.5f, bounds.extents.y * 0.5f),
                0f
            );

            // Perform the slicing operation using EzySlice
            SlicedHull slicedHull = pieceToSlice.Slice(pointOnPlane, sliceNormal, crossSectionMaterial);

            // If slicing was successful, add the new pieces
            if (slicedHull != null)
            {
                GameObject upperHull = slicedHull.CreateUpperHull(pieceToSlice, crossSectionMaterial);
                GameObject lowerHull = slicedHull.CreateLowerHull(pieceToSlice, crossSectionMaterial);

                // Set up the new debris pieces
                SetupDebrisPiece(upperHull);
                SetupDebrisPiece(lowerHull);

                // Add the new pieces to the lists
                newPieces.Add(upperHull);
                newPieces.Add(lowerHull);

                // Remove the sliced piece from the current pieces
                currentPieces.Remove(pieceToSlice);

                // Destroy the original piece
                Destroy(pieceToSlice);

                // Add the new pieces to the current pieces for further slicing
                currentPieces.Add(upperHull);
                currentPieces.Add(lowerHull);
            }
            else
            {
                Debug.LogWarning($"Slicing attempt {attempt} failed. Retrying...");
            }
        }

        // After slicing, destroy the original object if it still exists
        if (gameObject != null)
        {
            Destroy(gameObject);
        }

        return true;
    }

    // Setup a debris piece with necessary properties and components
    private void SetupDebrisPiece(GameObject debris)
    {
        if (debris == null) return;

        string materialName = materialType == MaterialType.Wood ? "Wood" : "Stone";
        string structureName = "";

        switch (structureType)
        {
            case StructureType.Wall:
                structureName = "Wall";
                break;
            case StructureType.Square:
                structureName = "Square";
                break;
            case StructureType.Slab:
                structureName = "Slab";
                break;
            case StructureType.RightTriangle:
                structureName = "RightTriangle";
                break;
            default:
                structureName = "Unknown";
                break;
        }
        debris.name = $"{materialName}_{structureName}_Debris_Level_1";

        AdjustDebrisPosition(debris);
        AddCollider(debris);
        Rigidbody debrisRigidbody = AddRigidbody(debris);

        // Calculate the mass based on the volume ratio
        MeshFilter debrisMeshFilter = debris.GetComponent<MeshFilter>();
        if (debrisMeshFilter != null && debrisMeshFilter.sharedMesh != null)
        {
            float debrisVolume = CalculateMeshVolume(debrisMeshFilter.sharedMesh);
            float volumeRatio = debrisVolume / originalVolume;
            float debrisMass = originalMass * volumeRatio;
            SetDebrisRigidbodyProperties(debrisRigidbody, debrisMass);
        }
        else
        {
            Debug.LogError("Debris piece does not have a valid MeshFilter or mesh.");
            Destroy(debris);
            return;
        }

        AssignMaterial(debris);
        AddDebrisPieceComponent(debris);
        ApplyExplosionForce(debrisRigidbody);
    }

    // Slightly adjust the debris position to prevent possible overlapping
    private void AdjustDebrisPosition(GameObject debris)
    {
        debris.transform.position += new Vector3(
            Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f),
            0f
        );
    }

    // Add a MeshCollider to the debris piece and set it up
    private void AddCollider(GameObject debris)
    {
        MeshFilter debrisMeshFilter = debris.GetComponent<MeshFilter>();
        if (debrisMeshFilter == null || debrisMeshFilter.sharedMesh == null)
        {
            Debug.LogError("Debris piece does not have a valid MeshFilter or mesh.");
            Destroy(debris);
            return;
        }

        MeshCollider debrisCollider = debris.AddComponent<MeshCollider>();
        debrisCollider.sharedMesh = debrisMeshFilter.sharedMesh;
        debrisCollider.convex = true;
    }

    // Add a Rigidbody to the debris piece and configure it
    private Rigidbody AddRigidbody(GameObject debris)
    {
        Rigidbody rb = debris.AddComponent<Rigidbody>();
        return rb;
    }

    // Set Rigidbody properties for the debris pieces
    private void SetDebrisRigidbodyProperties(Rigidbody rb, float mass)
    {
        if (rb == null) return;

        // Set mass based on calculated value
        rb.mass = mass;

        // Set drag and angular drag based on material type
        switch (materialType)
        {
            case MaterialType.Wood:
                rb.drag = 0.2f;
                rb.angularDrag = 0.05f;
                break;
            case MaterialType.Stone:
                rb.drag = 0.25f;
                rb.angularDrag = 1f;
                break;
        }
        // Freeze movement and rotation for the StructureTypes
        // Due to how I made the triangle, it has to be slightly different
        switch (structureType)
        {
            case StructureType.RightTriangle:
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                break;
            default:
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
                break;
        }

}

    // Assign appropriate material to the debris piece
    private void AssignMaterial(GameObject debris)
    {
        MeshRenderer debrisRenderer = debris.GetComponent<MeshRenderer>();
        if (debrisRenderer != null)
        {
            debrisRenderer.material = materialType == MaterialType.Wood ? woodMaterial : stoneMaterial;
        }
    }

    // Add a DebrisPiece component to each debris and configure its properties
    private void AddDebrisPieceComponent(GameObject debris)
    {
        DebrisPiece debrisPiece = debris.AddComponent<DebrisPiece>();
        debrisPiece.baseDebrisHealth = baseDebrisHealth;
        debrisPiece.splitLevel = 1;
        debrisPiece.maxSplitLevel = maxSplitLevel;
        debrisPiece.healthMultiplier = debrisHealthMultiplier;
        debrisPiece.materialType = materialType;
        debrisPiece.structureType = structureType;
        debrisPiece.damageFactor = damageFactor;
        debrisPiece.crossSectionMaterial = crossSectionMaterial;
        debrisPiece.woodMaterial = woodMaterial;
        debrisPiece.stoneMaterial = stoneMaterial;
        debrisPiece.explosionForce = explosionForce;
    }

    // Apply an explosion force to the debris piece
    // Kinda buggy at high-ish values
    private void ApplyExplosionForce(Rigidbody rb)
    {
        Vector3 explosionDirection = (rb.transform.position - transform.position).normalized;
        rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
    }

    // Calculate the volume of a mesh
    private float CalculateMeshVolume(Mesh mesh)
    {
        float volume = 0f;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Transform vertices to world space
        Matrix4x4 localToWorld = transform.localToWorldMatrix;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = localToWorld.MultiplyPoint3x4(vertices[triangles[i]]);
            Vector3 p2 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
            Vector3 p3 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);

            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }

        return Mathf.Abs(volume);
    }

    // Calculate the signed volume of a triangle
    private float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Vector3.Dot(Vector3.Cross(p1, p2), p3) / 6f;
    }

    public void SetMaterialType(MaterialType newMaterialType)
    {
        materialType = newMaterialType;
        SetMaterialDependentProperties();
    }
}
