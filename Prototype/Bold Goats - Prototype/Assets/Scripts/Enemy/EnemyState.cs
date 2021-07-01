using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{

    public class EnemyState : MonoBehaviour
    {
        public States state = States.Patrol;

        public delegate void StateChange();
        public event StateChange Alert;
        public event StateChange Investigate;
        public event StateChange Return;
        public event StateChange Patrol;

        public void InvokeAlert()
        {
            if (Alert != null)
            {
                Alert.Invoke();
            }

            ChangeState(States.Alert);
        }

        public void InvokeInvestigate()
        {
            if (Investigate != null)
            {
                Investigate.Invoke();
            }

            ChangeState(States.Investigate);
        }

        public void InvokeReturn()
        {
            if (Return != null)
            {
                Return.Invoke();
            }

            ChangeState(States.Return);
        }

        public void InvokePatrol()
        {
            if (Patrol != null)
            {
                Patrol.Invoke();
            }

            ChangeState(States.Patrol);
        }

        #region TEST CODE

        public bool ChangeToInvestigate;
        public bool ChangeToPatrol;
        public bool ChangeToReturn;
        public bool ChangeToChase;

        private void Update()
        {
            if (ChangeToInvestigate)
            {
                ChangeToInvestigate = false;
                InvokeInvestigate();
            }
        }

        private void ChangeState(States _state)
        {
            state = _state;
        }


        #endregion
    }

    public enum States
    {
        Patrol,
        Investigate,
        Alert,
        Chase,
        Attack,
        Return,
        Death
    }


}