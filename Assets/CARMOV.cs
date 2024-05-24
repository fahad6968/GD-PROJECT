using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

    public enum Axel
    {
        Front,
        Rear
    }

    [System.Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;
    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;
    public Vector3 _centerOfMass;
    public List<Wheel> wheels;

    private Rigidbody carRb;
    private bool isSpeedBoosted = false;
    private float moveInput;
    private float steerInput;
    private Vector3 startPosition;
    private Quaternion startRotation; // Declare startRotation at the class level

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;

        // Save the initial position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;

    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
    }

    void LateUpdate()
    {
        Move(isSpeedBoosted ? 2.0f : 1.0f);
        Steer();
        Brake();
    }

    void GetInputs()
    {
        if (control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
        }
    }

    void Move(float speedMultiplier)
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime * speedMultiplier;
        }
    }

    void Steer()
    {
        float steerAngle = steerInput * turnSensitivity * maxSteerAngle;

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                // Apply the steering angle to the front wheels
                wheel.wheelCollider.steerAngle = steerAngle;
            }
        }

        // If there is no horizontal input, reset the steer angle to zero
        if (Mathf.Abs(steerInput) < 0.01f)
        {
            foreach (var wheel in wheels)
            {
                if (wheel.axel == Axel.Front)
                {
                    wheel.wheelCollider.steerAngle = 0;
                }
            }
        }

        // Adjust the car's Rigidbody's center of mass for better stability
        carRb.centerOfMass = _centerOfMass;

        // Optionally, add downforce to keep the car grounded during turns
        carRb.AddForce(-transform.up * carRb.velocity.magnitude);
    }
    void Brake()
    {
        bool isBraking = Input.GetKey(KeyCode.Space) || moveInput == 0;

        foreach (var wheel in wheels)
        {
            if (isBraking)
            {
                // Apply a strong brake torque when braking
                wheel.wheelCollider.brakeTorque = brakeAcceleration;
            }
            else
            {
                // Release the brake torque when not braking
                wheel.wheelCollider.brakeTorque = 0;
            }

            // Additionally, if braking, set motor torque to zero to stop the wheels from driving
            if (isBraking && wheel.axel == Axel.Rear)
            {
                wheel.wheelCollider.motorTorque = 0;
            }
        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("SpeedBoostCube"))
        {
            BlastCar(); // Call the method to "blast" the car
            Destroy(collision.gameObject); // Destroy the cube after collision
        }
    }

    void BlastCar()
    {
        // Destroy the car
        Destroy(gameObject);

        // Call the respawn method
        RespawnCar();
    }

    void RespawnCar()
    {
        Debug.Log("Respawning car...");
        // Instantiate a new car at the start position with the start rotation
        Instantiate(gameObject, startPosition, startRotation);
    }
}