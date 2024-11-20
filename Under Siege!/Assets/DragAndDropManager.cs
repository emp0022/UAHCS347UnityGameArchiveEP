using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DragAndDropManager : MonoBehaviour
{
    /*
    * A script for the drag and drop functionality/placing building objects.
    * This script handles the instantiation of building objects, setting their material type, and placing them in the scene.
    * It also listens for changes to the material dropdown and updates the current materiat using the GlobalMaterialManager.
    *
    * Author: Parker Clark
    */

    // Prefab objects for spawning walls, slabs, and squares
    public GameObject wallPrefab;
    public GameObject slabPrefab;
    public GameObject squarePrefab;

    // Reference to the currently dragged object
    private GameObject currentDraggedObject;

    // Main camera to use for dragging math
    private Camera mainCamera;

    // Maintain a mouse offset for dragging objects
    private Vector3 mouseOffset;

    private void Start()
    { 
        /*
        * Setup operations before first frame update.
        */

        // Get the main camera
        mainCamera = Camera.main;

        // Add event listeners to buttons, passing in the prefab to instantiate
        // There is a better way to do this (e.g., using a dictionary), but im tired and this works
        AddEventTrigger(GameObject.FindWithTag("Button_Wall"), wallPrefab);
        AddEventTrigger(GameObject.FindWithTag("Button_Slab"), slabPrefab);
        AddEventTrigger(GameObject.FindWithTag("Button_Square"), squarePrefab);

        // Add event listener to the material dropdown
        TMP_Dropdown materialDropdown = GameObject.FindWithTag("Dropdown_Material").GetComponent<TMP_Dropdown>();
        materialDropdown.onValueChanged.AddListener(OnMaterialChanged);
    }

    private void AddEventTrigger(GameObject button, GameObject prefab)
    { 
        /*
        * Add an EventTrigger component to a button and register PointerDown and PointerUp events.
        * This function is used to add drag and drop functionality to the building buttons.
        *
        * Parameters:
        * button: The button to add the EventTrigger to
        * prefab: The prefab to instantiate when the button is clicked
        */

        // Add an EventTrigger component to the button if it doesn't already have one
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.AddComponent<EventTrigger>();
        }

        // Register the PointerDown and make a new event entry, pass in prefab to spawn
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data, prefab); });
        trigger.triggers.Add(pointerDownEntry);

        // Register the PointerDown and make a new event entry
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        trigger.triggers.Add(pointerUpEntry);
    }

    private void OnPointerDown(PointerEventData data, GameObject prefab)
    {   
        /*
        * Instantiate a building object when a button is clicked and start dragging it.
        * Assumes the user clicked and is holding left click.
        *
        * Parameters:
        * data: The PointerEventData from the event trigger
        * prefab: The prefab to instantiate
        */

        // Destroy the current dragged object if it exists
        if (currentDraggedObject != null)
        {
            Destroy(currentDraggedObject);
        }

        // Spawn the object and set to currentDraggedObject
        currentDraggedObject = Instantiate(prefab);

        // Set the object's position to the mouse position
        // Have to set Z to 0, otherwise the prefab spawns behind the background
        Vector3 spawnPosition = GetMouseWorldPosition();
        spawnPosition.z = 0; // Set Z position to 0 for 2D
        currentDraggedObject.transform.position = spawnPosition;

        // Disable physics interactions while dragging
        Rigidbody rb = currentDraggedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Set the damage component material type
        damageCal damageComponent = currentDraggedObject.GetComponent<damageCal>();
        if (damageComponent != null)
        {
            damageComponent.SetMaterialType(GlobalMaterialManager.CurrentMaterialType);
            Debug.Log("Material type set to: " + GlobalMaterialManager.CurrentMaterialType);
        }
        else
        {
            Debug.LogError("damageCal component not found on the instantiated object!");
        }

        // Calculate the mouse offset for dragging
        mouseOffset = currentDraggedObject.transform.position - GetMouseWorldPosition();
    }

    private void OnPointerUp(PointerEventData data)
    {
        /*
        * Stop dragging the object and place it in the scene. Re-enable physics interactions.
        * Assumes the user is no longer holding left click.
        *
        * Parameters:
        * data: The PointerEventData from the event trigger
        */

        // Set the current dragged object to null if it exists
        if (currentDraggedObject != null)
        {   
            // Re-enable physics interactions when placed
            Rigidbody rb = currentDraggedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            currentDraggedObject = null;
        }
    }

    private void Update()
    {   
        /*
        * Update is called once per frame. Used to update the position of the dragged object.
        */

        // Update the position of the dragged object if it exists
        if (currentDraggedObject != null)
        {   
            // Once again, set Z to 0 for 2D, unless you want the object to magically disappear
            Vector3 mousePosition = GetMouseWorldPosition() + mouseOffset;
            mousePosition.z = 0;
            currentDraggedObject.transform.position = mousePosition;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {   
        /*
        * Get the mouse position in world space.
        *
        * Returns:
        * The mouse position in world space
        */

        // Get the mouse position and set Z to near clip plane
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    private void OnMaterialChanged(int index)
    {   
        /*
        * Event listener for the material dropdown. Updates the current material type using the GlobalMaterialManager.
        *
        * Parameters:
        * index: The index of the selected dropdown item
        */
        
        GlobalMaterialManager.CurrentMaterialType = (MaterialType)index;
        Debug.Log("Material changed to: " + GlobalMaterialManager.CurrentMaterialType);
    }
}