using UnityEngine;
using System.Collections;

public enum E_AI_STATUS
{
    ATTACK,
    CHASE,
    PATROL,
    FLEE,
    IDLE,
}

public enum E_MONSTER_TYPE
{
    UNIDENTIFIED = 0,
    MO = 1,
    BO,
    SPINE_MO,
    CANNON,
    REMAINS_MO,
    FACTORY_MO
}

public class EnemyMovement : MonoBehaviour
{
    public E_MONSTER_TYPE eMonsterType = E_MONSTER_TYPE.UNIDENTIFIED;
    public string monsterID;
    public E_AI_STATUS eAIStatus;

    public bool isImmortal = false;
    public float health = 1F;
    public bool isInvincible = false;
    public bool isPlayerOnTheHead = false;

    // 상태에서 직접 접근하는 컴포넌트 레퍼런스
    [HideInInspector] public EnemyVision    vision;
    [HideInInspector] public PatrolRoute    patrolRoute;
    [HideInInspector] public EnemyWeapon   weapon;
    [HideInInspector] public Animator      animator;
    [HideInInspector] public UnityEngine.AI.NavMeshAgent nav;

    private CapsuleCollider _myCollider;
    private AnimatorControllerParameter[] _animParams;

    public float attackRange;
    public bool canMoveAttack = false;

    private float _modifiedSpeed = -999F;
    public float currentSpeed;

    public float chaseSpeed;
    public float patrolSpeed;

    public float aggroWaitTime = 1F;
    public float chaseWaitTime;
    public float patrolWaitTime;
    public float stunWaitTime;
    public float stunInterval;
    public float invincibleTime = 1F;

    private float _deathWaitTime = 0F;
    private float _stunTimer;
    private float _invincibleTimer;

    [SerializeField] private float _attackTimer;

    public float stuckCheckTime;
    public int stuckTriesLimit;

    // 상태에서 읽고 쓰는 런타임 값
    [System.NonSerialized] public float aggroTimer;
    [System.NonSerialized] public bool  isAggroed = false;
    [System.NonSerialized] public bool  isLostFlag = false;
    [System.NonSerialized] public float chaseTimer;
    [System.NonSerialized] public float patrolTimer;
    [System.NonSerialized] public int   stuckTries;
    [System.NonSerialized] public float stuckIntervalTimer;
    [System.NonSerialized] public float prevRemainingDist;
    [System.NonSerialized] public float stoppingDist;

    private bool _isStunned = false;
    private Vector3 _destination;

    public bool autoColliderSetting = true;
    public bool autoVariableSetting = true;

    public string dropObjStr;
    public GameObject dropFxObj = null;
    public bool hasDroppable = true;
    public int dropCount = 3;
    private float _objDropForce = 270F;

    private const float ColliderCenterY  = 0.25F;
    private const float ColliderRadius   = 0.15F;
    private const float ColliderHeight   = 0.5F;

    public Animation foundAni;
    public Animation lostAni;
    public SkinnedMeshRenderer[] stunnedBody;

    // 상태머신
    private IEnemyState _currentState;

    void Awake()
    {
        _myCollider  = GetComponent<CapsuleCollider>();
        nav          = GetComponent<UnityEngine.AI.NavMeshAgent>();
        patrolRoute  = GetComponent<PatrolRoute>();
        weapon       = GetComponent<EnemyWeapon>();
        vision       = transform.Find("VisionCollider").GetComponent<EnemyVision>();

        if (autoColliderSetting)
        {
            _myCollider.center = new Vector3(0F, ColliderCenterY, 0F);
            _myCollider.radius = ColliderRadius;
            _myCollider.height = ColliderHeight;
        }

        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            animator = this.transform.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            _animParams = animator.parameters;

            if (animator.gameObject.GetComponent<AnimatorEventCaller>() == null)
                animator.gameObject.AddComponent<AnimatorEventCaller>();

            stunnedBody = animator.GetComponentsInChildren<SkinnedMeshRenderer>();

            var artRsrcName = animator.gameObject.name;
            var monsterEnums = System.Enum.GetValues(typeof(E_MONSTER_TYPE)) as int[];

            if (eMonsterType == E_MONSTER_TYPE.UNIDENTIFIED)
            {
                for (int i = 0; i < monsterEnums.Length; i++)
                {
                    monsterID = monsterEnums[i].ToString("000");
                    if (artRsrcName.Contains(monsterID))
                    {
                        eMonsterType = (E_MONSTER_TYPE)monsterEnums[i];
                        break;
                    }
                }
            }
        }

