using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/* 
 * 
 * Author: Parker Clark
 */


/* 
 * Evan Pagani CS347 Project: Under Siege!
 * Added polish to Parker's original implementations
 * Added deleteZone implementation, deleteZone alpha highlight, and budget
 */
public class DragAndDropManager : MonoBehaviour
{
    public GameObject wallPrefab = null;
    public GameObject slabPrefab = null;
    public GameObject squarePrefab = null;
    public GameObject trianglePrefab = null;
    private GameObject currentDraggedObject = null;
    private Camera mainCamera = null;
    private Vector3 mouseOffset = Vector3.zero;

    public LayerMask deleteZoneLayer; // Layer mask for delete zones

    private BudgetManager budgetManager;

    private List<Renderer> deleteZoneRenderers = new List<Renderer>();
    private List<Collider> deleteZoneColliders = new List<Collider>();
    private List<Material> deleteZoneMaterials = new List<Material>();
    private List<Color> deleteZoneInitialColors = new List<Color>();
    private List<bool> isDeleteZoneHighlighted = new List<bool>();

    private void Start()
    {
        mainCamera = Camera.main;

        budgetManager = FindObjectOfType<BudgetManager>();
        if (budgetManager == null)
        {
            Debug.LogError("BudgetManager not found in the scene!");
        }

        AddEventTrigger(GameObject.FindWithTag("Button_Wall"), wallPrefab);
        AddEventTrigger(GameObject.FindWithTag("Button_Slab"), slabPrefab);
        AddEventTrigger(GameObject.FindWithTag("Button_Square"), squarePrefab);
        AddEventTrigger(GameObject.FindWithTag("Button_Triangle"), trianglePrefab);

        TMP_Dropdown materialDropdown = GameObject.FindWithTag("Dropdown_Material").GetComponent<TMP_Dropdown>();
        if (materialDropdown != null)
        {
            materialDropdown.onValueChanged.AddListener(OnMaterialChanged);
        }

        // Ensure the deleteZoneLayer is set
        if (deleteZoneLayer == 0)
        {
            deleteZoneLayer = LayerMask.GetMask("DeleteZone");
        }

        // Find all delete zones in the scene on the DeleteZone layer
        GameObject[] deleteZoneObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in deleteZoneObjects)
        {
            if (((1 << obj.layer) & deleteZoneLayer) != 0)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                Collider collider = obj.GetComponent<Collider>();
                if (renderer != null && collider != null)
                {
                    renderer.enabled = false; // Initially disable renderer
                    deleteZoneRenderers.Add(renderer);
                    deleteZoneColliders.Add(collider);

                    Material material = renderer.material; // Creates an instance of the material
                    deleteZoneMaterials.Add(material);
                    deleteZoneInitialColors.Add(material.color);

                    isDeleteZoneHighlighted.Add(false);
                }
            }
        }
    }

    private void AddEventTrigger(GameObject button, GameObject prefab)
    {
        if (button == null || prefab == null) return;

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data, prefab); });
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        trigger.triggers.Add(pointerUpEntry);
    }

    private void OnPointerDown(PointerEventData data, GameObject prefab)
    {
        // Check if the budget is exceeded
        if (budgetManager != null && budgetManager.isBudgetExceeded)
        {
            Debug.Log("Cannot place new pieces. Budget is exceeded.");
            return;
        }

        if (currentDraggedObject != null)
        {
            Destroy(currentDraggedObject);
        }

        currentDraggedObject = Instantiate(prefab);
        Vector3 spawnPosition = GetMouseWorldPosition();

        // Get the damageCal component to check the structure type
        damageCal damageComponent = currentDraggedObject.GetComponent<damageCal>();
        if (damageComponent != null)
        {
            // Set z position based on structure type
            if (damageComponent.structureType == StructureType.RightTriangle)
            {
                spawnPosition.z = 0.5f; // Set z to 0.5 for triangles; they spawn weird
            }
            else
            {
                spawnPosition.z = 0f; // Default z position for other structures
            }

            damageComponent.SetMaterialType(GlobalMaterialManager.CurrentMaterialType);
            Debug.Log("Material type set to: " + GlobalMaterialManager.CurrentMaterialType);
        }
        else
        {
            Debug.LogError("damageCal component not found on the instantiated object!");
        }

        currentDraggedObject.transform.position = spawnPosition;

        Rigidbody rb = currentDraggedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider collider = currentDraggedObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        mouseOffset = currentDraggedObject.transform.position - GetMouseWorldPosition();

        // Enable delete zones
        SetDeleteZonesVisibility(true);
    }

    private void OnPointerUp(PointerEventData data)
    {


        if (budgetManager != null && budgetManager.isBudgetExceeded)
        {
            Debug.Log("Cannot place new pieces. Budget is exceeded.");
            Destroy(currentDraggedObject);
            return;
        }

        if (currentDraggedObject != null)
        {
            // Check if the object is over a delete zone
            if (IsOverAnyDeleteZone(currentDraggedObject))
            {
                Destroy(currentDraggedObject);
                currentDraggedObject = null;

                // Disable delete zones
                SetDeleteZonesVisibility(false);

                return;
            }

            Rigidbody rb = currentDraggedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            Collider collider = currentDraggedObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            currentDraggedObject = null;

            // Disable delete zones
            SetDeleteZonesVisibility(false);
        }
    }

    private bool IsOverAnyDeleteZone(GameObject obj)
    {
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider == null)
            return false;

        foreach (Collider deleteZoneCollider in deleteZoneColliders)
        {
            if (objCollider.bounds.Intersects(deleteZoneCollider.bounds))
            {
                return true;
            }
        }

        return false;
    }

    private void WakeUpAllRigidbodies()
    {
        Rigidbody[] allRigidbodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.WakeUp();
        }
    }

    private void Update()
    {
        if (currentDraggedObject != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition() + mouseOffset;
            currentDraggedObject.transform.position = mousePosition;

            Collider objCollider = currentDraggedObject.GetComponent<Collider>();
            if (objCollider != null)
            {
                for (int i = 0; i < deleteZoneColliders.Count; i++)
                {
                    Collider deleteZoneCollider = deleteZoneColliders[i];
                    Material deleteZoneMaterial = deleteZoneMaterials[i];
                    Color initialColor = deleteZoneInitialColors[i];

                    bool isOverDeleteZone = objCollider.bounds.Intersects(deleteZoneCollider.bounds);

                    if (isOverDeleteZone)
                    {
                        if (!isDeleteZoneHighlighted[i])
                        {
                            // Increase the alpha of the delete zone's material
                            SetDeleteZoneAlpha(deleteZoneMaterial, 0.25f); // Highlight Alpha; currently set to %25
                            isDeleteZoneHighlighted[i] = true;
                        }
                    }
                    else
                    {
                        if (isDeleteZoneHighlighted[i])
                        {
                            // Reset the alpha of the delete zone's material
                            SetDeleteZoneAlpha(deleteZoneMaterial, initialColor.a);
                            isDeleteZoneHighlighted[i] = false;
                        }
                    }
                }
            }
        }
       
        // Handle picking up existing objects
        if (Input.GetMouseButtonDown(0))
        {
            if (currentDraggedObject == null)
            {
                // Check if the game is in ATTACK phase
                if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.GamePhase.ATTACK)
                {
                    // Do not allow draggable objects
                    return;
                }

                    // Perform a raycast into the scene
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                int layerMask = ~deleteZoneLayer.value; // Inverts to exclude the delete zone layer
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    // Check if the object has the damageCal component
                    damageCal damageComponent = hit.collider.GetComponent<damageCal>();
                        if (damageComponent != null)
                        {
                            // Start dragging this object
                            currentDraggedObject = hit.collider.gameObject;

                            // Disable physics and collider
                            Rigidbody rb = currentDraggedObject.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = true;
                                WakeUpAllRigidbodies();
                            }

                            Collider collider = currentDraggedObject.GetComponent<Collider>();
                            if (collider != null)
                            {
                                collider.enabled = false;
                            }

                            mouseOffset = currentDraggedObject.transform.position - GetMouseWorldPosition();

                            // Enable delete zones
                            SetDeleteZonesVisibility(true);
                        }
                    }
                
            }
        }

        // Handle releasing the object
        if (Input.GetMouseButtonUp(0) && currentDraggedObject != null)
        {
            // Check if the object is over a delete zone
            if (IsOverAnyDeleteZone(currentDraggedObject))
            {
                Destroy(currentDraggedObject);
                currentDraggedObject = null;

                // Disable delete zones
                SetDeleteZonesVisibility(false);

                return;
            }

            // Reenable physics and collider
            Rigidbody rb = currentDraggedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            Collider collider = currentDraggedObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            currentDraggedObject = null;

            // Disable delete zones
            SetDeleteZonesVisibility(false);
        }
    }

    private void SetDeleteZonesVisibility(bool visible)
    {
        for (int i = 0; i < deleteZoneRenderers.Count; i++)
        {
            Renderer renderer = deleteZoneRenderers[i];
            Material material = deleteZoneMaterials[i];
            Color initialColor = deleteZoneInitialColors[i];

            if (renderer != null)
            {
                renderer.enabled = visible;
                // Reset the alpha when visibility changes
                if (!visible)
                {
                    SetDeleteZoneAlpha(material, initialColor.a);
                    isDeleteZoneHighlighted[i] = false;
                }
            }
        }
    }

    private void SetDeleteZoneAlpha(Material material, float alpha)
    {
        if (material != null)
        {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    private void OnMaterialChanged(int index)
    {
        GlobalMaterialManager.CurrentMaterialType = (MaterialType)index;
        Debug.Log("Material changed to: " + GlobalMaterialManager.CurrentMaterialType);
    }
}
