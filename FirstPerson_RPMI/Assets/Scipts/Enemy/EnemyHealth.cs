using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health System Parameters")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health;

    [Header("Feedback Configuration")]
    [SerializeField] Material damageMat;
    [SerializeField] GameObject deathVfx;
    [SerializeField] MeshRenderer enemyRend;
    Material baseMat;

    private void Awake()
    {
        health = maxHealth;
        baseMat = enemyRend.material;
    }


    void Update()
    {
        if(health <= 0)
        {
            health = 0;
            deathVfx.SetActive(true);
            deathVfx.transform.position = transform.position;
            gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        enemyRend.material= damageMat;
        Invoke(nameof(ResetEnemyMaterial), 0.1f);
    }

    void ResetEnemyMaterial()
    {
        enemyRend.material = baseMat;
    }
}
