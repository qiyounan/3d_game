using UnityEngine;
using System.Collections;

namespace BossBattle.Boss
{
    /// <summary>
    /// Boss AI系统 - 增强的AI行为和决策
    /// </summary>
    public class BossAI : MonoBehaviour
    {
        [Header("AI设置")]
        public float detectionRange = 15f;
        public float optimalAttackRange = 5f;
        public float retreatRange = 2f;
        public float patrolRadius = 8f;
        
        [Header("行为权重")]
        [Range(0f, 1f)] public float aggressiveness = 0.7f;
        [Range(0f, 1f)] public float defensiveness = 0.3f;
        [Range(0f, 1f)] public float unpredictability = 0.2f;
        
        [Header("决策间隔")]
        public float decisionInterval = 1f;
        public float reactionTime = 0.5f;
        
        private BossController bossController;
        private Transform player;
        private Vector3 patrolCenter;
        private Vector3 currentPatrolTarget;
        private float lastDecisionTime;
        private float lastPlayerSightTime;
        private bool playerInSight = false;
        
        // AI状态
        public enum AIState
        {
            Patrol,
            Hunt,
            Engage,
            Retreat,
            Special
        }
        
        public AIState currentAIState = AIState.Patrol;
        
        void Start()
        {
            bossController = GetComponent<BossController>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            patrolCenter = transform.position;
            
            // 设置初始巡逻目标
            SetNewPatrolTarget();
        }
        
        void Update()
        {
            if (bossController == null || bossController.isDead) return;
            
            UpdatePlayerDetection();
            
            // 定期做出AI决策
            if (Time.time - lastDecisionTime >= decisionInterval)
            {
                MakeAIDecision();
                lastDecisionTime = Time.time;
            }
            
            ExecuteCurrentState();
        }
        
        void UpdatePlayerDetection()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            bool wasPlayerInSight = playerInSight;
            
