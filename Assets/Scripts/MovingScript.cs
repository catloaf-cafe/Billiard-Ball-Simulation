using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;

public class MovingScript : BallScript
{
    // -------------------------------------- text -------------------------------------- //
    [SerializeField] TextMeshProUGUI Label;
    bool label;
    float textPositionX;
    float textPositionY;
    
    // -------------------------------------- steering -------------------------------------- //
    Vector3 lastPosition;
    Vector3 movingDirection; // normalized directional vector
    double movingIncrement; // scalar for movingDirection; represents speed (from logic scipt)
    Vector3 positionIncrement; // change added to lastPosition

    double steeringAngle; // sum of (partitioned) steeringAngleDelta, used to determine movingDirection
    double steeringAngleDelta; // (partitioned) new change in angle
    double angleIncrement; // change added to steeringAngle (from logic scipt)
    double standardDev = 1; // used to generate steeringAngleDelta
    int steeringAngleCount = 0;

    // -------------------------------------- force -------------------------------------- //
    int forcePosOrNeg; // decides the type of force (from logic scipt)
    float forceConstant; // force strength (from logic scipt)
    float forceRadius; // (from logic scipt)
    Vector2 ballCenter;

    // --------------------------------------------------------------------------------------------- //
    // -------------------------------------- default methods -------------------------------------- //
    // --------------------------------------------------------------------------------------------- //

    // Start is called before the first frame update
    void Start()
    {
        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<BallSpawnScript>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        
        SpecialBallSetup();
    }

    // Update is called once per frame
    void Update()
    {
        if (!logic.Paused)
        {
            ballCenter = transform.position;
            CollisionDetection();
            Steering();
            ExertForce();
            AddLabel();
            OffScreen();
        }
    }

    // -------------------------------------------------------------------------------------------- //
    // -------------------------------------- custom methods -------------------------------------- //
    // -------------------------------------------------------------------------------------------- //

    void SpecialBallSetup()
    {
        RegularBallSetup();

        label = logic.LabelSpecialBall;

        movingIncrement = logic.MovingIncrement * logic.VelocityConstant * spawner.Scaler;
        angleIncrement = logic.AngleIncrement * spawner.Scaler;

        forcePosOrNeg = logic.ForcePosOrNeg;
        forceConstant = logic.ForceConstant * spawner.Scaler;
        forceRadius = logic.ForceRadius * diameter;

        lastPosition = new Vector3(positionX, positionY, 0f);
        steeringAngle = Math.PI * (1.0 - 2.0 * random.NextDouble()); // [-pi, pi)
    }

    // -------------------------------------- steering -------------------------------------- //

    void Steering()
    {
        if (steeringAngleCount == 0)
        {
            SetNewSteeringAngleDelta();
        }
        steeringAngle += steeringAngleDelta;
        //Debug.Log("steering angle delta: " + steeringAngleDelta);
        steeringAngleCount -= 1;
        KeepAngleInRange();
        
        movingDirection.x = (float) Math.Cos(steeringAngle);
        movingDirection.y = (float) Math.Sin(steeringAngle);
        
        positionIncrement = movingDirection * (float) movingIncrement * Time.timeScale;
        //Debug.Log("steering angle: " + steeringAngle);
        KeepPositionInBoundary();

        transform.position = lastPosition + positionIncrement;
        lastPosition = transform.position;
    }

    void KeepPositionInBoundary()
    {
        if ((lastPosition + positionIncrement).x > 19.2f - 0.5f * diameter || 
            (lastPosition + positionIncrement).x < -19.2f + 0.5f * diameter)
        {
            positionIncrement.x *= -1f;
            steeringAngle = Math.PI - steeringAngle;
            steeringAngleCount = 0;
        }
        if ((lastPosition + positionIncrement).y > 10.8f - 0.5f * diameter || 
            (lastPosition + positionIncrement).y < -10.8f + 0.5f * diameter)
        {
            positionIncrement.y *= -1f;
            steeringAngle = -steeringAngle;
            steeringAngleCount = 0;
        }
    }

