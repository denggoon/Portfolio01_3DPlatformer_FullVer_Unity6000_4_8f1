using UnityEngine;

public class MonsterAnimEventCaller : AnimatorEventCaller
{
    private EnemyMovement _enemy;

    protected override void Awake()
    {
        base.Awake();
        _enemy = parentObj.GetComponent<EnemyMovement>();
    }

    public void FootSound()        => _enemy?.FootSound();
    public void ProjectileAttack() => _enemy?.weapon?.ProjectileAttack();
    public void RaycastAttack()    => _enemy?.weapon?.RaycastAttack();
    public void MonsterDeath()     => _enemy?.MonsterDeath();
}
