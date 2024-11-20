using UnityEngine;
using System.Collections.Generic;
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
    Wall,  // Structure type Wall
    Square, // Structure type Square
    Slab // Structure type Slab
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
    public int minDebrisPieces = 2; // Minimum number of debris pieces generated upon destruction
    public int maxDebrisPieces = 5; // Maximum number of debris pieces generated upon destruction
    public float explosionForce = 30f; // Force applied to debris explosion

    [Header("Debris Health Settings")]
    public float baseDebrisHealth = 50f; // Base health value for debris pieces
    public float debrisHealthMultiplier = 1.5f; // Multiplier for health of debris pieces
    public int maxSplitLevel = 5; // Maximum level of splitting for debris pieces

    private MeshFilter meshFilter; // Reference to the MeshFilter component
    private MeshRenderer meshRenderer; // Reference to the MeshRenderer component
    private bool isDestroyed = false; // Flag to check if the structure is already destroyed

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateMaterialProperties();
        SetName(); // Set the initial name based on material type and structure type
    }

    // Called when properties are modified in the Inspector
    private void OnValidate()
    {
        UpdateMaterialProperties();
        SetName(); // Update the name whenever the material type or structure type is changed
    }

    // Set the name of the object based on its material type and structure type
    private void SetName()
    {
        string materialName = materialType == MaterialType.Wood ? "Wood" : "Stone";
        string structureName = structureType == StructureType.Wall ? "Wall" :
                               structureType == StructureType.Square ? "Square" : "Slab";
        gameObject.name = $"{materialName}_{structureName}";
    }

    // Update the material properties based on the selected material type
    public void UpdateMaterialProperties()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        switch (materialType)
        {
            case MaterialType.Wood:
                meshRenderer.material = woodMaterial;
                break;
            case MaterialType.Stone:
                meshRenderer.material = stoneMaterial;
                break;
        }

        UpdateRigidbodyProperties();
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
                    SetRigidbodyProperties(StructRigidbody, mass: 2f, drag: 0.1f, angularDrag: 0.05f);
                    break;
                case MaterialType.Stone:
                    SetRigidbodyProperties(StructRigidbody, mass: 5f, drag: 0.1f, angularDrag: 1f);
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

        int debrisPieces = Random.Range(minDebrisPieces, maxDebrisPieces + 1);

        DisableStructureComponents();
        GenerateDebrisWithEzySlice(debrisPieces);
    }

    // Disable the components of the structure before destroying it
    private void DisableStructureComponents()
    {
        meshRenderer.enabled = false;

        Collider structureCollider = GetComponent<Collider>();
        if (structureCollider != null) structureCollider.enabled = false;

        Rigidbody structureRigidbody = GetComponent<Rigidbody>();
        if (structureRigidbody != null)
        {
            structureRigidbody.isKinematic = true;
            structureRigidbody.detectCollisions = false;
        }
    }

    // Generate debris pieces by slicing the original structure
    private void GenerateDebrisWithEzySlice(int targetDebrisPieces)
    {
        List<GameObject> objectsToSlice = new List<GameObject> { gameObject };
        List<GameObject> finalDebrisPieces = new List<GameObject>();

        while (objectsToSlice.Count > 0 && finalDebrisPieces.Count + objectsToSlice.Count < targetDebrisPieces)
        {
            List<GameObject> newObjectsToSlice = new List<GameObject>();

            foreach (GameObject obj in objectsToSlice)
            {
                if (finalDebrisPieces.Count + newObjectsToSlice.Count >= targetDebrisPieces)
                {
                    finalDebrisPieces.Add(obj);
                    continue;
                }

                if (!SliceObject(obj, newObjectsToSlice))
                {
                    finalDebrisPieces.Add(obj);
                }
            }

            objectsToSlice = newObjectsToSlice;
        }

        finalDebrisPieces.AddRange(objectsToSlice);
    }

    // Slice an object into pieces and add them to the debris list
    private bool SliceObject(GameObject obj, List<GameObject> newObjectsToSlice)
    {
        Vector3 sliceNormal = Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)) * Vector3.up;
        Vector3 pointOnPlane = obj.transform.position + new Vector3(
            Random.Range(-obj.transform.localScale.x * 0.5f, obj.transform.localScale.x * 0.5f),
            Random.Range(-obj.transform.localScale.y * 0.5f, obj.transform.localScale.y * 0.5f),
            0f
        );

        SlicedHull slicedHull = obj.Slice(pointOnPlane, sliceNormal, crossSectionMaterial);

        if (slicedHull != null)
        {
            SetupDebrisPiece(slicedHull.CreateUpperHull(obj, crossSectionMaterial), newObjectsToSlice);
            SetupDebrisPiece(slicedHull.CreateLowerHull(obj, crossSectionMaterial), newObjectsToSlice);
            Destroy(obj);
            return true;
        }
        return false;
    }

    // Setup a debris piece with necessary properties and components
    private void SetupDebrisPiece(GameObject debris, List<GameObject> debrisList)
    {
        string materialName = materialType == MaterialType.Wood ? "Wood" : "Stone";
        string structureName = structureType == StructureType.Wall ? "Wall" :
                               structureType == StructureType.Square ? "Square" : "Slab";
        debris.name = $"{materialName}_{structureName}_Debris_Level_1";

        debris.transform.position += new Vector3(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f), 0f);

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

        Rigidbody debrisRigidbody = debris.AddComponent<Rigidbody>();
        SetDebrisRigidbodyProperties(debrisRigidbody);

        AssignMaterialToDebris(debris);

        AddDebrisPieceComponent(debris);
        debrisList.Add(debris);
    }

    // Set Rigidbody properties for the debris pieces
    private void SetDebrisRigidbodyProperties(Rigidbody rb)
    {
        if (rb == null) return;

        switch (materialType)
        {
            case MaterialType.Wood:
                SetRigidbodyProperties(rb, mass: 2f, drag: 0.1f, angularDrag: 0.05f);
                break;
            case MaterialType.Stone:
                SetRigidbodyProperties(rb, mass: 5f, drag: 0.1f, angularDrag: 1f);
                break;
        }

        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        rb.AddForce((rb.transform.position - transform.position).normalized * explosionForce, ForceMode.Impulse);
    }

    // Assign appropriate material to the debris piece
    private void AssignMaterialToDebris(GameObject debris)
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

    public void SetMaterialType(MaterialType newMaterialType)
{
    materialType = newMaterialType;
    UpdateMaterialProperties();
}
    
}
