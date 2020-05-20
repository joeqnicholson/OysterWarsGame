using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum WadeCameraState
{

}
public class WadeCamera : MonoBehaviour
{

    public float Distance = 50.0f;
    public float Height = 4.0f;
    public float maxDistance;
    public float maxRegDistance;
    public float minDistance;

    private bool _distanceIsObstructed;
    private float _currentDistance;
    private float _targetVerticalAngle;
    private RaycastHit _obstructionHit;
    private int _obstructionCount;
    private RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];
    private float _obstructionTime;
    private Vector3 _currentFollowPosition;
    private const int MaxObstructions = 32;
    public float ObstructionCheckRadius = 0.2f;
    public LayerMask ObstructionLayers = -1;
    public float ObstructionSharpness = 10000f;
    public List<Collider> IgnoredColliders = new List<Collider>();
    private float targetDistance { get; set; }
    public float lockTilt;
    public float cachedDistance;
    public bool triggerInUse;


    public GameObject PlayerTarget;
    public GameObject lockOnIndicator;
    public Transform followTarget;
    public WadeInputs input;
    private Transform target;
    public Transform lockOnTarget;
    public GameObject lockOnInstance;
    //public Component input;
    public bool lockedOn = false;
    private WadeMachine machine;
    public float tilt = 15;
    public float heading;
    public float orbitSpeed;
    public float autoOrbitSpeed;
    public float boatAutoOrbitSpeed;
    public float boatOrbit;
    public float boatDistance;
    public float aimDistance;
    public float aimTilt;
    //private SuperCharacterController controller;
    public float lockOnDistance;
    public float lockOnHeight;
    public float pullbackSpeed;
    public float offsetY;
    public float aimMovement;
    public Vector3 aimMovementDifference;
    public List<Transform> enemiesOnScreen = new List<Transform>();
    public int enemyIndex = 0;
    bool hasHitCurrent;

    public bool cube = true;
    int num = 1;

    // Update is called once per frame


    private void Awake()
    {
        targetDistance = Distance;

    }
    // Use this for initialization
    void Start()
    {
        lockOnInstance = Instantiate(lockOnIndicator, PlayerTarget.transform.position + transform.forward * 50, PlayerTarget.transform.rotation);
        Vector3 rot = transform.localRotation.eulerAngles;
        heading = rot.y;
        tilt = 360;
        input = PlayerTarget.GetComponent<WadeInputs>();
        machine = PlayerTarget.GetComponent<WadeMachine>();
        target = followTarget.transform;

    }
    private void LateUpdate()
    {
        TargetingList();
        LockOnControls();
        HandleCollision();

        heading = heading % 360;

        if (!lockedOn)
        {
            tilt = Mathf.Clamp(tilt, 360, 410);

            if(machine.stateString != "Aim")
            {
                if (Mathf.Abs(input.Current.MouseInput.y) > 0.01f) { tilt -= input.Current.MouseInput.y * orbitSpeed * Time.deltaTime; }

                if (Mathf.Abs(input.Current.MouseInput.x) > 0.01f) { heading += input.Current.MouseInput.x * orbitSpeed * Time.deltaTime; }

            }

            if (machine.stateString == "Walk")
            {
                if(input.Current.MouseInput.magnitude < 0.1f)
                {
                    if (Mathf.Abs(input.Current.MoveInput.x) > 0.1f)
                    {


                        heading += autoOrbitSpeed * input.Current.MoveInput.x * Time.deltaTime;
                    }
                }
                
            }


            if (machine.stateString == "Drive")
            {
                
            }
            else
            {
                maxDistance = Mathf.Lerp(maxDistance, maxRegDistance, 6 * Time.deltaTime);
            }

            Quaternion desiredRot = Quaternion.Euler(tilt, heading, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 12f * Time.deltaTime);

            Vector3 desiredPos = target.position - transform.forward * Distance;
            transform.position = desiredPos + Vector3.up * offsetY;

        }

        if (lockedOn)
        {

            float lockHeading = lockOnInstance.transform.localRotation.eulerAngles.y;

            float lockTilt = lockOnInstance.transform.localRotation.eulerAngles.x + 20;

            tilt = Mathf.Clamp(tilt, 350, 440);

            heading = Mathf.LerpAngle(heading, lockHeading, 5 * Time.deltaTime);

            tilt = Mathf.LerpAngle(tilt, lockTilt, 5 * Time.deltaTime);

            Distance = Mathf.Lerp(Distance, lockOnDistance, 2f * Time.deltaTime);

            transform.rotation = Quaternion.Euler(tilt, heading, 0);

            Vector3 desiredPos = target.position - transform.forward * Distance;

            transform.position = desiredPos + Vector3.up * offsetY;


        }



        void CamShake(int duration)
        {
            Vector3 originalPos = transform.localPosition;

            float elapsed = 0.0f;

            while (elapsed < duration)
            {

                float x = Random.Range(-1f, 1f);
                float y = Random.Range(-1f, 1f);

                transform.localPosition = Vector3.Lerp(transform.position, originalPos + (transform.right * x) + (transform.up * y), 10f * Time.deltaTime);

                elapsed += Time.deltaTime;

            }
        }

    }

    void TargetingList()
    {
        int i = 0;
        foreach (Transform enemy in enemiesOnScreen)
        {
            Vector3 enemyPosition = GetComponent<Camera>().WorldToViewportPoint(enemy.position);
            bool onScreen = enemyPosition.z > 0 && enemyPosition.z < 80 && enemyPosition.x > 0 && enemyPosition.x < 1 && enemyPosition.y > 0 && enemyPosition.y < 1;
            
            if (i < enemyIndex && !onScreen)
            {
                enemyIndex -= 1;
                enemiesOnScreen.Remove(enemy);
                break;
            }
            else if(i >= enemyIndex && !onScreen)
            {
                enemiesOnScreen.Remove(enemy);
                break;
            }

            i += 1;
        }
    }

    void LockOnControls()
    {
        
        if(enemyIndex > enemiesOnScreen.Count - 1)
        {
            if(enemyIndex != 0)
            {
                enemyIndex -= 1;
            }
            
        }

        if (machine.lockedOn && enemiesOnScreen.Count > 0)
        {
            //if (Input.GetKeyDown(KeyCode.X))
            //{
            //    if(enemyIndex == enemiesOnScreen.Count - 1)
            //    {
            //        enemyIndex = 0;
            //    }
            //    else
            //    {
            //        enemyIndex += 1;
            //    }
                
            //}
        }

        if (input.Current.MouseInput.magnitude == 0)
        {
            triggerInUse = false;
        }


        if (machine.lockedOn && enemiesOnScreen.Count > 0)
        {
            lockedOn = true;

            lockOnInstance.transform.position = enemiesOnScreen[enemyIndex].transform.position;

            Vector3 lockOnRotation = (lockOnInstance.transform.position - target.position);

            Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);

            lockOnInstance.transform.rotation = lookRotation;
        }
        else
        {
            Vector3 lockOnRotation = (lockOnInstance.transform.position - target.position);

            Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);

            lockOnInstance.transform.rotation = lookRotation;

            lockOnInstance.transform.position = PlayerTarget.transform.position + transform.forward * 50;
            lockedOn = false;
        }



    }

    private void HandleCollision()
    {
        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;
        _obstructionCount = Physics.SphereCastNonAlloc(PlayerTarget.transform.position, ObstructionCheckRadius, -transform.forward, _obstructions, targetDistance, ObstructionLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < _obstructionCount; i++)
        {
            bool isIgnored = false;
            for (int j = 0; j < IgnoredColliders.Count; j++)
            {
                if (IgnoredColliders[j] == _obstructions[i].collider)
                {
                    isIgnored = true;
                    break;
                }
            }
            for (int j = 0; j < IgnoredColliders.Count; j++)
            {
                if (IgnoredColliders[j] == _obstructions[i].collider)
                {
                    isIgnored = true;
                    break;
                }
            }

            if (!isIgnored && _obstructions[i].distance < closestHit.distance && _obstructions[i].distance > 0)
            {
                closestHit = _obstructions[i];
            }
        }

        // If obstructions detecter
        if (closestHit.distance < Mathf.Infinity)
        {
            _distanceIsObstructed = true;

            if (lockedOn)
            {

            }
            else
            {

            }
            Distance = Mathf.Lerp(Distance, closestHit.distance, pullbackSpeed * Time.deltaTime);
        }
        // If no obstruction
        else
        {
            _distanceIsObstructed = false;
            Distance = Mathf.Lerp(Distance, maxDistance, pullbackSpeed * Time.deltaTime);
        }

    }
}

