using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialDropdownHandler : MonoBehaviour
{
    /*
    * A script that handles the Dropdown UI component for selecting materials.
    * Will update the GlobalMaterialManager with the selected material type.
    *
    * Author: Parker Clark
    */

    [SerializeField] private TMP_Dropdown materialDropdown; // Reference to the Dropdown UI component

    private void Start()
    {
        // Add listener for value changes
        materialDropdown.onValueChanged.AddListener(OnMaterialSelected);
    }

    private void OnMaterialSelected(int index)
    {
        // Map selected dropdown index to the MaterialType enum
        MaterialType selectedMaterial = (MaterialType)index;
        GlobalMaterialManager.CurrentMaterialType = selectedMaterial;

        Debug.Log($"Material changed to: {selectedMaterial}");
    }

    private void OnDestroy()
    {
        // Clean up the listener when the object is destroyed
        materialDropdown.onValueChanged.RemoveListener(OnMaterialSelected);
    }
}
