using UnityEngine;
using UnityEngine.AI;

public class EnemyAIBase : MonoBehaviour
{

    #region General Variables
    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent;//Ref al cerebro NavMeshAgent del objeto
    [SerializeField] Transform target;//Ref a la posición del objetivo a perseguir
    [SerializeField] LayerMask targetLayer;//Define la capa del objetivo (Detección)
    [SerializeField] LayerMask groundLayer;//Define la capa del suelo (Definir puntos navegables)

    [Header("PAtroling Stats")]
    [SerializeField] float walkPointRange = 8f;//Radio máximo de margen espacial para buscar puntos de avegación aleatorios
    Vector3 walkPoint;//Punto de navegación aleatorio a perseguir
    bool walkPointSet;//Bool que define si ya se ha establecido un punto de navegación aleatorio o no

    [Header("Attacking Stats")]
    [SerializeField] float timeBetweenAttacks = 1f;//Tiempo de espera entre ataques
    [SerializeField] GameObject projectile;//Prefab del proyectil a disparar
    [SerializeField] Transform shootPoint;//Punto de origen del proyectil a disparar
    [SerializeField] float shootSpeedY = 0f;//Fuerza de disparo vertical (solo catapulta) 
    [SerializeField] float shootSpeedZ = 0f;//Fuerza de disparo delante (siempre está) 
    bool alreadyAttacked;//Bool que define si ya se ha atacado o no para controlar el tiempo entre ataques

    [Header("States & Detection Areas")]
    [SerializeField] float sightRange = 8f;//Radio de detección visual del objetivo
    [SerializeField] float attackRange = 2f;//Radio de detección visual del objetivo
    [SerializeField] bool targetInSightRange;//Determina si entra el estado PERSEGUIR
    [SerializeField] bool targetInAttackRange;//Determina si entra el estado ATACAR

    [Header("Stuck Detection")]
    [SerializeField] float stuckCheckTime = 2f;//Tiempo que el agente espera quieto antes de preguntarse si esta STUCK
    [SerializeField] float stuckThreshold = 0.1f;//Margen de detección de STUCK
    [SerializeField] float maxStuckDuration = 3f;//Tiempo máximo de estar STUCK 

    float stuckTimer;//Reloj que cuenta el tiempo de estar STUCK
    float lastCheckTime;//Tiempo de chequeo previo a estar STUCK
    Vector3 lastPosition;//Posición previa a estar STUCK
    #endregion
    void Awake()
    {
        target = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
        lastCheckTime = Time.time;
    }
    // Update is called once per frame
    void Update()
    {
        EnemyStateUpdater();
        CheckIfStuck();
    }
    void EnemyStateUpdater()
    {
        //Acción que se encarga de los estados de la IA
        //Esfera de detección fisica
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
        targetInSightRange = hits.Length > 0;
        //Si está persiguiendo, calcula la distancia hasta que el minimo entre en el rango de ataque para cambiar a ese estado
        if (targetInSightRange)
        {
            float distance = Vector3.Distance(transform.position, target.position);
           targetInAttackRange = distance <= attackRange;
        }
        //Logica de los cambios de estado
        if (!targetInSightRange && !targetInAttackRange) Patroling();//Si no ve al target, patrulla
        if (targetInSightRange && !targetInAttackRange) ChaseTarget();//Si ve al target pero no puede atacarlo, lo persigue
        if (targetInAttackRange && targetInSightRange) AttackTarget();//Si ve al target y puede atacarlo, lo ataca
    }
    void Patroling()
    {
        //Define que el objeto patrulle y genere puntos de pratrulla random
        //1 - Revisa si hay punto a patrullar
        if (!walkPointSet) 
        {
            //Si no hay wlakpoin, busca uno 
            SearchWalkPoint();
        }
        else agent.SetDestination(walkPoint);//Si hay punto, lo persigue
        //2 - Una vez que llega al punto, hay que decirle al sistema que puede generar uno nuevo
        if ((transform.position-walkPoint).sqrMagnitude<1f)
        {
            walkPointSet = false;
        }
    }
    void SearchWalkPoint()
    {
        //Acción que busca un punto de patrulla random si no lo hay
        int attempts = 0;//Contador de intentos para evitar bucles infinitos
        const int maxAttempts = 5;
        while(!walkPointSet && attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomPoint = transform.position + new Vector3(Random.Range(-walkPointRange, walkPointRange), 0, Random.Range(-walkPointRange, walkPointRange));
            //Chequea si el punto está en un lugar en el que haya NavMesh Surface
            if(NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                walkPoint = hit.position;//Determina el Vector3 random a perseguir
                if(Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
                {
                    walkPointSet = true;//Tenemos punto y el agente va hacia él
                }
            }
        }
    }
    void ChaseTarget()
    {
        //Le dice al agente que persiga al target
        agent.SetDestination(target.position);
    }
    void AttackTarget()
    {
        //Acción que determina el ataque al objetivo
        //1- Detener el movimiento
        agent.SetDestination(transform.position);
        //2- Rotación suavizada para mirar al objetivo
        Vector3 direction = (target.position - transform.position).normalized;
        //Condicional que revisa si el ajente y objetivo NO se están mirando
        if(direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, agent.angularSpeed * Time.deltaTime);
        }
        //3- Definir el ataque en sí
        //Solo atacará si no se esta atacando
        if(!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, shootPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * shootSpeedZ, ForceMode.Impulse);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    void ResetAttack()
    {
        //Acción que resetea el ataque
        alreadyAttacked = false;
    }
    void CheckIfStuck()
    {
        //Acción que revisa si el agente está STUCK
        if(Time.time - lastCheckTime > stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if(distanceMoved < stuckThreshold)
            {
                stuckTimer += Time.time - lastCheckTime;
                if(stuckTimer >= stuckThreshold && agent.hasPath)
                {
                    stuckTimer+=stuckCheckTime;
                }
            }
            else
            {
                stuckTimer = 0; 
            }
            if(stuckTimer >= maxStuckDuration)
            {
                agent.ResetPath();
                stuckTimer = 0;
            }
            lastPosition = transform.position;
            lastCheckTime = Time.time;
        }
    }
    private void OnDrawGizmosSelected()
    {
       if(Application.isPlaying)return;//Solo se ejecutan los gizmos en editor de Unity

       Gizmos.color = Color.red;
       Gizmos.DrawWireSphere(transform.position, attackRange);//Dibuja el radio de ataque en rojo
       Gizmos.color = Color.yellow;
       Gizmos.DrawWireSphere(transform.position, sightRange);//Dibuja el radio de detección en amarillo
    }
}
