using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* 
 * Evan Pagani CS347 Project: Under Siege!
 * Completely redid Cole's initial implementation of this
 * Implements the budget mechanic of the game
 */

public class BudgetManager : MonoBehaviour
{
    public float BudgetMax = 2000f;
    private float BudgetCurrent = 0f;
    public TextMeshProUGUI budgetText;
    public bool isBudgetExceeded = false;

    void Update()
    {
        // Check if the game is in ATTACK phase
        if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.GamePhase.ATTACK)
        {
            // Do not process budget during the ATTACK phase
            return;
        }
        CalculateCurrentBudget();
        UpdateBudgetDisplay();
        CompareBudget();
    }

    void CalculateCurrentBudget()
    {
        BudgetCurrent = 0f; // Reset the current budget before calculation

        // Find all game objects with the "BaseComponent" tag
        GameObject[] baseComponents = GameObject.FindGameObjectsWithTag("BaseComponent");

        // Loop through each object to sum up the costs
        foreach (GameObject piece in baseComponents)
        {
            damageCal damageCalComponent = piece.GetComponent<damageCal>();
            if (damageCalComponent != null)
            {
                BudgetCurrent += damageCalComponent.cost;
            }
        }
    }

    void UpdateBudgetDisplay()
    {

        if (budgetText != null)
        {
            budgetText.text = $"Budget: ${BudgetCurrent} / ${BudgetMax}";
        }
    }

    void CompareBudget()
    {
        isBudgetExceeded = BudgetCurrent > BudgetMax;
    }
}
