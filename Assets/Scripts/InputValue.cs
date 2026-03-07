using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputValue: MonoBehaviour
{

    public bool _displayQuantityText = true;
    public void UpdateDisplayQuantityText(bool b)
    {
        _displayQuantityText = b;
    }
    // the comments are the bounds for values

    // -------------------------------------- balls -------------------------------------- //

    public int _regularBallQuantity = 196; // [0, 500]
    public void UpdateRegularBallQuantity(string s)
    {
        _regularBallQuantity = int.Parse(s);
    }

    public bool _randomizeBallSize = true;
    public void UpdateRandomizeBallSize(bool b)
    {
        //Debug.Log("" + b);
        _randomizeBallSize = b;
    }

    public float _velocityConstant = 1f; // (0, 1.5]
    public void UpdateVelocityConstant(string s)
    {
        //Debug.Log(s);
        //Debug.Log(float.TryParse(s, out _));
        _velocityConstant = float.Parse(s);
    }

    public float _diameterConstant = 1f; // (0, 1.5]
    public void UpdateDiameterConstant(string s) 
    {
        //Debug.Log(s);
        //Debug.Log(float.TryParse(s, out _));
        _diameterConstant = float.Parse(s);
    }


    // -------------------------------------- special balls -------------------------------------- //

    public int _specialBallQuantity = 4; // [0, 50]
    public void UpdateSpecialBallQuantity(string s)
    {
        //Debug.Log(s);
        //Debug.Log(int.TryParse(s, out _));
        _specialBallQuantity = int.Parse(s);
    }

    public string _forceType = "Repel"; // None, Repel, Attract
    public void UpdateForceType(int n)
    {
        if (n == 0) 
        {
            _forceType = "Repel";
        }
        else if (n == 1)
        {
            _forceType = "Attract";
        }
        else if (n == 2)
        {
            _forceType = "None";
        }
        else
        {
            _forceType = "None";
        }
    }

    public float _forceConstant = 60f; // (0, 200]
    public void UpdateForceConstant(string s)
    {
        //Debug.Log(s);
        //Debug.Log(float.TryParse(s, out _));
        _forceConstant = float.Parse(s);
    }

    public float _forceRadius = 20f; // (0, 100]
    public void UpdateForceRadius(string s) 
    {
        //Debug.Log(s);
        //Debug.Log(float.TryParse(s, out _));
        _forceRadius = float.Parse(s);
    }

    public double _movingIncrement = 0.05; // (0, 0.15]
    public void UpdateMovingIncrement(string s)
    {
        //Debug.Log(s);
        //Debug.Log(double.TryParse(s, out _));
        _movingIncrement = double.Parse(s);
    }

    public double _angleIncrement = 0.02; // (0, 0.15]
    public void UpdateAngleIncrement(string s)
    {
        //Debug.Log(s);
        //Debug.Log(double.TryParse(s, out _));
        _angleIncrement = double.Parse(s);
    }

    public bool _colorSpecialBall = true;
    public void UpdateColorSpecialBall(bool b)
    {
        //Debug.Log("" + b);
        _colorSpecialBall = b;
    }

    void ValidateInput()
    {
        if (_regularBallQuantity < 0)
        {
            _regularBallQuantity = 0;
        }
        else if (_regularBallQuantity > 500)
        {
            _regularBallQuantity = 500;
        }

        if (_velocityConstant <= 0)
        {
            _velocityConstant = 0.001f;
        }
        else if (_velocityConstant > 1.5f)
        {
            _velocityConstant = 1.5f;
        }

        if (_diameterConstant <= 0)
        {
            _diameterConstant = 0.001f;
        }
        else if (_diameterConstant > 1.5f)
        {
            _diameterConstant = 1.5f;
        }

        if (_specialBallQuantity < 0)
        {
            _specialBallQuantity = 0;
        }
        else if (_specialBallQuantity > 50)
        {
            _specialBallQuantity = 50;
        }

        if (_forceConstant <= 0)
        {
            _forceConstant = 0.001f;
        }
        else if (_forceConstant > 200)
        {
            _forceConstant = 200;
        }

        if (_forceRadius <= 0)
        {
            _forceRadius = 0.001f;
        }
        else if (_forceRadius > 100)
        {
            _forceRadius = 100;
        }

        if (_movingIncrement <= 0)
        {
            _movingIncrement = 0.0001f;
        }
        else if (_movingIncrement > 0.1f)
        {
            _movingIncrement = 0.15f;
        }

        if (_angleIncrement <= 0)
        {
            _angleIncrement = 0.0001f;
        }
        else if (_angleIncrement > 0.1f)
        {
            _angleIncrement = 0.15f;
        }
    }
}
