using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController
{
    public class BoatDeckMover : MonoBehaviour, IMoverController
    {
        public PhysicsMover Mover;
        public Transform Hull;
        public float offsetUp;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private void Start()
        {
            _originalPosition = Mover.Rigidbody.position;
            _originalRotation = Mover.Rigidbody.rotation;

            Mover.MoverController = this;
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = Hull.position + Vector3.up * offsetUp;
            goalRotation = Hull.rotation;
        }
    }
}