        if (attackRange == 0)
            attackRange = 2F;
    }

    void Start()
    {
        if (autoVariableSetting)
        {
            chaseWaitTime = patrolWaitTime = stunWaitTime = 3F;
            _deathWaitTime = 1F;
            chaseSpeed = patrolSpeed = 1F;
            stuckTriesLimit = 1;
            stuckCheckTime = 1F;
            stunInterval = .05F;
        }

        patrolTimer    = patrolWaitTime;
        chaseTimer     = chaseWaitTime;
        _invincibleTimer = invincibleTime;

        AssignAttackRange();

        TransitionTo(new EnemyPatrolState());
    }

    void Update()
    {
        if (GameRuleManager.instance == null) return;
        if (GameRuleManager.instance.eGameStatus == E_GAME_STATUS.GAME_READY) return;

        if (weapon != null && _attackTimer >= 0F)
            _attackTimer -= Time.deltaTime;

        if (_isStunned)
        {
            nav.speed = 0F;
            return;
        }

        _currentState?.Update(this);

        if (weapon != null && !canMoveAttack && _attackTimer >= 0F)
            SetCurrentSpeed(0F);

        nav.speed = _modifiedSpeed == -999F ? currentSpeed : _modifiedSpeed;

        if (ContainsParam(AnimatorParams.Speed))
            animator.SetFloat(AnimatorParams.Speed, nav.speed);

        stoppingDist = nav.stoppingDistance;
        _destination  = nav.destination;
    }

    // ─── 상태 전환 ────────────────────────────────────────────────────

    public void TransitionTo(IEnemyState next)
    {
        _currentState?.Exit(this);
        _currentState = next;
        _currentState.Enter(this);
    }

    // ─── 상태에서 호출하는 헬퍼 ──────────────────────────────────────

    public void AssignAttackRange()
    {
        if (weapon != null)
            attackRange = weapon.attackRange;

        if (attackRange > vision.visionCollider.radius || attackRange <= 0F)
            attackRange = vision.visionCollider.radius;
    }

    public void PerformAttack()
    {
        if (weapon == null) return;
        eAIStatus = E_AI_STATUS.ATTACK;
        var dir = eMonsterType == E_MONSTER_TYPE.CANNON
            ? weapon.fireTrans.TransformDirection(Vector3.forward)
            : vision.normVisionVector;
        weapon.Attack(dir);
    }

    public void ProceedToNextPatrolPoint()
    {
        if (patrolRoute.routeIndex == patrolRoute.routes.Length - 1)
            patrolRoute.routeIndex = 0;
        else
            patrolRoute.routeIndex++;

        patrolTimer = 0F;
        stuckTries  = 0;
    }

    public void RunStuckFreeSequence(System.Action onStuck)
    {
        if (stuckTries > stuckTriesLimit)
        {
            onStuck?.Invoke();
            return;
        }

        stuckIntervalTimer += Time.deltaTime;
        if (stuckIntervalTimer > stuckCheckTime)
        {
            stuckIntervalTimer = 0F;
            if (Mathf.Abs(nav.remainingDistance - prevRemainingDist) <= stoppingDist)
                stuckTries++;
            prevRemainingDist = nav.remainingDistance;
        }
    }

    // ─── 스턴 / 사망 (외부 호출, 기존 유지) ─────────────────────────

    public void Stun(float damage = 0F, bool stomped = false)
    {
        if (_isStunned || isInvincible) return;

        if (!isImmortal && health > 0F)
            health -= damage;

        if (eMonsterType == E_MONSTER_TYPE.FACTORY_MO)
        {
            vision.transform.gameObject.SetActive(false);
            var contactCollider = transform.Find("MeleeContactCollider")?.gameObject;
            if (contactCollider != null)
                contactCollider.SetActive(false);
            vision.playerInSight = false;
            // LostState로 직접 전환하여 잃어버림 처리
            TransitionTo(new EnemyLostState());
            StartCoroutine(ActiveVisionDelayed(stunWaitTime));
        }
        else
        {
            StartCoroutine(StartStun(stomped));
        }

        vision.lastPlayerSighted = vision.unreachablePos;

        if (SoundBoard.instance != null)
            SoundBoard.instance.PlayFromSoundBoard(SoundID.MON_Hit, this.transform.position);
    }

    public void MonsterDeath()
    {
        if (!isImmortal && health <= 0F)
        {
            if (SoundBoard.instance != null)
                SoundBoard.instance.PlayFromSoundBoard(SoundID.MON_Despawn, this.transform.position);

            if (hasDroppable)
                MakeDropItems();

            Destroy(gameObject);
        }
    }

    IEnumerator ActiveVisionDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        vision.transform.gameObject.SetActive(true);
        var contactCollider = transform.Find("MeleeContactCollider")?.gameObject;
        if (contactCollider != null)
            contactCollider.SetActive(true);
    }

    void MakeDropItems()
    {
        if (dropFxObj != null)
            ResourcesManager.instance.PopEffect(dropFxObj, this.transform.position);
        else
            ResourcesManager.instance.PopEffect(AddressableKeys.Fx.M_Die01, this.transform.position);

        for (int i = 0; i < dropCount; i++)
            DropItem(Random.insideUnitSphere);
    }

    void DropItem(Vector3 droppingDirection)
    {
        hasDroppable = false;

        if (string.IsNullOrEmpty(dropObjStr)) return;

        var dropItemObj = ResourcesManager.instance.LoadGameObject(dropObjStr) as GameObject;
        if (dropItemObj == null) return;

        var dropItem = GameObject.Instantiate(dropItemObj, this.transform.position + this.transform.up, Quaternion.identity);
        var dropRigid = dropItem.GetComponent<Rigidbody>();
        if (dropRigid != null)
            dropRigid.AddForce(droppingDirection * _objDropForce);
    }

    IEnumerator StartStun(bool stomped)
    {
        _isStunned   = true;
        isInvincible = true;

        animator.SetTrigger(stomped ? AnimatorParams.JumpDamagedTgr : AnimatorParams.DamagedTgr);

        _stunTimer = health < 1 ? 0F : stunWaitTime;

        float intervalChecker = 0F;
        while (_stunTimer > 0F)
        {
            _stunTimer       -= Time.deltaTime;
            intervalChecker += Time.deltaTime;
            animator.SetFloat(AnimatorParams.StunTime, _stunTimer);

            if (intervalChecker > stunInterval)
            {
                foreach (var r in stunnedBody) r.enabled = !r.enabled;
                intervalChecker = 0F;
            }
            yield return null;
        }

        if (health > 0)
        {
            _isStunned       = false;
            _invincibleTimer = invincibleTime;

            while (_invincibleTimer > 0F)
            {
                _invincibleTimer -= Time.deltaTime;
                intervalChecker  += Time.deltaTime;
                if (intervalChecker > stunInterval)
                {
                    foreach (var r in stunnedBody) r.enabled = !r.enabled;
                    intervalChecker = 0F;
                }
                yield return null;
            }

            foreach (var r in stunnedBody) r.enabled = true;
        }

        isInvincible = false;
    }

    // ─── 유틸리티 ────────────────────────────────────────────────────

    public bool ContainsParam(string paramName)
    {
        if (_animParams == null) return false;
        foreach (var p in _animParams)
            if (p.name.Equals(paramName)) return true;
        return false;
    }

    public void FootSound() { }

    // ─── Get / Set ───────────────────────────────────────────────────

    public EnemyVision GetVision()               => vision;
    public bool IsStunned()                      => _isStunned;
    public CapsuleCollider GetMyCollider()        => _myCollider;
    public UnityEngine.AI.NavMeshAgent GetMyNav() => nav;
    public float GetAttackTimer()                => _attackTimer;
    public Animator GetAnimator()                => animator;
    public Vector3 GetDestination()              => _destination;

    public void SetAttackTimer(float timer)      => _attackTimer   = timer;
    public void SetCurrentSpeed(float speed)     => currentSpeed   = speed;
    public void SetModifiedSpeed(float speed)    => _modifiedSpeed = speed;
    public void SetAggroTimer(float value)       => aggroTimer     = value;

    public EnemyWeapon GetWeapon() => weapon;
}