            // 检查玩家是否在检测范围内
            if (distanceToPlayer <= detectionRange)
            {
                // 检查是否有视线阻挡
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                RaycastHit hit;
                
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerInSight = true;
                        lastPlayerSightTime = Time.time;
                    }
                    else
                    {
                        playerInSight = false;
                    }
                }
            }
            else
            {
                playerInSight = false;
            }
            
            // 如果刚发现玩家，触发反应
            if (!wasPlayerInSight && playerInSight)
            {
                OnPlayerDetected();
            }
        }
        
        void MakeAIDecision()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float healthPercentage = bossController.currentHealth / bossController.maxHealth;
            
            // 基于多种因素决定AI状态
            AIState newState = currentAIState;
            
            if (!playerInSight && Time.time - lastPlayerSightTime > 5f)
            {
                // 失去玩家视线太久，回到巡逻
                newState = AIState.Patrol;
            }
            else if (playerInSight)
            {
                if (distanceToPlayer > optimalAttackRange * 1.5f)
                {
                    // 距离太远，追击
                    newState = AIState.Hunt;
                }
                else if (distanceToPlayer < retreatRange && healthPercentage < 0.3f)
                {
                    // 血量低且距离太近，撤退
                    newState = AIState.Retreat;
                }
                else if (distanceToPlayer <= optimalAttackRange)
                {
                    // 在最佳攻击范围内，交战
                    newState = AIState.Engage;
                }
                
                // 随机使用特殊技能
                if (Random.value < unpredictability * 0.1f && healthPercentage < 0.5f)
                {
                    newState = AIState.Special;
                }
            }
            
            // 应用决策延迟
            if (newState != currentAIState)
            {
                StartCoroutine(ChangeStateWithDelay(newState, reactionTime));
            }
        }
        
        IEnumerator ChangeStateWithDelay(AIState newState, float delay)
        {
            yield return new WaitForSeconds(delay);
            currentAIState = newState;
            OnStateChanged(newState);
        }
        
        void ExecuteCurrentState()
        {
            switch (currentAIState)
            {
                case AIState.Patrol:
                    ExecutePatrol();
                    break;
                case AIState.Hunt:
                    ExecuteHunt();
                    break;
                case AIState.Engage:
                    ExecuteEngage();
                    break;
                case AIState.Retreat:
                    ExecuteRetreat();
                    break;
                case AIState.Special:
                    ExecuteSpecial();
                    break;
            }
        }
        
        void ExecutePatrol()
        {
            // 巡逻行为
            float distanceToTarget = Vector3.Distance(transform.position, currentPatrolTarget);
            
            if (distanceToTarget < 2f)
            {
                SetNewPatrolTarget();
            }
            
            // 朝向巡逻目标移动
            Vector3 direction = (currentPatrolTarget - transform.position).normalized;
            direction.y = 0;
            
            transform.position += direction * bossController.moveSpeed * 0.5f * Time.deltaTime;
            transform.LookAt(new Vector3(currentPatrolTarget.x, transform.position.y, currentPatrolTarget.z));
        }
        
        void ExecuteHunt()
        {
            if (player == null) return;
            
            // 追击玩家
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            // 使用预测性移动
            Vector3 predictedPosition = PredictPlayerPosition();
            Vector3 huntDirection = (predictedPosition - transform.position).normalized;
            huntDirection.y = 0;
            
            transform.position += huntDirection * bossController.moveSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
        
        void ExecuteEngage()
        {
            if (player == null) return;
            
            // 保持最佳攻击距离
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer > optimalAttackRange)
            {
                // 靠近
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                transform.position += direction * bossController.moveSpeed * 0.8f * Time.deltaTime;
            }
            else if (distanceToPlayer < optimalAttackRange * 0.7f)
            {
                // 后退
                Vector3 direction = (transform.position - player.position).normalized;
                direction.y = 0;
                transform.position += direction * bossController.moveSpeed * 0.5f * Time.deltaTime;
            }
            
            // 始终面向玩家
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            
            // 尝试攻击
            if (Time.time - bossController.lastAttackTime >= bossController.attackCooldown)
            {
                // 根据攻击性调整攻击频率
                if (Random.value < aggressiveness)
                {
                    bossController.currentState = BossController.BossState.Attacking;
                }
            }
        }
        
        void ExecuteRetreat()
        {
            if (player == null) return;
            
            // 撤退到安全距离
            Vector3 retreatDirection = (transform.position - player.position).normalized;
            retreatDirection.y = 0;
            
            // 添加一些随机性避免被预测
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized * 2f;
            
            Vector3 finalDirection = (retreatDirection + randomOffset * 0.3f).normalized;
            
            transform.position += finalDirection * bossController.moveSpeed * 1.2f * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
        
        void ExecuteSpecial()
        {
            // 执行特殊行为（由具体Boss类实现）
            // 这里可以触发特殊攻击或技能
            
            // 短暂停留后回到交战状态
            StartCoroutine(ReturnToEngageAfterDelay(2f));
        }
        
        IEnumerator ReturnToEngageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (playerInSight)
            {
                currentAIState = AIState.Engage;
            }
            else
            {
                currentAIState = AIState.Hunt;
            }
        }
        
        Vector3 PredictPlayerPosition()
        {
            if (player == null) return transform.position;
            
            // 简单的玩家位置预测
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 playerVelocity = playerRb.velocity;
                float timeToReach = Vector3.Distance(transform.position, player.position) / bossController.moveSpeed;
                return player.position + playerVelocity * timeToReach;
            }
            
            return player.position;
        }
        
        void SetNewPatrolTarget()
        {
            // 在巡逻半径内设置新的巡逻目标
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            currentPatrolTarget = patrolCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // 确保目标在地面上
            RaycastHit hit;
            if (Physics.Raycast(currentPatrolTarget + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                currentPatrolTarget.y = hit.point.y;
            }
        }
        
        void OnPlayerDetected()
        {
            UnityEngine.Debug.Log($"{bossController.bossName} 发现了玩家！");
            // 可以在这里播放发现玩家的音效或动画
        }

        void OnStateChanged(AIState newState)
        {
            UnityEngine.Debug.Log($"{bossController.bossName} AI状态改变: {currentAIState} -> {newState}");
        }
        
        void OnDrawGizmosSelected()
        {
            // 显示AI相关的范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, optimalAttackRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, retreatRange);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
            
            // 显示当前巡逻目标
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(currentPatrolTarget, 1f);
                Gizmos.DrawLine(transform.position, currentPatrolTarget);
            }
        }
    }
}
