using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyVisionChild : MonoBehaviour
    {
        EnemyVision enemyVision;

        private void Awake()
        {
            enemyVision = transform.parent.GetComponent<EnemyVision>();
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.tag == "Player")
            {
                enemyVision.checkForPlayer = true;
            }
            else if (other.tag == "Enemy")
            {
                other.GetComponent<EnemyState>().Alert += HandleAlert;
            }


        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                enemyVision.checkForPlayer = false;
            }
            else if (other.tag == "Enemy")
            {
                other.GetComponent<EnemyState>().Alert -= HandleAlert;
            }
        }

        void HandleAlert()
        {
            throw new System.NotImplementedException();
        }
    }
}