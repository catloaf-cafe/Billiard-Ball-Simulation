using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Configuration;
using Unity.VisualScripting;

public enum ForceType
{
    None, 
    Repel,
    Attract
}

public class LogicScript : MonoBehaviour
{
    [SerializeField] GameObject spawnerObject;
    BallSpawnScript spawner;

    [SerializeField] GameObject inputObject;
    InputValue input;

    // -------------------------------------- GUI -------------------------------------- //

    [SerializeField] GameObject pauseMenu;
    bool paused = false;
    public bool Paused 
    {
        get { return paused; }
    }

    [SerializeField] TextMeshProUGUI quantityText;

    // -------------------------------------- balls -------------------------------------- //

    int regularBallQuantity = 200; // [0, 500]
    public int RegularBallQuantity
    {
        get { return regularBallQuantity; }
        //set { ballQuantity = value; }
    }

    bool randomizeBallSize = true;
    public bool RandomizeBallSize
    {
        get { ;return randomizeBallSize;}
    }

    float velocityConstant = 1f; // (0, 1.5]
    public float VelocityConstant
    {
        get { return velocityConstant; }
    }

    float diameterConstant = 1f; // (0, 1.5]
    public float DiameterConstant
    {
        get { return diameterConstant; }
    }
    
    // -------------------------------------- special balls -------------------------------------- //
    int specialBallQuantity = 3; // [0, 50]
    public int SpecialBallQuantity
    {
        get { return specialBallQuantity; }
    }

    ForceType forceType;

    int forcePosOrNeg; // {0, 1, -1}
    public int ForcePosOrNeg
    {
        get { return forcePosOrNeg;}
    }

    float forceConstant = 60f; // (0, 200]
    public float ForceConstant
    {
        get { return forceConstant; }
    }

    float forceRadius = 20f; // (0, 100]
    public float ForceRadius
    {
        get { return forceRadius; }
    }

    double movingIncrement = 0.05; // (0, 0.15]
    public double MovingIncrement
    {
        get { return movingIncrement; }
    }

    double angleIncrement = 0.02; // (0, 0.15]
    public double AngleIncrement
    {
        get { return angleIncrement; }
    }

    bool colorSpecialBall = true;
    public bool ColorSpecialBall
    {
        get { return colorSpecialBall; }
    }

    // -------------------------------------- special ball appearance -------------------------------------- //

    [SerializeField] Color specialBallColor = new Color(0.34f, 0.79f, 0.76f, 1);
    public Color SpecialBallColor
    {
        get { return specialBallColor; }
    }

    [SerializeField] bool labelSpecialBall = false;
    public bool LabelSpecialBall
    {
        get { return labelSpecialBall; }
    }

    // -------------------------------------- momentum -------------------------------------- //

    double initTotalMomentum = 0;
    public double InitTotalMomentum 
    {
        get { return initTotalMomentum; } 
        set { initTotalMomentum = value; }
    }

    double lastTotalMomentum = 0;
    public double LastTotalMomentum 
    {
        get { return lastTotalMomentum; } 
        set { lastTotalMomentum = value; }
    }

    double curTotalMomentum = 0;
    public double CurTotalMomentum 
    {
        get { return curTotalMomentum; } 
        set { curTotalMomentum = value; }
    }

    // --------------------------------------------------------------------------------------------- //
    // -------------------------------------- default methods -------------------------------------- //
    // --------------------------------------------------------------------------------------------- //

    // Start is called before the first frame update
    void Start()
    {   
        spawner = spawnerObject.GetComponent<BallSpawnScript>();
        spawner.SetLogic();
        input = inputObject.GetComponent<InputValue>();
        PauseGame();
    }

    // Update is called once per frame
    void Update()
    {
        ManageGame();
        lastTotalMomentum = curTotalMomentum;
        curTotalMomentum = 0f;
        //Debug.Log("------------------------------ reset cur total ------------------------------");
    }

    // -------------------------------------------------------------------------------------------- //
    // -------------------------------------- custom methods -------------------------------------- //
    // -------------------------------------------------------------------------------------------- //

    // -------------------------------------- GUI -------------------------------------- //

    public void UpdateQuantity(int quantity)
    {
        quantityText.text = quantity.ToString();
    }

    void ManageGame()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (paused)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResumeGame();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                ResetGame();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        paused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        paused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    void ResetGame()
    {
        spawner.DeleteAllBall();
        ReadInput();
        spawner.SpawnNewBall();

        ResumeGame();
    }

    // -------------------------------------- Inputs -------------------------------------- //

    void ReadInput() 
    {
        quantityText.enabled = input._displayQuantityText;

        regularBallQuantity = input._regularBallQuantity;
        randomizeBallSize = input._randomizeBallSize;
        velocityConstant = input._velocityConstant;
        diameterConstant = input._diameterConstant;

        specialBallQuantity = input._specialBallQuantity;
        forceType = (ForceType) Enum.Parse(typeof(ForceType), input._forceType);
        SetForceTypeValue();
        forceConstant = input._forceConstant;
        forceRadius = input._forceRadius;
        movingIncrement = input._movingIncrement;
        angleIncrement = input._angleIncrement;
        colorSpecialBall = input._colorSpecialBall;
    }



    void SetForceTypeValue()
    {
        if (forceType == ForceType.None)
        {
            forcePosOrNeg = 0;
        }
        else if (forceType == ForceType.Repel)
        {
            forcePosOrNeg = 1;
        }
        else if (forceType == ForceType.Attract)
        {
            forcePosOrNeg = -1;
        }
    }
}

