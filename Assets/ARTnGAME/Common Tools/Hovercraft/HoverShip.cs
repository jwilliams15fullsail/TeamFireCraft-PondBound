using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Artngame.Oceanis {
    [RequireComponent (typeof (Rigidbody))]
    public class HoverShip : MonoBehaviour {

        //v0.1
        public GlobalOceanisControllerURP ocean;

        [Header ("Ship handling")]
        [SerializeField] private float _fwdAccel = 50f; //accelaterion
        [SerializeField] private float _fwdMaxSpeed = 200f; //max speed without boosting
        private float _oldFwdMaxSpeed;
        [SerializeField] private float _maxBoost = 300f; //max speed with boosting
        [SerializeField] private float _brakeSpeed = 70f;
        [SerializeField] private float _turnSpeed = 10f;
        [SerializeField] private float _turnSpeedInAir = 3f;
        private float _input = 0f; //horizontal axis
        [SerializeField] private float _currentSpeed;
        private bool _isDead;
        private bool _canBoost;
        private Transform _startPos;
        [Header ("Fuel")]
        [SerializeField] private float _maxFuel = 100f;
        [SerializeField] private float _fuelConsumption = 1f;
        [SerializeField] private float _currentFuel;
        [Header ("Hovering")]
        [SerializeField] private LayerMask groundLayer; //objects we want to be able to hover on
        private Rigidbody _rb;
        private Vector3 previousUpDir; //stores transform.up
        [SerializeField] private float _hoverHeight = 3f; //Distance from the ground
        [SerializeField] private float _heightSmoothing = 10f; //How fast the ship will readjust to "hoverHeight"
        [SerializeField] private float _normalRotSmoothing = 50f; //How fast the ship will adjust its rotation to match ground normal
        private float _smoothY = 1f;
        [SerializeField] private float _startDelay = 0.2f;
        private bool _isGrounded; //Also for modelVFX
        [Header ("Dropping")]
        [SerializeField] private float _dropOffTime = 0.2f;
        private bool _isDroppingOff;
        private float _oldDropOffTime;
        [SerializeField] private float _rotationLerp = 7f; //how fast we will rotate towards the ground if ship is in the air
        [SerializeField] private float _positionLerp = 5f; //how fast we will sink down if ship is in the air
        private float _oldPositionLerp;
        private bool _dropOffForceAdded;

        private void Start () {
            //ship handling
            _currentSpeed = 0f;
            _oldFwdMaxSpeed = _fwdMaxSpeed;
            _canBoost = true;
            _isDead = false;
            _startPos = transform;
            //fuel
            _currentFuel = _maxFuel;
            //hovering
            _rb = GetComponent<Rigidbody> ();
            _rb.useGravity = false;
            _isGrounded = true;
            //dropping
            _isDroppingOff = false;
            _oldDropOffTime = _dropOffTime;
            _oldPositionLerp = _positionLerp;
            _dropOffForceAdded = false;

            //v0.1
            heightSampler = this.transform;
        }

        //v0.1
        public bool castFloaters = false;
        Vector3 prev_pos;
        float CurrentRot;
        //v0.1
        public Transform heightSampler;

        private void Update () {



            //v0.1
            if (ocean != null)
            {
                //v0.1
                if (heightSampler != null)
                {
                    Vector3 displacment = ocean.GetWaterDisplacement(heightSampler.position);
                    if (Mathf.Abs(displacment.y) < 40)
                    {
                        Vector3 heightSampler_position = new Vector3(heightSampler.position.x + displacment.x,
                                                                        ocean.GetWaterHeight(heightSampler.position),
                                                                        heightSampler.position.z + +displacment.z);
                        //                heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler_position, Time.deltaTime * ocean.lerpSpeed);
                        //                heightSampler.position = new Vector3(heightSampler.position.x, ocean.GetWaterHeight(heightSampler.transform.position), heightSampler.position.z);

                        //heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler_position, Time.deltaTime * ocean.lerpSpeed);
                        //               heightSampler.position = new Vector3(heightSampler.position.x, ocean.GetWaterHeight(heightSampler.transform.position), heightSampler.position.z);

                    }
                    //Debug.Log("heightSampler.position.y = " + heightSampler.position.y + " displacment.y = " + displacment.y);
                    if ((heightSampler.position.y - displacment.y) > 0 && (heightSampler.position.y - displacment.y) < 3)
                    {
                        // heightSampler.position = new Vector3(heightSampler.position.x, ocean.GetWaterHeight(heightSampler.transform.position), heightSampler.position.z);
                        // _isDroppingOff = false;
                    }

                    Vector3 MotionDirection = (heightSampler.position - prev_pos).normalized;
                    //Debug.DrawLine(heightSampler.position, heightSampler.position + MotionDirection* 5,Color.red,1);
                    //                 heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    //                    Quaternion.AngleAxis(-100 * MotionDirection.y, (Vector3.up + MotionDirection)),
                    //                     Time.deltaTime * ocean.lerpRotSpeed * 0.05f);

                    //Sample boat back and front !!!
                    Vector3 frontPos = heightSampler.position + heightSampler.forward * ocean.boatLength / 2;
                    Vector3 backPos = heightSampler.position - heightSampler.forward * ocean.boatLength / 2;

                    Debug.DrawLine(frontPos, frontPos + Vector3.up * 4, Color.red, 1);
                    Debug.DrawLine(backPos, backPos + Vector3.up * 4, Color.red, 1);
                    float frontH = ocean.GetWaterHeight(frontPos);
                    float backH = ocean.GetWaterHeight(backPos);
                    float diffBF = frontH - backH;
                    //                heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                    //                   Quaternion.AngleAxis(diffBF, heightSampler.right),
                    //                    Time.deltaTime * ocean.lerpRotSpeed * 0.05f);
                    Debug.DrawLine(new Vector3(frontPos.x, frontH, frontPos.z), new Vector3(frontPos.x, frontH, frontPos.z) + Vector3.up * 4, Color.green, 2);
                    Debug.DrawLine(new Vector3(backPos.x, backH, backPos.z), new Vector3(backPos.x, backH, backPos.z) + Vector3.up * 4, Color.green, 2);
                    Vector3 waterGradient = new Vector3(frontPos.x, frontH, frontPos.z) - new Vector3(backPos.x, backH, backPos.z);
                    if (1 == 0 && ocean.controlBoat)
                    {
                        Vector3 Forward_flat = new Vector3(heightSampler.forward.x, 0, heightSampler.forward.z);

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                        Vector2 moveA =
                         Keyboard.current != null
                         ? new Vector2(
                             (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                             (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
                           )
                         : Vector2.zero;

                        heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler.position + new Vector3(waterGradient.x * 1 * moveA.y,
                            0, waterGradient.z * 1 * moveA.y), Time.deltaTime * ocean.BoatSpeed);

                        CurrentRot = Mathf.Lerp(CurrentRot, CurrentRot + moveA.x, Time.deltaTime * ocean.BoatRotSpeed);

#else
                        heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler.position + new Vector3(waterGradient.x * 1 * Input.GetAxis("Vertical"),
                            0, waterGradient.z * 1 * Input.GetAxis("Vertical")), Time.deltaTime * ocean.BoatSpeed);

                        CurrentRot = Mathf.Lerp(CurrentRot, CurrentRot + Input.GetAxis("Horizontal"), Time.deltaTime * ocean.BoatRotSpeed);
#endif

                        heightSampler.rotation = Quaternion.Lerp(heightSampler.rotation,
                             Quaternion.AngleAxis(CurrentRot, Vector3.up) * Quaternion.AngleAxis(diffBF * 2, heightSampler.right),
                             Time.deltaTime * ocean.BoatRotSpeed);
                    }
                    //------------------------------------------------FLOATERS
#region Floaters
                    if (castFloaters)
                    {
                        //HDRP 1 - sample randomly to add wave splashes
                        if (ocean.waterSpray != null)
                        {
                            if (ocean.waterSprays.Count == 0)
                            { //create 6 instances
                                for (int i = 0; i < ocean.maxSplashes; i++)
                                {
                                    GameObject splash = Instantiate(ocean.waterSpray);
                                    ParticleSystem[] partsA = splash.GetComponentsInChildren<ParticleSystem>(true);
                                    if (partsA.Length > 0)
                                    {
                                        partsA[0].gameObject.SetActive(false); //and disable them
                                    }
                                    ocean.waterSprays.Add(splash);
                                }
                            }
                            for (int i = 0; i < ocean.waterSprays.Count; i++)
                            {
                                //sample a random placement for the first disabled
                                if (!ocean.waterSprays[i].activeInHierarchy)
                                {
                                    float aheadDistX = 4 * Random.Range(ocean.splashDistMin, ocean.splashDistMax);
                                    float aheadDistZ = 4 * Random.Range(ocean.splashDistMin, ocean.splashDistMax);
                                    float PosX1 = Camera.main.transform.position.x + Camera.main.transform.forward.x * aheadDistX;
                                    float PosZ1 = Camera.main.transform.position.z + Camera.main.transform.forward.z * aheadDistZ;

                                    PosX1 = PosX1 + Camera.main.transform.right.x * Random.Range(-ocean.splashDistMin, ocean.splashDistMax) * 2;
                                    PosZ1 = PosZ1 + Camera.main.transform.right.z * Random.Range(-ocean.splashDistMin, ocean.splashDistMax) * 2;

                                    float PosY1 = Camera.main.transform.position.y + Camera.main.transform.forward.y;
                                    //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                                    Vector3 cameraPos = new Vector3(PosX1, PosY1, PosZ1);
                                    Vector3 displacementTO = ocean.GetWaterDisplacement(cameraPos);
                                    Vector3 GerstnerOffsets1 = new Vector3(cameraPos.x + displacementTO.x,
                                                                                        ocean.GetWaterHeight(cameraPos),
                                                                                        cameraPos.z + +displacementTO.z);
                                    ocean.waterSprays[i].transform.position =
                                        new Vector3(GerstnerOffsets1.x * 1 + PosX1,
                                        GerstnerOffsets1.y * ocean.heightFactor1 + ocean.heightOffsetY,
                                        GerstnerOffsets1.z * 1 + PosZ1);
                                    ocean.waterSprays[i].SetActive(true);
                                    //v0.6 - splash
                                    ParticleSystem[] parts = ocean.waterSprays[i].GetComponentsInChildren<ParticleSystem>(true);
                                    if (parts.Length > 0)
                                    {
                                        parts[0].gameObject.SetActive(true);
                                    }
                                    break;//stop here
                                }
                                else //if stopped play, disable again                        
                                {
                                    ParticleSystem[] parts = ocean.waterSprays[i].GetComponentsInChildren<ParticleSystem>(true);
                                    if (parts.Length > 0)
                                    {
                                        if (!parts[0].isPlaying)
                                        {
                                            parts[0].gameObject.SetActive(false);
                                            ocean.waterSprays[i].SetActive(false);
                                        }
                                    }
                                }
                            }
                        }
                        for (int i = ocean.WaterObjects.Count - 1; i >= 0; i--)
                        {
                            if (ocean.WaterObjects[i] == null)
                            {
                                ocean.WaterObjects.RemoveAt(i);
                                ocean.WaterObjectsWaterPos.RemoveAt(i);
                                ocean.WaterObjectsStartPos.RemoveAt(i);
                            }
                        }
                        for (int i = ocean.ThrowObjects.Count - 1; i >= 0; i--)
                        {
                            if (ocean.ThrowObjects[i] == null)
                            {
                                ocean.ThrowObjects.RemoveAt(i);
                                ocean.ThrowObjectsWaterPos.RemoveAt(i);
                                ocean.ThrowObjectsStartPos.RemoveAt(i);
                            }
                        }
                        for (int i = 0; i < ocean.WaterObjects.Count; i++)
                        {
                            float PosX1 = ocean.WaterObjects[i].position.x;
                            float PosZ1 = ocean.WaterObjects[i].position.z;
                            float PosY1 = ocean.WaterObjects[i].position.y;
                            //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                            Vector3 displacementTO = ocean.GetWaterDisplacement(ocean.WaterObjects[i].position);
                            Vector3 GerstnerOffsets1 = new Vector3(ocean.WaterObjects[i].position.x + displacementTO.x,
                                                                                ocean.GetWaterHeight(ocean.WaterObjects[i].position),
                                                                                ocean.WaterObjects[i].position.z + displacementTO.z);
                            if (ocean.ShiftHorPositionFloaters)
                            {
                                ocean.WaterObjectsWaterPos[i] = new Vector3(displacementTO.x * ocean.ShiftHorPositionFactor.x,
                                    GerstnerOffsets1.y * ocean.heightFactor1 + ocean.heightOffsetY,
                                    displacementTO.z * ocean.ShiftHorPositionFactor.z);
                            }
                            else
                            {
                                ocean.WaterObjectsWaterPos[i] = new Vector3(0, GerstnerOffsets1.y * ocean.heightFactor1 + ocean.heightOffsetY, 0);
                            }

                            if (ocean.alignToNormalFloaters)
                            {
                                ocean.WaterObjects[i].up = new Vector3((Mathf.Abs(displacementTO.x) - ocean.alignToNormalFactor.x),
                                    (Mathf.Abs(displacementTO.y) - ocean.alignToNormalFactor.y),
                                    (Mathf.Abs(displacementTO.z) - ocean.alignToNormalFactor.z));
                            }
                        }
                        for (int i = 0; i < ocean.WaterObjectsWaterPos.Count; i++)
                        {
                            if (ocean.LerpMotion)
                            {
                                ocean.WaterObjects[i].position = Vector3.Lerp(ocean.WaterObjects[i].position,
                                    new Vector3(ocean.WaterObjectsStartPos[i].x,
                                     - 0.45f, ocean.WaterObjectsStartPos[i].z) + ocean.WaterObjectsWaterPos[i],
                                    Time.deltaTime * ocean.lerpSpeed);
                            }
                            else
                            {
                                ocean.WaterObjects[i].position = new Vector3(ocean.WaterObjectsStartPos[i].x,
                                    -0.45f,//this.transform.position.y - 0.45f, //v0.1
                                    ocean.WaterObjectsStartPos[i].z) + ocean.WaterObjectsWaterPos[i];
                            }
                        }
                        for (int i = 0; i < ocean.ThrowObjects.Count; i++)
                        {
                            float PosX1 = ocean.ThrowObjects[i].position.x;
                            float PosZ1 = ocean.ThrowObjects[i].position.z;
                            float PosY1 = ocean.ThrowObjects[i].position.y;
                            //Vector3 GerstnerOffsets1 = GerstnerOffset(new Vector2(PosX1, PosZ1), PosY1, new Vector2(PosX1, PosZ1));
                            Vector3 displacementTO = ocean.GetWaterDisplacement(ocean.ThrowObjects[i].position);
                            Vector3 GerstnerOffsets1 = new Vector3(ocean.ThrowObjects[i].position.x + displacementTO.x,
                                                                                ocean.GetWaterHeight(ocean.ThrowObjects[i].position),
                                                                                ocean.ThrowObjects[i].position.z + +displacementTO.z);
                            ocean.ThrowObjectsWaterPos[i] = new Vector3(GerstnerOffsets1.x, GerstnerOffsets1.y * ocean.heightFactor1, GerstnerOffsets1.z);
                        }
                        for (int i = 0; i < ocean.ThrowObjectsWaterPos.Count; i++)
                        {
                            //if (ocean.ThrowObjectsWaterPos[i].y + this.transform.position.y > ocean.ThrowObjects[i].position.y) //v0.1
                            if (ocean.ThrowObjectsWaterPos[i].y + 0 > ocean.ThrowObjects[i].position.y)
                            {
                                //v0.6 - splash
                                ParticleSystem[] parts = ocean.ThrowObjects[i].GetComponentsInChildren<ParticleSystem>(true);
                                if (parts.Length > 0)
                                {
                                    parts[0].gameObject.SetActive(true);
                                }
                                //add to water list
                                ocean.ThrowObjects[i].GetComponent<Rigidbody>().isKinematic = true;
                                ocean.WaterObjects.Add(ocean.ThrowObjects[i]);
                                ocean.WaterObjectsStartPos.Add(ocean.ThrowObjects[i].position);
                                ocean.WaterObjectsWaterPos.Add(ocean.ThrowObjectsWaterPos[i]);
                                ocean.ThrowObjectsWaterPos.RemoveAt(i);
                                ocean.ThrowObjects.RemoveAt(i);
                                ocean.ThrowObjectsStartPos.RemoveAt(i);
                            }
                        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                        if (
                            Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame
                        )
                        {
#else
                        if (Input.GetMouseButtonDown(1))
                        { //v3.4
#endif
                            if (ocean.ThrowItem != null)
                            {
                                GameObject TEMP = (GameObject)Instantiate(ocean.ThrowItem, 
                                    heightSampler.transform.position + heightSampler.transform.forward * 22 + heightSampler.transform.up *6, //new Vector3(0, 5, 0), //v0.1 
                                    Quaternion.identity);
                                TEMP.transform.localScale = TEMP.transform.localScale * 5;
                                ocean.ThrowObjects.Add(TEMP.transform);
                                ocean.ThrowObjectsWaterPos.Add(new Vector3(0, 0, 0));
                                ocean.ThrowObjectsStartPos.Add(TEMP.transform.position);
                                if (TEMP != null)
                                {
                                    Rigidbody RGB = TEMP.GetComponent<Rigidbody>() as Rigidbody;
                                    if (RGB != null)
                                    {
                                        if (Camera.main != null)
                                        {
                                            RGB.AddForce(Camera.main.transform.forward * ocean.ThrowPower + Camera.main.transform.up * ocean.ThrowPower/10);//v0.1 
                                        }
                                        else
                                        {
                                            RGB.AddForce(heightSampler.transform.forward * ocean.ThrowPower);
                                        }
                                    }
                                }
                            }
                        }
                        //HDRP
                        Vector3 Velocity = (heightSampler.transform.position - prev_pos) / Time.deltaTime;

                        //HDRP - check collisions with boat
                        for (int i = 0; i < ocean.WaterObjects.Count; i++)
                        {
                            float distBoatBall = Vector3.Distance(ocean.WaterObjects[i].position, heightSampler.transform.position);
                            if (distBoatBall < 6/3)//v0.1 
                            {
                                ocean.WaterObjects[i].GetComponent<Rigidbody>().isKinematic = false;
                                ocean.ThrowObjects.Add(ocean.WaterObjects[i]);
                                ocean.ThrowObjectsStartPos.Add(ocean.WaterObjects[i].position);
                                ocean.ThrowObjectsWaterPos.Add(ocean.WaterObjectsWaterPos[i]);

                                ocean.WaterObjectsWaterPos.RemoveAt(i);
                                ocean.WaterObjects.RemoveAt(i);
                                ocean.WaterObjectsStartPos.RemoveAt(i);

                                Rigidbody RGB = ocean.ThrowObjects[ocean.ThrowObjects.Count - 1].GetComponent<Rigidbody>() as Rigidbody;
                                if (RGB != null)
                                {
                                    if (Camera.main != null)
                                    {
                                        //RGB.AddForce(Camera.main.transform.forward * ThrowPower);
                                        RGB.AddForce((Vector3.up * (0.1f + 0.12f * Random.Range(0.0f, 1.0f)) + 0.0075f * new Vector3(Velocity.x, 0, Velocity.z)) * ocean.ThrowPower);
                                    }
                                    else
                                    {
                                        RGB.AddForce(heightSampler.transform.forward * ocean.ThrowPower);
                                    }
                                }
                            }
                        }
                    }
#endregion
                    //---------------------------------------------------END FLOATERS
                    prev_pos = heightSampler.position;
                }
            }





#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (
                (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame) ||
                (Gamepad.current != null &&
                 Gamepad.current.buttonSouth.wasPressedThisFrame)
            )
            {
                DropOff();
            }
#else
            if (Input.GetButtonDown("Fire1"))
            {
                //if (Input.GetButtonDown ("X Button")) {
                DropOff();
            }
#endif

            if (_startDelay >= 0f)
                _startDelay -= Time.deltaTime;


#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Vector2 move =
             Keyboard.current != null
             ? new Vector2(
                 (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                 (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
               )
             : Vector2.zero;

            _input = move.x;
#else
            _input = Input.GetAxis ("Horizontal");
#endif

            if (_startDelay <= 0)
                DrivingBehaviour ();

        }

        private void DrivingBehaviour () {
            if (_isDead) {
                //Stop ship movement if dead
                _currentSpeed = 0;
                _rb.linearVelocity = Vector3.zero;
                GetComponent<HoverShip> ().enabled = false;
                //GetComponentInChildren<MeshRenderer>().enabled = false;
                transform.gameObject.isStatic = true;
                return;
            }
            if (!_isDroppingOff) {
                //can boost while on ground
                _canBoost = true;
                //user inputs
                if (HasInput ()) {
                    //moving
                    if (!IsBoosting ()) {
                        _fwdMaxSpeed = _oldFwdMaxSpeed;
                        Accelerate (_fwdAccel);
                    } else {
                        _fwdMaxSpeed = _maxBoost;
                        Accelerate (_fwdAccel * 2);
                    }
                    //brake if we over shoot our max speed
                    if (_currentSpeed >= _fwdMaxSpeed) {
                        Brake (_brakeSpeed);
                    }
                } else {
                    //no input, so brake
                    Brake (_brakeSpeed);
                }
                //set velocity to zero if not moving and if not dropping of
                if (_currentSpeed <= 0f && _isGrounded) {
                    _rb.linearVelocity = Vector3.zero;
                }
                //get current up dir for fixedUpdate calculations
                previousUpDir = transform.up;
            } else {
                //if dropping off stop boosting
                _canBoost = false;

                //add little force to velocity once we take off
                if (!_dropOffForceAdded) {
                    _rb.linearVelocity += new Vector3 (0, 250, 0);
                    _dropOffForceAdded = true;
                }
                //start brake, so we can't lfy in the air
                //also decrease _brakespeed a little amount so we can glide a little longer
                Brake (_brakeSpeed / 1.5f);

                //measure time we are in the air
                _dropOffTime -= Time.deltaTime;
            }

        }

        private void FixedUpdate () {
            if (_isDead) return;

            //turning behaviour
            if (IsTurning ()) {
                if (_isDroppingOff) {
                    TurnShip (_turnSpeedInAir);
                } else {
                    TurnShip (_turnSpeed);
                }
            } else {
                //if we don't turn, then stop the angular rotation
                _rb.angularVelocity = Vector3.Lerp (_rb.angularVelocity, Vector3.zero, 0.05f * Time.fixedDeltaTime);
            }

                       
            Vector3 displacementB = ocean.GetWaterDisplacement(transform.position);
            float distB = transform.position.y - displacementB.y;
            if (distB <= 0)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, displacementB.y + _hoverHeight, transform.localPosition.z);
            }


            //if we are not in the air
            if (!_isDroppingOff) {
                //Normal alignment
                RaycastHit hit;
                if (Physics.Raycast (transform.position, -previousUpDir, out hit, 10f, groundLayer)) {
                    //we hit the ground
                    _isGrounded = true;
                    //smooth new up vector
                    Vector3 desiredUp = Vector3.Lerp (previousUpDir, hit.normal, Time.deltaTime * _normalRotSmoothing);
                    //get the angle
                    Quaternion tilt = Quaternion.FromToRotation (transform.up, desiredUp) * transform.rotation;
                    //apply rotation
                    transform.localRotation = tilt;
                    //Smoothly adjust height
                    _smoothY = Mathf.Lerp (_smoothY, _hoverHeight - hit.distance, Time.deltaTime * _heightSmoothing);

                    _smoothY = _smoothY <= 0.01f ? 0f : _smoothY;
                    transform.localPosition += previousUpDir * _smoothY;
                } else {
                    
                    if (ocean != null)
                    {
                        //v0.1                       
                        Vector3 displacement = ocean.GetWaterDisplacement(transform.position);
                        //Debug.Log("displacement = " + displacement.y);
                        //displacement = displacement + transform.position;
                        //if ((Mathf.Abs(displacement.y) > 0 && Mathf.Abs(displacement.y) < 10) || displacement.y > 0)
                        float dist = transform.position.y - Mathf.Abs(displacement.y);
                        if (dist > 0)
                        {
                            _isGrounded = true;
                            Vector3 desiredUp = Vector3.Lerp(previousUpDir, Vector3.up, Time.deltaTime * _normalRotSmoothing);
                            //get the angle
                            Quaternion tilt = Quaternion.FromToRotation(transform.up, desiredUp) * transform.rotation;
                            //apply rotation
                            transform.localRotation = tilt;
                            //Smoothly adjust height
                            _smoothY = Mathf.Lerp(_smoothY, _hoverHeight - dist, Time.deltaTime * _heightSmoothing);
                            _smoothY = _smoothY <= 0.01f ? 0f : _smoothY;
                            transform.localPosition += previousUpDir * _smoothY;
                        }
                        else
                        {
                            //if we don't hit anything, that means we are in the air
                            DropOff();
                        }
                    }
                    else
                    {
                        //if we don't hit anything, that means we are in the air
                        DropOff();
                    }

                }
            } else {
                //if we are in the air
                RaycastHit rotationHit;
                bool foundHit = false;
                //rotate towards ground normal
                if (Physics.Raycast (transform.position, Vector3.down * 50, out rotationHit, groundLayer)) {
                    Vector3 desiredUp = Vector3.Lerp (transform.up, Vector3.up, Time.deltaTime * _normalRotSmoothing);
                    //get the angle
                    Quaternion tilt = Quaternion.FromToRotation (transform.up, desiredUp) * transform.rotation;
                    //apply rotation
                    transform.localRotation = Quaternion.Lerp (transform.localRotation, tilt, _rotationLerp * Time.deltaTime);
                    foundHit = true;
                }

                //v0.1
                if (ocean != null && !foundHit)
                {
                    Vector3 displacement = ocean.GetWaterDisplacement(transform.position);
                    //displacement = displacement + transform.position;
                    float dist = transform.position.y - Mathf.Abs(displacement.y);
                    //if (Mathf.Abs(displacement.y) >=10 && Mathf.Abs(displacement.y) < 50)
                    if(dist >= 10 && dist < 50)
                    {
                        Vector3 desiredUp = Vector3.Lerp(transform.up, Vector3.up, Time.deltaTime * _normalRotSmoothing);
                        //get the angle
                        Quaternion tilt = Quaternion.FromToRotation(transform.up, desiredUp) * transform.rotation;
                        //apply rotation
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, tilt, _rotationLerp * Time.deltaTime);
                    }
                }


                //only sink for a given amount of time so we don't get extremly high values
                if (_dropOffTime <= 0) {
                    //increase lerp value over time, so we sink faster and faster
                    _positionLerp += 10 * Time.deltaTime;
                    //start sinking
                    transform.localPosition += Vector3.down * 9.81f * _positionLerp * Time.deltaTime;
                    RaycastHit hit;
                    //check distance to ground
                    bool foundHit2 = false;
                    if (Physics.Raycast (transform.position, Vector3.down * 50, out hit, groundLayer)) {
                        //if we are at our hoverheight again
                        if (hit.distance <= _hoverHeight) {
                            _dropOffTime = _oldDropOffTime;
                            //reset bool for next drop off
                            _dropOffForceAdded = false;
                            _positionLerp = _oldPositionLerp;
                            //get back on the ground
                            DropOn ();

                            foundHit2 = true;
                        }
                    }

                    //v0.1
                    if (ocean != null && !foundHit2)
                    {
                        Vector3 displacementA = ocean.GetWaterDisplacement(transform.position);
                        float distA = transform.position.y - Mathf.Abs(displacementA.y);
                        //displacementA = displacementA + transform.position;
                        //if ((displacementA.y- transform.position.y) < 0 && Mathf.Abs(displacementA.y) <= _hoverHeight)
                        if (distA <= _hoverHeight)
                        {
                            _dropOffTime = _oldDropOffTime;
                            //reset bool for next drop off
                            _dropOffForceAdded = false;
                            _positionLerp = _oldPositionLerp;
                            //get back on the ground
                            DropOn();
                        }
                    }


                }
            }

            //now actually move the ship
            MoveShip (_currentSpeed * 80);

        }

        private bool IsTurning () {


#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Vector2 move =
             Keyboard.current != null
             ? new Vector2(
                 (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                 (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
               )
             : Vector2.zero;

            return (move.x > 0 || move.x < 0);
#else
            return (Input.GetAxis ("Horizontal") > 0 || Input.GetAxis ("Horizontal") < 0);
#endif

        }

        private void TurnShip (float speed) {
            _rb.AddTorque (previousUpDir * speed * _input * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        private bool IsMoving () {
            return (_currentSpeed > 0);
        }

        private void MoveShip (float speed) {
            //move ship forward with current speed
            _rb.linearVelocity = transform.forward * speed * Time.fixedDeltaTime;

        }

        private void Brake (float brakeSpeed) {
            if (_currentSpeed > 0) {
                //decrease only if it's greater than 0, so we don't start flying backwards
                _currentSpeed -= brakeSpeed * Time.deltaTime;
            } else {
                //set speed to 0 if we can't brake any more
                _currentSpeed = 0f;
            }
        }

        private void Accelerate (float speed) {
            //stop acceleration if we are out of fuel
            if (_currentFuel <= 0f) {
                _currentSpeed = Mathf.Lerp (_currentSpeed, 0f, 1f * Time.deltaTime);
                return;
            }
             //if current speed is greater than max speed, only add 0f so we don't get faster and faster
            //else apply our acceleration
            _currentSpeed += (_currentSpeed >= _fwdMaxSpeed) ? 0f : speed * Time.deltaTime;
            //reduce fuel
            _currentFuel -= IsBoosting () ? _fuelConsumption * 2 * Time.deltaTime : _fuelConsumption * Time.deltaTime;
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private bool HasInput()
        {
            return Keyboard.current != null &&
                   (Keyboard.current.wKey.isPressed ||
                    Keyboard.current.upArrowKey.isPressed);
        }
        private bool IsBoosting()
        {
            return Keyboard.current != null &&
                   Keyboard.current.spaceKey.isPressed &&
                   _canBoost;
        }
#else
          private bool HasInput () {
            return (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
            //return (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow) || Input.GetAxis ("Right Trigger") > 0);
        }

        private bool IsBoosting () {
            //only boost if we get correct inputs and if we can boost
            // return ((Input.GetKey (KeyCode.Space) || Input.GetButton ("A Button")) && _canBoost);
            return ((Input.GetKey(KeyCode.Space) ) && _canBoost);
        }
#endif

        private void Die () {
            _isDead = true;
        }

        private void DropOff () {
            if (!_isDroppingOff) {
                //not on ground any more
                _isGrounded = false;
                _isDroppingOff = true;
            }
        }

        private void DropOn () {
            //again on ground
            _isGrounded = true;
            _isDroppingOff = false;
        }

    }
}
