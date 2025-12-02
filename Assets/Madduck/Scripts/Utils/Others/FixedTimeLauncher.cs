using System;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Utils
{
    public class FixedTimeLauncher : MonoBehaviour
    {
        [Header("References")] 
        public Transform startPoint;
        public Transform target;
        public VerletFishingLine2D line;

        [Header("Launch Settings")] 
        public float div = 4f;
        public float power = 0.7f;
        public bool launchOnStart = true;
        public bool showTrajectory = true;

        [Header("Visualization")] 
        public LineRenderer trajectoryLine;
        public int trajectoryPoints = 30;
        
        private float _flightTime = 2f; // Time in seconds to reach target
        private Rigidbody2D _rb;
        
        [Button("Test Launch")]
        private void TestLaunch()
        {
            gameObject.transform.position = startPoint.position;
            LaunchProjectile();
        }

        void Start()
        {
            _rb = gameObject.GetComponent<Rigidbody2D>();
            gameObject.transform.position = startPoint.position;
            if (launchOnStart)
            {
                LaunchProjectile();
            }

            if (showTrajectory && trajectoryLine)
            {
                UpdateTrajectoryVisualization();
            }
        }

        void Update()
        {
            // Update trajectory visualization in real-time (optional)
            if (showTrajectory && trajectoryLine && (target.hasChanged || startPoint.hasChanged))
            {
                UpdateTrajectoryVisualization();
            }
        }

        public void LaunchProjectile()
        {
            if (!_rb)
            {
                Debug.LogError("Projectile needs a Rigidbody component!");
                return;
            }
            var distance = Vector3.Distance(startPoint.position, target.position);
            var divDistance = distance / div;
            _flightTime = Mathf.Pow(divDistance, power);
            _rb.constraints = RigidbodyConstraints2D.None;
            var launchVelocity = CalculateLaunchVelocity(_flightTime);
            _rb.linearVelocity = launchVelocity;
            Debug.Log($"Launched projectile with velocity: {launchVelocity}, Flight time: {_flightTime}s");
            Observable.Timer(TimeSpan.FromSeconds(_flightTime))
                .Subscribe(_ =>
                {
                    _rb.linearVelocity = Vector3.zero;
                    _rb.constraints = RigidbodyConstraints2D.FreezePosition;
                });
            line.CastLine(_flightTime, distance, true);
        }

        private Vector3 CalculateLaunchVelocity(float timeToTarget)
        {
            Vector3 startPos = startPoint.position;
            Vector3 targetPos = target.position;

            // Calculate displacement
            Vector3 displacement = targetPos - startPos;
            Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);

            // Calculate horizontal velocity (constant)
            Vector3 velocityXZ = displacementXZ / timeToTarget;

            // Calculate vertical velocity (accounts for gravity)
            // Using the equation: y = vy*t + 0.5*g*t²
            // Rearranged: vy = (y - 0.5*g*t²) / t
            float gravity = Physics.gravity.magnitude;
            float velocityY = (displacement.y - 0.5f * -gravity * timeToTarget * timeToTarget) / timeToTarget;

            Vector3 launchVelocity = velocityXZ + Vector3.up * velocityY;
            return launchVelocity;
        }

        void UpdateTrajectoryVisualization()
        {
            if (!trajectoryLine) return;

            trajectoryLine.positionCount = trajectoryPoints;
            Vector3 launchVelocity = CalculateLaunchVelocity(_flightTime);

            for (int i = 0; i < trajectoryPoints; i++)
            {
                float simulationTime = i / (float)trajectoryPoints * _flightTime;
                Vector3 displacement = launchVelocity * simulationTime +
                                       Vector3.up * (0.5f * Physics.gravity.y * simulationTime * simulationTime);
                trajectoryLine.SetPosition(i, startPoint.position + displacement);
            }
        }

        // Draw gizmos in scene view for visualization
        void OnDrawGizmos()
        {
            if (!startPoint|| !target) return;
            
            var distance = Vector3.Distance(startPoint.position, target.position) / div;
            _flightTime = Mathf.Pow(distance, power);

            // Draw start and target points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.5f);

            // Draw line between points
            Gizmos.color = Color.white;
            Gizmos.DrawLine(startPoint.position, target.position);

            // Draw calculated trajectory
            Vector3 launchVelocity = CalculateLaunchVelocity(_flightTime);
            Gizmos.color = Color.yellow;
            Vector3 previousPoint = startPoint.position;

            for (int i = 1; i <= 20; i++)
            {
                float simulationTime = i / 20f * _flightTime;
                Vector3 displacement = launchVelocity * simulationTime +
                                       Vector3.up * 0.5f * Physics.gravity.y * simulationTime * simulationTime;
                Vector3 currentPoint = startPoint.position + displacement;

                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
}