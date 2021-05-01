using System;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public float maxHealth = 15f;

    [Header("Attacking Settings")] public float minDist = 3f, maxDist = 15f;
    public Vector2 damage = Vector2.up;
    public float attackTime = 0.5f;

    [SerializeField] private float influence = 7f;
    [SerializeField] private int _team;
    private float _currHealth;
    private float _minDistSqr, _attackTimer;
    private Transform _healthBar;

    private TowerUnitBrain _enemyUnit;
    private bool _seesEnemyTower, _inAttackRange;

    private void Start()
    {
        _currHealth = maxHealth;
        _healthBar = transform.GetChild(0);
        _minDistSqr = minDist * minDist;
        _attackTimer = 0f;
    }

    public float Influence => influence;

    public int Team
    {
        get => _team;
        set => _team = value;
    }

    private void Update()
    {
        // if see an enemy
        if (_seesEnemyTower)
        {
            if (_enemyUnit)
                AttackUnit();
            else
            {
                _enemyUnit = null;
                _seesEnemyTower = false;
                _inAttackRange = false;
            }
        }
        else
            CheckForUnit();
    }

    private void AttackUnit()
    {
        // check if time to attack yet
        if (Time.time >= _attackTimer)
        {
            _enemyUnit.ApplyDamage(UnityEngine.Random.Range(damage.x, damage.y));
            _attackTimer = Time.time + attackTime;
        }
    }

    private void CheckForUnit()
    {
        Vector3 pos = transform.position;
        // search for units w/in max radius
        Collider2D[] colls = Physics2D.OverlapCircleAll(
            pos,
            maxDist,
            LayerMask.GetMask("Agent"));

        // cycle thru results to see if there is one of the enemy
        bool foundEnemy = false; // init to false
        foreach (var col in colls)
        {
            TowerUnitBrain enemy = col.GetComponent<TowerUnitBrain>();
            // check if an enemy
            if (enemy.team != Team)
            {
                Vector3 diff = enemy.transform.position - pos;
                // check if still far enough
                if (diff.sqrMagnitude >= _minDistSqr)
                {
                    // set found enemy to true
                    foundEnemy = true;
                    // record enemy tower
                    _enemyUnit = col.GetComponent<TowerUnitBrain>();
                    // set attack timer
                    _attackTimer = Time.time + attackTime;
                    break;
                }
            }
        }

        // set sees enemy to whatever the result is
        _seesEnemyTower = foundEnemy;
    }


    public void ApplyDamage(float d)
    {
        _currHealth -= d;
        // die if no health
        if (_currHealth <= 0)
        {
            //GameManager.instance.TowerDied(this, transform.position);
            Destroy(gameObject);
        }
        else
        {
            // update health bar
            _healthBar.localScale = new Vector3(_currHealth / maxHealth, 0.1f, 1f);
        }
    }
}
