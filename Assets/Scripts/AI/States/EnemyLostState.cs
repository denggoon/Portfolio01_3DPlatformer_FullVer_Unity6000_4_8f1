using UnityEngine;

public class EnemyLostState : IEnemyState
{
    public void Enter(EnemyMovement enemy)
    {
        enemy.isLostFlag = true;
        enemy.SetCurrentSpeed(0F);

        if (enemy.ContainsParam(AnimatorParams.LostEnemyTgr))
            enemy.animator.SetTrigger(AnimatorParams.LostEnemyTgr);
    }

    public void Update(EnemyMovement enemy)
    {
        // 플레이어 재발견 시 즉시 추격 재개
        if (enemy.vision.playerInSight)
        {
            enemy.TransitionTo(new EnemyChaseState());
            return;
        }

        enemy.chaseTimer -= Time.deltaTime;
        enemy.SetCurrentSpeed(0F);

        if (enemy.ContainsParam(AnimatorParams.LostTime))
            enemy.animator.SetFloat(AnimatorParams.LostTime, enemy.chaseTimer);

        // 대기 시간 만료 → 순찰로 복귀
        if (enemy.chaseTimer <= 0F)
        {
            enemy.vision.lastPlayerSighted = enemy.vision.unreachablePos;
            enemy.chaseTimer = enemy.chaseWaitTime;
            if (enemy.aggroTimer <= 0F)
                enemy.isAggroed = false;
            enemy.stuckTries = 0;
            enemy.TransitionTo(new EnemyPatrolState());
        }
    }

    public void Exit(EnemyMovement enemy)
    {
        enemy.isLostFlag = false;
    }
}
