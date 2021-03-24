using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TowerUnitBrain : MonoBehaviour
{
    public int team;

    public float maxHealth = 5f;

    [Header("Attacking Settings")] public float towerCheckRadius = 10f;
    public float minAttackDist = 3f;
    public Vector2 damage = Vector2.up;
    public float attackTime = 0.2f;

    private TowerUnitSteering _steering;
    private Tower _enemyTower;
    private float _currHealth;
    private float _attackTimer;
    private bool _seesEnemyTower, _inAttackRange;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("start");
        _steering = GetComponent<TowerUnitSteering>();
        _steering.SetPathMode(TowerUnitSteering.PathMode.FLOW);

        _currHealth = maxHealth;
        _seesEnemyTower = false;
        _inAttackRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("update");
        // if see an enemy
        if (_seesEnemyTower)
        {
            if (_enemyTower)
            {
                if (_inAttackRange)
                    AttackTower();
                else
                    MoveTowardTower();
            }
            else
            {
                _enemyTower = null;
                _seesEnemyTower = false;
                _inAttackRange = false;
                _steering.SetPathMode(TowerUnitSteering.PathMode.FLOW);
            }
        }
        else
            CheckForTower();
    }

    private void AttackTower()
    {
        // check if time to attack yet
        if (Time.time >= _attackTimer)
        {
            _enemyTower.ApplyDamage(Random.Range(damage.x, damage.y));
            _attackTimer = Time.time + attackTime;
        }
    }

    private void MoveTowardTower()
    {
        // if in attack range
        if ((transform.position - _enemyTower.transform.position).sqrMagnitude < minAttackDist * minAttackDist)
        {
            _inAttackRange = true;
            _attackTimer = Time.time + attackTime;
            _steering.SetPathMode(TowerUnitSteering.PathMode.STOP);
        }
        else
        {
            _inAttackRange = false;
            _steering.SetPathMode(TowerUnitSteering.PathMode.SEEK);
        }
    }

    private void CheckForTower()
    {
        Vector3 pos = transform.position;
        // search for towers in radius
        Collider2D[] colls = Physics2D.OverlapCircleAll(
            pos,
            towerCheckRadius,
            LayerMask.GetMask("Tower"));

        // cycle thru results to see if there is one of the enemy
        bool foundEnemy = false; // init to false
        foreach (var col in colls)
        {
            // if not on same team
            Tower tower = col.GetComponent<Tower>();
            if (tower.Team != team)
            {
                // set found enemy to true
                foundEnemy = true;
                // record enemy tower
                _enemyTower = col.GetComponent<Tower>();
                // set seek target to that tower's position
                _steering.SetTargetLocation(_enemyTower.transform.position);
                break;
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
            GameManager.instance.UnitDied(this, transform.position);
    }
}
