using UnityEngine;

public class Tower : MonoBehaviour
{
    public float maxHealth = 15f;
    
    [Header("Attacking Settings")] 
    public float minDist = 3f, maxDist = 15f;
    public Vector2 damage = Vector2.up;

    [SerializeField] private float influence = 7f;
    [SerializeField]
    private int _team;
    private float _m, _b;
    private float _currHealth;
    private Transform _healthBar;

    private void Start()
    {
        _currHealth = maxHealth;
        _healthBar = transform.GetChild(0);
        // calculates for mx+b that produces an upside down function that zeroes at x=minDist,maxDist
        _m = (2f * influence) / (maxDist - minDist);
        _b = -(influence * (minDist + maxDist)) / (maxDist - minDist);
    }

    public float Influence => influence;

    public int Team
    {
        get => _team;
        set => _team = value;
    }

    public float M => _m;
    public float B => _b;

    public void ApplyDamage(float d)
    {
        _currHealth -= d;
        // die if no health
        if (_currHealth <= 0)
        {
            GameManager.instance.TowerDied(transform.position);
            Destroy(gameObject);
        }
        else
        {
            // update health bar
            _healthBar.localScale = new Vector3(_currHealth / maxHealth, 0.1f, 1f);
        }
    }
}
