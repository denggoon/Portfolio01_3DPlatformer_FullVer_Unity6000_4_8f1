using UnityEngine;

public class EnemyPatrolState : IEnemyState
{
    public void Enter(EnemyMovement enemy)
    {
        enemy.eAIStatus = E_AI_STATUS.PATROL;
        enemy.vision.ChangeVisionRadius(E_AI_STATUS.PATROL);
    }

    public void Update(EnemyMovement enemy)
    {
        if (enemy.vision.playerInSight)
        {
            enemy.TransitionTo(new EnemyChaseState());
            return;
        }

        var route = enemy.patrolRoute;
        if (route == null || route.routes == null || route.routes.Length == 0) return;
        if (route.routes[0] == null || route.routes[0].routeTrans == null) return;

        enemy.SetCurrentSpeed(enemy.patrolSpeed);
        if (route.routes[route.routeIndex].routeSpeed > 0F)
            enemy.SetCurrentSpeed(route.routes[route.routeIndex].routeSpeed);

        var nav = enemy.nav;
        if (nav.remainingDistance <= nav.stoppingDistance || nav.destination == enemy.vision.unreachablePos)
        {
            if (enemy.patrolTimer >= enemy.patrolWaitTime)
            {
                enemy.ProceedToNextPatrolPoint();
            }
            else
            {
                enemy.patrolTimer += Time.deltaTime;
                enemy.SetCurrentSpeed(0F);
            }
        }
        else
        {
            enemy.patrolTimer = 0F;
            enemy.RunStuckFreeSequence(() => enemy.ProceedToNextPatrolPoint());
        }

        var target = route.routes[route.routeIndex].routeTrans.position;
        if (nav.destination != target)
            nav.SetDestination(target);
    }

    public void Exit(EnemyMovement enemy) { }
}
