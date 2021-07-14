using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    [RequireComponent(typeof(EnemyVision))]
    public class EnemyPathFind : MonoBehaviour
    {
        private NavMeshAgent aiEnemy;
        public Transform[] guardPoints;
        private int destinationPoint = 0;

        public Color colorAttack;
        Color originalColor;

        Transform lastPosition;
        Transform investigatePosition;

        EnemyVision enemyVision;

        
        EnemyState enemyState;

        [SerializeField] float maxDistanceForChase = 10f;
        [SerializeField] float distanceToAttack = 2f;

        [SerializeField] Material attackMaterial;

        // Start is called before the first frame update
        void Awake()
        {
            aiEnemy = GetComponent<NavMeshAgent>();
            enemyState = GetComponent<EnemyState>();
            enemyVision = GetComponent<EnemyVision>();
            originalColor = GetComponent<Renderer>().material.color;


            enemyState.Chase += HandleInvokeChase;
            enemyState.Investigate += HandleInvokeInvestigate;
            enemyState.Return += HandleInvokeReturn;

            lastPosition = new GameObject().transform;
            investigatePosition = new GameObject().transform;

            lastPosition.name = gameObject.name + " lastPos";
            investigatePosition.name = gameObject.name + " investigatePos";

            enemyState.state = States.Patrol;
            GoToNextPoint();
        }

        private void OnDisable()
        {
            enemyState.Chase -= HandleInvokeChase;
            enemyState.Investigate -= HandleInvokeInvestigate;
            enemyState.Return -= HandleInvokeReturn;
        }

        // Update is called once per frame
        void Update()
        {

            PerformNavigation();
        }

        void GoToNextPoint()
        {
            // If there are no points set, No Need to continue Function
            
            //if (gaurdPoints.Length == 0)
            //{
            //    return;
            //}

            // Set Gaurd point to the point currently selected
            aiEnemy.destination = guardPoints[destinationPoint].position;

            // Set Destination Point to next point
            destinationPoint = (destinationPoint + 1) % guardPoints.Length;
        }

        public void HandleInvokeChase()
        {
            lastPosition.position = transform.position;
        }

        public void HandleInvokeInvestigate()
        {
            lastPosition.position = transform.position;
        }

        public void HandleInvokeReturn()
        {
            enemyVision.Suspicion = 45;
            GetComponent<Renderer>().material.color = originalColor;
            aiEnemy.ResetPath();
        }

        // Switch for enemy behavior
        void PerformNavigation()
        {
            switch (enemyState.state)
            {
                case States.Alert:
                    break;
                case States.Attack:

                    Debug.Log("Reached attack state");

                    GetComponent<Renderer>().material.color = colorAttack;



                    break;
                case States.Chase:

                    
                    aiEnemy.destination = GameManager.Instance.Player.transform.position;

                    float distance = Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);

                    if (distance >= maxDistanceForChase)
                    {
                        enemyState.InvokeReturn();
                    } else if (distance <= distanceToAttack)
                    {

                        aiEnemy.isStopped = true;
                        aiEnemy.ResetPath();
                        enemyState.InvokeAttack();
                    }


                    break;
                case States.Death:



                    break;
                case States.Investigate:
                    break;
                case States.Patrol:
                    
                    if (aiEnemy.remainingDistance < 1.0f && !aiEnemy.pathPending)
                    {
                        GoToNextPoint();
                    }

                    break;
                case States.Return:

                    
                    aiEnemy.destination = lastPosition.position;

                    if (aiEnemy.remainingDistance <= 1.0f && !aiEnemy.pathPending)
                    {
                        destinationPoint--;
                        if (destinationPoint < 0)
                        {
                            destinationPoint = guardPoints.Length - 1;
                        }

                        enemyVision.Suspicion = 0;
                        enemyState.InvokePatrol();
                    }

                    break;
                default:
                    break;
            }

        }
    }
}
