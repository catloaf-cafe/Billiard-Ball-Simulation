using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    protected System.Random random = new System.Random();
    
    // -------------------------------------- other game objects -------------------------------------- //
    protected BallSpawnScript spawner;
    protected LogicScript logic;

    // -------------------------------------- parameters -------------------------------------- //
    [SerializeField] protected Rigidbody2D myRigidBody;

    protected Vector2 initVelocity;
    protected float positionX = 19.2f;
    protected float positionY = 10.8f;
    protected float diameter = 7.0f;

    // -------------------------------------- momentum -------------------------------------- //
    protected double curMomentum = 0f;
    public double CurMomentum
    {
        get { return curMomentum; }
    }
    protected bool hasStoredInitMomentum = false;

    // -------------------------------------- bug fix -------------------------------------- //
    protected float zeroSpeedTimeCount = 0f;

    // Start is called before the first frame update
    void Start()
    {
        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<BallSpawnScript>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        
        RegularBallSetup();
    }

    // Update is called once per frame
    void Update()
    {
        if (!logic.Paused)
        {
            CalculateMomentum();
            ResolveCornerZeroSpeedIssue();
            SpeedControl();
            OffScreen();
        }
    }

    // -------------------------------------------------------------------------------------------- //
    // -------------------------------------- custom methods -------------------------------------- //
    // -------------------------------------------------------------------------------------------- //

    protected void RegularBallSetup()
    {
        if (logic.RandomizeBallSize)
        {
            diameter -= (float)(5 * random.NextDouble()); // (2, 7]        
        }
        else
        {
            diameter -= 2.5f;
        }
        diameter *= spawner.Scaler * logic.DiameterConstant; 
        transform.localScale = Vector3.one * diameter;
        
        positionX -= 0.5f * diameter;
        positionY -= 0.5f * diameter;
        positionX -= (float) ((38.4f - diameter) * random.NextDouble()); // (-19.2, 19.2]
        positionY -= (float) ((21.6f - diameter) * random.NextDouble()); // (-10.8, 10.8]

        if (myRigidBody != null)
        {
            //myRigidBody.mass *= 2 * diameter;

            initVelocity = SetVelocity2();
            initVelocity *= spawner.Scaler;

            myRigidBody.velocity = initVelocity * logic.VelocityConstant;
            myRigidBody.position = new Vector2(positionX, positionY);
        }
    }

    protected Vector2 SetVelocity2()
    {
        float xVelocity = 30 - (float) (60 * random.NextDouble()); // (-30, 30]
        float yVelocity = 30 - (float) (60 * random.NextDouble()); // (-30, 30]

        return new Vector2(xVelocity, yVelocity) * logic.VelocityConstant;
    }

    // -------------------------------------- momentum -------------------------------------- //

    void CalculateMomentum()
    {
        curMomentum = (double) myRigidBody.velocity.magnitude * (double) myRigidBody.mass;
        if (hasStoredInitMomentum == false)
        {
            logic.InitTotalMomentum += curMomentum;
            hasStoredInitMomentum = true;
        }
        logic.CurTotalMomentum += curMomentum;
        //Debug.Log($"cur momentum ({curMomentum}) = velocity.mag ({myRigidBody.velocity.magnitude}) * mass ({myRigidBody.mass})");
        //Debug.Log($"cur total: {logic.CurTotalMomentum}; last total: {logic.LastTotalMomentum}; init total: {logic.InitTotalMomentum}; init total upper: {logic.InitTotalMomentum * 1.1f}");
    }

    void SpeedControl()
    {
        if (myRigidBody.sharedMaterial.bounciness < 1.0f)
        {
            if (logic.LastTotalMomentum < logic.InitTotalMomentum * 0.9f)
            {
                myRigidBody.velocity *= 1.1f;
                //Debug.Log($"cur total: {logic.CurTotalMomentum}; last total: {logic.LastTotalMomentum}; init total: {logic.InitTotalMomentum}; init total upper: {logic.InitTotalMomentum * 1.1f}");
                //Debug.Log("speed changed for " + this.ToString());
            }
        }
    }

    // -------------------------------------- bug fix: stuck -------------------------------------- //

    void ResolveCornerZeroSpeedIssue()
    {
        if (myRigidBody.velocity == Vector2.zero)
        {
            zeroSpeedTimeCount += Time.deltaTime; // in seconds
            if (zeroSpeedTimeCount > 0.2)
            {
                SetForce();
            }
        }
    }

    void SetForce()
    {
        float leftX = myRigidBody.position.x - diameter / 2 - 0.1f;
        float lowerY = myRigidBody.position.y - diameter / 2 - 0.1f;
        float rightX = myRigidBody.position.x + diameter / 2 + 0.1f;
        float upperY = myRigidBody.position.y + diameter / 2 + 0.1f;

        Vector2 force = Vector2.zero;

        if (leftX <= -19.2 && upperY >= 10.8) // upper left
        {
            force.x = 1;
            force.y = -1;
        }
        else if (rightX >= 19.2 && upperY >= 10.8) // upper right
        {
            force.x = -1;
            force.y = -1;
        }
        else if (leftX <= -19.2 && lowerY <= -10.8) // lower left
        {
            force.x = 1;
            force.y = 1;
        }
        else if (rightX >= 19.2 && lowerY <= -10.8) // lower right
        {
            force.x = -1;
            force.y = 1;
        }
        else // stuck at some other location
        {
            force.x = (float) random.NextDouble();
            force.y = (float) random.NextDouble();
        }
        GivePush(force);
    }

    void GivePush(Vector2 force)
    {
        force *= initVelocity.magnitude * 20 * spawner.Scaler;
        myRigidBody.AddForce(force);
    }

    // -------------------------------------- bug fix: off screen -------------------------------------- //

    protected void OffScreen()
    {
        bool boolX = transform.position.x < -19.2 || transform.position.x > 19.2;
        bool boolY = transform.position.y < -10.8 || transform.position.y > 10.8;
        if (boolX || boolY)
        {
        spawner.DeleteBall(gameObject);
        }
    }
}