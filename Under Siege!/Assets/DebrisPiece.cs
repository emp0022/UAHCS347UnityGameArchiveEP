using UnityEngine;
using System.Collections.Generic;
using EzySlice;

/*
 * Evan Pagani CS347 Project: Under Siege!
 * This script handles the behavior of debris pieces that are generated after a structure is destroyed. 
 * Each debris piece has health, can take damage, and can potentially split into smaller pieces upon destruction, depending on the varibles listed below. 
 * The script also manages the physical properties, materials, and naming assignment for each debris piece. 
 * It uses EzySlice to split the debris into smaller parts, attempts multiple slicing retries, and applies explosion forces to simulate destruction. Thank you EzySlice ! ! !
 */
public class DebrisPiece : MonoBehaviour
{
    public float health; // Current health of the debris piece
    public int splitLevel = 1; // The current level of splitting for this debris piece
    public int maxSplitLevel = 5; // Maximum number of times debris can split
    public float healthMultiplier = 1.5f; // Multiplier for health based on split level
    public float baseDebrisHealth = 50f; // Base health value for each debris piece
    public StructureType structureType; // The type of structure the debris came from (Wall, Square, or Slab)
    public MaterialType materialType; // The material type of the debris (currently Wood or Stone)
    public float damageFactor; // Multiplier for damage calculation
    public Material crossSectionMaterial; // Material for internal cross sections (debug stuff)
    public Material woodMaterial; // Material for wood structures
    public Material stoneMaterial; // Material for stone structures
    public float explosionForce; // Force applied to debris explosion

    private bool isDestroyed = false; // Flag to determine if the debris is already destroyed

    private void Start()
    {
        InitializeHealth(); // Initialize the health of the debris based on its split level
    }

    // Initialize the health of the debris
    private void InitializeHealth()
    {
        // Calculate initial health based on split level and health multiplier
        health = baseDebrisHealth * Mathf.Pow(healthMultiplier, splitLevel - 1); // An increase in split level raises the healthMultiplier exponentially
    }

    // Handle collision events and apply damage to the debris
    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        float damage = CalculateDamage(collision);
        health -= damage;

        Debug.Log($"{gameObject.name} hit by {collision.gameObject.name}. Damage: {damage}. Remaining Health: {health}");

        // Check if the debris health falls below zero
        if (health <= 0f)
        {
            if (splitLevel < maxSplitLevel)
            {
                Debug.Log("Attempting to split debris...");
                SplitDebris();
            }
            else
            {
                Debug.Log("Max split level reached. Damage threshold reached. Destroying debris.");
                Destroy(gameObject);
            }
        }
    }

    // Calculate the damage based on the collision momentum
    private float CalculateDamage(Collision collision)
    {
        if (collision.rigidbody == null) return 0f;

        float momentum = collision.rigidbody.mass * collision.relativeVelocity.magnitude;
        return momentum * damageFactor;
    }

    // Split the debris into smaller pieces
    private void SplitDebris()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        // Delete if the debris slice could not be generated within the amount of attempts given
        if (!AttemptToSliceDebris())
        {
            Debug.LogWarning("Slicing failed after retries. Destroying debris.");
            Destroy(gameObject);
        }
    }

    // Attempt to slice the debris piece into smaller pieces
    private bool AttemptToSliceDebris()
    {
        int maxRetries = 200; // Maximum number of retry attempts for slicing
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // Generate a random slicing plane within the debris bounds
            Vector3 sliceNormal = Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)) * Vector3.up;
            Vector3 pointOnPlane = transform.position + new Vector3(
                Random.Range(-transform.localScale.x * 0.5f, transform.localScale.x * 0.5f),
                Random.Range(-transform.localScale.y * 0.5f, transform.localScale.y * 0.5f),
                0f
            );

            // Perform the slicing operation using EzySlice
            SlicedHull slicedHull = gameObject.Slice(pointOnPlane, sliceNormal, crossSectionMaterial);

            // If slicing was successful, create new debris pieces
            if (slicedHull != null)
            {
                Debug.Log("Slicing successful! Creating new debris pieces.");
                SetupDebrisPiece(slicedHull.CreateUpperHull(gameObject, crossSectionMaterial));
                SetupDebrisPiece(slicedHull.CreateLowerHull(gameObject, crossSectionMaterial));
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.LogWarning($"Slicing attempt {attempt + 1} failed. Retrying...");
            }
        }

        // If all slicing attempts failed
        Debug.LogWarning("All slicing attempts failed.");
        return false;
    }

    // Set up a debris piece with necessary properties and components
    private void SetupDebrisPiece(GameObject debris)
    {
        if (debris == null) return;

        // Set the name of the debris based on material type, structure type, and split level
        string materialName = materialType == MaterialType.Wood ? "Wood" : "Stone";
        string structureName = structureType == StructureType.Wall ? "Wall" :
                               structureType == StructureType.Square ? "Square" : "Slab";
        debris.name = $"{materialName}_{structureName}_Debris_Level_{splitLevel + 1}";

        AdjustDebrisPosition(debris);
        AddCollider(debris);
        Rigidbody debrisRigidbody = AddRigidbody(debris);
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

        if (materialType == MaterialType.Wood)
        {
            SetRigidbodyProperties(rb, mass: 2f, drag: 0.1f, angularDrag: 0.15f);
        }
        else if (materialType == MaterialType.Stone)
        {
            SetRigidbodyProperties(rb, mass: 10f, drag: 0.1f, angularDrag: 1f);
        }

        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY; // lock to 2d grid
        return rb;
    }

    // Set properties for Rigidbody
    private void SetRigidbodyProperties(Rigidbody rb, float mass, float drag, float angularDrag)
    {
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
    }

    // Assign appropriate material to the debris piece
    private void AssignMaterial(GameObject debris)
    {
        MeshRenderer renderer = debris.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = materialType == MaterialType.Wood ? woodMaterial : stoneMaterial;
        }
    }

    // Add a DebrisPiece component to each debris and configure its properties
    private void AddDebrisPieceComponent(GameObject debris)
    {
        DebrisPiece newDebrisPiece = debris.AddComponent<DebrisPiece>();
        newDebrisPiece.baseDebrisHealth = baseDebrisHealth;
        newDebrisPiece.splitLevel = splitLevel + 1;
        newDebrisPiece.maxSplitLevel = maxSplitLevel;
        newDebrisPiece.healthMultiplier = healthMultiplier;
        newDebrisPiece.materialType = materialType;
        newDebrisPiece.structureType = structureType;
        newDebrisPiece.damageFactor = damageFactor;
        newDebrisPiece.crossSectionMaterial = crossSectionMaterial;
        newDebrisPiece.woodMaterial = woodMaterial;
        newDebrisPiece.stoneMaterial = stoneMaterial;
        newDebrisPiece.explosionForce = explosionForce;
    }

    // Apply an explosion force to the debris piece
    private void ApplyExplosionForce(Rigidbody rb)
    {
        Vector3 explosionDirection = (rb.transform.position - transform.position).normalized;
        explosionDirection.z = 0;

        rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
    }
}
