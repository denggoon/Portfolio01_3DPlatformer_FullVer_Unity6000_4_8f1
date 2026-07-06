using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    public void Enter(EnemyMovement enemy)
    {
        enemy.eAIStatus = E_AI_STATUS.CHASE;
        enemy.vision.ChangeVisionRadius(E_AI_STATUS.CHASE);
    }

    public void Update(EnemyMovement enemy)
    {
        enemy.SetCurrentSpeed(enemy.chaseSpeed);

        if (enemy.vision.playerInSight)
        {
            UpdateVisible(enemy);
        }
        else
        {
            UpdateSeek(enemy);
        }

        // 어그로 애니메이션 재생 중 정지
        if (enemy.aggroTimer > 0F)
        {
            enemy.aggroTimer -= Time.deltaTime;
            enemy.SetCurrentSpeed(0F);
        }

        // 잃어버린 상태(두리번) 중 정지
        if (enemy.isLostFlag)
            enemy.SetCurrentSpeed(0F);

        // 공격 범위 진입 시 공격
        enemy.AssignAttackRange();
        if (enemy.vision.playerInSight && enemy.vision.distance <= enemy.attackRange)
            enemy.PerformAttack();
    }

    private void UpdateVisible(EnemyMovement enemy)
    {
        enemy.isLostFlag = false;

        if (enemy.ContainsParam(AnimatorParams.LostTime))
            enemy.animator.SetFloat(AnimatorParams.LostTime, 0F);

        if (enemy.eMonsterType == E_MONSTER_TYPE.REMAINS_MO && enemy.isPlayerOnTheHead)
        {
            enemy.SetCurrentSpeed(enemy.patrolSpeed);
        }
        else
        {
            enemy.vision.visionCollider.enabled = true;
            enemy.nav.enabled = true;
        }

        if (enemy.nav.destination != enemy.vision.lastPlayerSighted)
            enemy.nav.SetDestination(enemy.vision.lastPlayerSighted);

        // 최초 발견 시 어그로 처리
        if (!enemy.isAggroed)
        {
            if (SoundBoard.instance != null)
                SoundBoard.instance.PlayFromSoundBoard(SoundID.MON_Find, GameRuleManager.instance.playerMove.transform.position);
            if (enemy.animator != null)
                enemy.animator.SetTrigger(AnimatorParams.FoundEnemyTgr);
            enemy.aggroTimer = 0F;
            enemy.isAggroed = true;
        }
    }

    private void UpdateSeek(EnemyMovement enemy)
    {
        // 마지막 목격 지점 도착 → LostState로 전환
        if (enemy.nav.remainingDistance <= enemy.nav.stoppingDistance)
        {
            enemy.TransitionTo(new EnemyLostState());
            return;
        }

        enemy.RunStuckFreeSequence(() => enemy.TransitionTo(new EnemyLostState()));
    }

    public void Exit(EnemyMovement enemy) { }
}
