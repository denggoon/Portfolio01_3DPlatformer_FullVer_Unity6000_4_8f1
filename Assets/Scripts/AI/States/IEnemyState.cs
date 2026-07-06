public interface IEnemyState
{
    void Enter(EnemyMovement enemy);
    void Update(EnemyMovement enemy);
    void Exit(EnemyMovement enemy);
}
