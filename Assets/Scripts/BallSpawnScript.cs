using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEditor.Callbacks;

public class BallSpawnScript : MonoBehaviour
{   
    LogicScript logic;
    public void SetLogic()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }

    // -------------------------------------- parameters -------------------------------------- //

    int ballQuantity = 0;
    public int BallQuantity
    {
        get { return ballQuantity; }
    }

    float scaler = 1.0f; 
    public float Scaler
    {
        get { return scaler; }
    }

    // -------------------------------------- balls -------------------------------------- //
    
    [SerializeField] GameObject ball;
    [SerializeField] GameObject specialBall;
    List<GameObject> balls = new List<GameObject>();
    List<GameObject> specialBalls = new List<GameObject>();
    int ballNameCount = 0;
    int specialBallNameCount = 0;

    // -------------------------------------- input -------------------------------------- //

    int keyInputTimer = 0;
    
    // --------------------------------------------------------------------------------------------- //
    // -------------------------------------- default methods -------------------------------------- //
    // --------------------------------------------------------------------------------------------- //
    
    // Start is called before the first frame update
    void Start()
    {   
        SetLogic();
    }

    // Update is called once per frame
    void Update()
    {   
        if (!logic.Paused)
        {
            ChangeBallQuantity();
            ChangeSpecialBallQuantity();
        }
    }

    // -------------------------------------------------------------------------------------------- //
    // -------------------------------------- custom methods -------------------------------------- //
    // -------------------------------------------------------------------------------------------- //

    void AdjustScalar(int quantity) 
    {
        if (quantity >= 9)
        {   
            scaler = (float) (3 / Math.Sqrt(quantity));
        }
    }

    public void SpawnNewBall()
    {
        AdjustScalar(logic.RegularBallQuantity + logic.SpecialBallQuantity);
        if (logic.SpecialBallQuantity > 0 )
        {
            ball.GetComponent<Rigidbody2D>().sharedMaterial.bounciness = 0.85f;
            //Debug.Log("bounciness = " + 0.85);
        }
        else
        {
            ball.GetComponent<Rigidbody2D>().sharedMaterial.bounciness = 1.0f;
            //Debug.Log("bounciness = " + 1.0);
        }
        SpawnSpecialBall();
        SpawnBall();
        logic.UpdateQuantity(ballQuantity);
    }

    public void DeleteAllBall()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            Destroy(balls[i]);
        }
        for (int i = 0; i < specialBalls.Count; i++)
        {
            Destroy(specialBalls[i]);
        }
        balls.Clear();
        specialBalls.Clear();
        ballNameCount = 0;
        specialBallNameCount = 0;

        ballQuantity = 0;

        if (logic != null) 
        {
            logic.UpdateQuantity(ballQuantity);

            logic.InitTotalMomentum = 0;
            logic.LastTotalMomentum = 0;
            logic.CurTotalMomentum = 0;
        }
    }

    // -------------------------------------- special ball -------------------------------------- //

    void SpawnSpecialBall()
    {
        for (int i = 0; i < logic.SpecialBallQuantity; i++) 
        {
            AddSpecialBall();
        }
    }

    void SetColor(GameObject ob)
    {
        if (logic.ColorSpecialBall == true)
        {
            ob.GetComponent<SpriteRenderer>().color = logic.SpecialBallColor;
        }
        else
        {
            ob.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    void AddSpecialBall()
    {
        GameObject clone = Instantiate(specialBall, Vector3.zero, transform.rotation);
        SetColor(clone);
        clone.name = $"Special Ball <{specialBallNameCount}>";
        specialBalls.Add(clone);
        specialBallNameCount += 1;
        ballQuantity += 1;
        logic.UpdateQuantity(BallQuantity);
    }

    public void DeleteSpecialBall(GameObject clone)
    {
        Destroy(clone);
        specialBalls.Remove(clone);
        ballQuantity -= 1;
        logic.UpdateQuantity(ballQuantity);
    }

    // -------------------------------------- regular ball -------------------------------------- //
    
    void SpawnBall()
    {
        for (int i = 0; i < logic.RegularBallQuantity; i++)
        {
            AddBall();
        }
    }

    void AddBall()
    {
        GameObject clone = Instantiate(ball, transform.position, transform.rotation);
        clone.name = $"Regular Ball ({ballNameCount})";
        balls.Add(clone);
        ballNameCount += 1;
        ballQuantity += 1;
        logic.UpdateQuantity(ballQuantity);
    }

    public void DeleteBall(GameObject clone)
    {
        logic.InitTotalMomentum -= clone.GetComponent<BallScript>().CurMomentum;
        Destroy(clone);
        balls.Remove(clone);
        ballQuantity -= 1;
        logic.UpdateQuantity(ballQuantity);
    }

    // -------------------------------------- keyboard input: [/] -------------------------------------- //

    void ChangeSpecialBallQuantity()
    {
        ChangeSpecialBallOnPress();
        ChangeSpecialBallOnHold();
        if (Input.GetKeyUp(KeyCode.LeftBracket) == true || Input.GetKeyUp(KeyCode.RightBracket) == true)
        {
            keyInputTimer = 0;
        }
    }

    void ChangeSpecialBallOnPress()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket) == true)
        {
            AddSpecialBall();
            AdjustScalar(ballQuantity);
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket) == true)
        {
            if (specialBalls.Count != 0)
            {
                DeleteSpecialBall(specialBalls[0]); 
                AdjustScalar(ballQuantity);
            }
            else
            {
                //Debug.Log("no special balls to be destroyed (press)");
            }
        }
    }

    void ChangeSpecialBallOnHold()
    {
        if (Input.GetKey(KeyCode.RightBracket) == true)
        {
            keyInputTimer += 1;
            if (keyInputTimer > 60 && keyInputTimer % 30 == 0)
            {
                AddSpecialBall();
                AdjustScalar(ballQuantity);
            }  
        }
        if (Input.GetKey(KeyCode.LeftBracket) == true)
        {
            keyInputTimer += 1;
            if (specialBalls.Count != 0)
            {
                if (keyInputTimer > 60 && keyInputTimer % 30 == 0)
                {
                    DeleteSpecialBall(specialBalls[0]); 
                    AdjustScalar(ballQuantity);
                }
            }
            else
            {
                //Debug.Log("no special balls to be destroyed (hold)");
            }
        }
    }

    // -------------------------------------- keyboard input: -/+ -------------------------------------- //

    void ChangeBallQuantity()
    {
        ChangeBallOnPress();
        ChangeBallOnHold();
        if (Input.GetKeyUp(KeyCode.Minus) == true || Input.GetKeyUp(KeyCode.Equals) == true)
        {
            keyInputTimer = 0;
        }
    }

    void ChangeBallOnPress()
    {
        if (Input.GetKeyDown(KeyCode.Equals) == true)
        {
            AddBall();
            AdjustScalar(ballQuantity);
        }
        if (Input.GetKeyDown(KeyCode.Minus) == true)
        {
            if (balls.Count != 0)
            {
                DeleteBall(balls[0]); 
                AdjustScalar(ballQuantity);
            }
            else
            {
                //Debug.Log("no balls to be destroyed (press)");
            }
        }
    }

    void ChangeBallOnHold()
    {
        if (Input.GetKey(KeyCode.Equals) == true)
        {
            keyInputTimer += 1;
            if (keyInputTimer > 60 && keyInputTimer % 30 == 0)
            {
                AddBall();
                AdjustScalar(ballQuantity);
            }  
        }
        if (Input.GetKey(KeyCode.Minus) == true)
        {
            keyInputTimer += 1;
            if (balls.Count != 0)
            {
                if (keyInputTimer > 60 && keyInputTimer % 30 == 0)
                {
                    DeleteBall(balls[0]); 
                    AdjustScalar(ballQuantity);
                }
            }
            else
            {
                //Debug.Log("no balls to be destroyed (hold)");
            }
        }
    }
}