    void KeepAngleInRange()
    {
        if (steeringAngle < -Math.PI)
        {
            steeringAngle = 2.0 * Math.PI - steeringAngle;
        }
        else if (steeringAngle >= Math.PI)
        {
            steeringAngle = steeringAngle - 2.0 * Math.PI;
        }
    }

    void SetNewSteeringAngleDelta()
    {
        steeringAngleDelta = NormalDistribution(0, standardDev);
        //Debug.Log("steering angle delta initialized: " + steeringAngleDelta);
        if (Math.Abs(steeringAngleDelta) > Math.PI)
        {
            steeringAngleDelta = Math.PI;
            //Debug.Log("steering angle delta changed to pi: " + steeringAngleDelta);
        }
        if (Math.Abs(steeringAngleDelta) > angleIncrement)
        {
            steeringAngleCount = (int) Math.Abs(Math.Round(steeringAngleDelta / angleIncrement, 0)); 
            steeringAngleDelta /= steeringAngleCount;
            //Debug.Log("steering angle delta divided: " + steeringAngleDelta);
            //Debug.Log("steering angle count: " + steeringAngleCount);
        }
        else
        {
            steeringAngleCount = 1;
            //Debug.Log("steering angle delta not divided: " + steeringAngleDelta);
            //Debug.Log("steering angle count: " + steeringAngleCount);
        }
        //Debug.Log("---------------- " + Time.frameCount + " ----------------");
    }

    double NormalDistribution(double mean, double stdDev)
    {
        double u1 = random.NextDouble(); // (0, 1]
        double u2 = random.NextDouble(); // (0, 1]
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * 
                                Math.Sin(2.0 * Math.PI * u2); // N(0, 1)
        double randNormal = mean + stdDev * randStdNormal; // N(mean, stdDev^2)
        return randNormal;
    }

    // -------------------------------------- force -------------------------------------- //
    void ExertForce()
    {
        if (forcePosOrNeg == 1 || forcePosOrNeg == -1)
        {
            Collider2D[] balls = Physics2D.OverlapCircleAll(ballCenter, forceRadius);
            foreach (Collider2D ball in balls)
            {
                Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 displacement = rb.position - ballCenter;
                    Vector2 forceDirection = displacement.normalized;
                    float forceStrength = (float) (forceConstant * rb.mass / Math.Pow(displacement.magnitude, 2));

                    Vector2 finalForce = forceDirection * forceStrength * forcePosOrNeg;
                    rb.AddForce(finalForce);
                    //Debug.Log("ball pushed with force " + finalForce.magnitude);
                }
            }
        }
        else if (forcePosOrNeg != 0)
        {
            forcePosOrNeg = 0;
            Debug.LogWarning("the value for force type is not allowed, hence is set to 0");
        }
    }

    // -------------------------------------- collision -------------------------------------- //

    void CollisionDetection() 
    {   
        Collider2D[] balls = Physics2D.OverlapCircleAll(ballCenter, diameter * 1.5f);
        foreach (Collider2D ball in balls)
        {
            if (ball.name.StartsWith("Special Ball") && ball.name != this.name)
            {
                float thatBallX = ball.gameObject.GetComponent<MovingScript>().ballCenter.x;
                float thatBallY = ball.gameObject.GetComponent<MovingScript>().ballCenter.y;
                Vector2 thatToThis = new Vector2(ballCenter.x - thatBallX, ballCenter.y - thatBallY).normalized;

                double newAngle = Math.Atan2(thatToThis.y, thatToThis.x);
                steeringAngle = newAngle;
                steeringAngleCount = 0;
            }
        }
    }

    // -------------------------------------- label -------------------------------------- //

    void AddLabel()
    {
        if (Label != null && label)
        {
            textPositionX = (transform.position.x + 19.2f) * 50f;
            textPositionY = (transform.position.y + 10.8f) * 50f;
            Label.text = name;
            Label.transform.position = new Vector3(textPositionX, textPositionY, 0);
        }
    }

    new void OffScreen()
    {
        bool boolX = transform.position.x < -19.2 || transform.position.x > 19.2;
        bool boolY = transform.position.y < -10.8 || transform.position.y > 10.8;
        if (boolX || boolY)
        {
        spawner.DeleteSpecialBall(gameObject);
        }
    }
}