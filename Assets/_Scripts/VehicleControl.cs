using Unity.VisualScripting;
using UnityEngine;

public class VehicleControl : MonoBehaviour
{
  private PlayerInput _input;

  private WorldGenerator _worldGenerator;

  public float motorTorque = 2000;
  public float brakeTorque = 2000;
  public float maxSpeed = 20;
  public float steeringRange = 30;
  public float steeringRangeAtMaxSpeed = 10;
  public float centreOfGravityOffset = -1f;
  public float buoyantForce = 18f;
  public float carHeight;

  WheelControl[] wheels;
  Rigidbody rigidBody;

  public Transform _playerSeat;
  public AudioSource _engineSound;

  public AnimationCurve _enginePitchCurve;

  [SerializeField] private GameObject _camera;

  private bool _inUse = false;

  // Start is called before the first frame update
  void Start()
  {
    _input = InputReader.Instance.PlayerInput;
    _worldGenerator = WorldGenInfo._worldGenerator;

    rigidBody = GetComponent<Rigidbody>();

    // Adjust center of mass vertically, to help prevent the car from rolling
    rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

    // Find all child GameObjects that have the WheelControl script attached
    wheels = GetComponentsInChildren<WheelControl>();
    _engineSound.enabled = false;
    Invoke("DisableRigidBody", 10f);
  }

  public void EnterVehicle() {
    _camera.SetActive(true);
    rigidBody.isKinematic = false;
    _inUse = true;
    _engineSound.enabled = true;
    _engineSound.Play();
    CancelInvoke("DisableRigidBody");
    WorldGenInfo._secondaryStructures.RemoveStructure(gameObject);
  }

  public void ExitVehicle() {
    _camera.SetActive(false);
    rigidBody.velocity = Vector3.zero;
    _inUse = false;
    _engineSound.enabled = false;
    Invoke("DisableRigidBody", 10f);
  }

  // Update is called once per frame
  void Update()
  {

    float vInput = -_input.Driving.Movement.ReadValue<Vector2>().y;
    float hInput = _input.Driving.Movement.ReadValue<Vector2>().x;
    if (!_inUse) {
      vInput = 0;
      hInput = 0;
    }
    // Calculate current speed in relation to the forward direction of the car
    // (this returns a negative number when traveling backwards)
    float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);


    // Calculate how close the car is to top speed
    // as a number from zero to one
    float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

    // Use that to calculate how much torque is available 
    // (zero torque at top speed)
    float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

    // …and to calculate how much to steer 
    // (the car steers more gently at top speed)
    float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

    // Check whether the user input is in the same direction 
    // as the car's velocity
    bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

    foreach (var wheel in wheels)
    {
      // Apply steering to Wheel colliders that have "Steerable" enabled
      if (wheel.steerable)
      {
        wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
      }
      
      if (isAccelerating)
      {
        // Apply torque to Wheel colliders that have "Motorized" enabled
        if (wheel.motorized)
        {
            wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
        }
        wheel.WheelCollider.brakeTorque = 0;
      }
      else
      {
        // If the user is trying to go in the opposite direction
        // apply brakes to all wheels
        wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
        wheel.WheelCollider.motorTorque = 0;
      }
    }

    // Update the engine sound pitch
    _engineSound.pitch = _enginePitchCurve.Evaluate(speedFactor);

    ApplyBuoyantForce();

    // If the car is not moving for 1 sec, disable the rigidbody to save performance
    if (Vector3.Distance(rigidBody.velocity, Vector3.zero) < 0.3f && !_inUse) {
      Invoke("CheckDisableRigidbody", 1f);
    }
  }

  private void ApplyBuoyantForce() {
    if (rigidBody.isKinematic) return;
    float waterLevel = 0;
    if (waterLevel == -1) return;
    float waterOffset = waterLevel - transform.position.y; // Technically negative value because reasons
    if (waterOffset < 0) return;

    float forceFactor;

    if (waterOffset > carHeight) forceFactor = 1;
    else forceFactor = (waterOffset - carHeight) / carHeight;

    rigidBody.AddForce(Vector3.up * buoyantForce * forceFactor, ForceMode.Acceleration);
  }

  private void CheckDisableRigidbody() {
    if (Vector3.Distance(rigidBody.velocity, Vector3.zero) < 0.3f && !_inUse) {
      rigidBody.isKinematic = true;
    }
  }

  private void DisableRigidBody() {
    rigidBody.isKinematic = true;
  }
}