using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{   
    /*
    * A script for goal logic. Defines color changing logic on damage. Triggers a lose condition when health reaches 0.
    *
    * Author: Parker Clark
    */


    // Max health of the goal
    public int maxHealth = 1000;

    // Current health of the goal
    private int currentHealth; 

    // Components controls for the goal
    private SpriteRenderer spriteRenderer;
    private Rigidbody rb;

    // Flag to determine if the goal has landed
    private bool hasLanded = false;

    private void Start()
    {
        /* 
        * Initialize the goal's health and components.
        */

        // Initialize health and components
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();

        // Ensure the goal starts stationary (build phase)
        rb.isKinematic = true;

        // Set initial position to play nice with a 2D plane
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        // Set initial color
        UpdateColor();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }


    private void OnCollisionEnter(Collision collision)
    {
        /*
        * Handle collisions with the goal.
        *
        * Parameters:
        * collision: The collision data
        */

        // Check if the goal collides with the platform or base component
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("BaseComponent"))
        {
            // Check if the game is in BUILD phase
            if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.GamePhase.BUILD)
            {
                // Do not process damage during the BUILD phase
                return;
            }
            // Ensure the goal stays in front of the platform
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);

            // Debug statement to confirm collision
            Debug.Log("Goal collided with the platform or base component.");

            // If the goal has not landed yet, set hasLanded to true and ignore damage
            if (!hasLanded)
            {
                hasLanded = true;
                return;
            }
        }

        // Apply damage if the goal has already landed
        float damage = CalculateDamage(collision);
        TakeDamage((int)damage);
    }

    private float CalculateDamage(Collision collision)
    {
        /*
        * Calculate the damage to apply to the goal based on the collision.
        *
        * Parameters:
        * collision: The collision data
        *
        * Returns:
        * The amount of damage to apply
        *
        * Note: Stole some of this from damageCal.cs because im lazy. Thanks Evan.
        */

        // Check if the collision has a rigidbody
        if (collision.rigidbody == null) return 0f;

        // Calculate the momentum of the collision
        float momentum = collision.rigidbody.mass * collision.relativeVelocity.magnitude;
        return momentum;
    }

    public void ActivateGravity()
    {
        /*
        * Enable gravity on the goal to allow physics to act on it.
        */

        rb.isKinematic = false;  // Allow physics to act on the goal
        rb.useGravity = true;     // Enable gravity
        rb.velocity = Vector3.zero; // Reset velocity
    }


    public void TakeDamage(int damage)
    {
        /*
        * Apply damage to the goal.
        *
        * Parameters:
        * damage: The amount of damage to apply
        */
        Debug.Log("Damage applied: " + damage);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Goal health: " + currentHealth);

        // Update the goal's appearance based on health
        UpdateColor();

        // Check if the goal is destroyed
        if (currentHealth <= 0)
        {
            OnGoalDestroyed();
        }
    }


    private void UpdateColor()
    {
        /*
        * Update the color of the goal based on its health.
        *
        * Green: 100-50% health
        * Yellow: 50-25% health
        * Red: 25-0% health
        */

        // Calculate the health percentage (0 to 1)
        float healthPercentage = (float)currentHealth / maxHealth;

        // Interpolate between red, yellow, and green based on health
        Color color;
        if (healthPercentage > 0.5f)
        {
            color = Color.Lerp(Color.yellow, Color.green, (healthPercentage - 0.5f) * 2);
        }
        else
        {
            color = Color.Lerp(Color.red, Color.yellow, healthPercentage * 2);
        }

        // Apply the color with transparency
        color.a = 0.5f; // Semi-transparent
        spriteRenderer.color = color;
    }

    private void OnGoalDestroyed()
    {
        /*
        * Trigger the lose condition when the goal is destroyed.
        */

        Debug.Log("Goal destroyed!");
        Destroy(gameObject);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerLoseCondition();
        }
        else
        {
            Debug.LogError("GameManager instance is not set.");
        }
    }

    public bool isAlive()
    {
        /*
        * Check if the goal is alive.
        *
        * Returns:
        * True if the goal is alive, false otherwise
        */

        return currentHealth > 0;
    }
}
