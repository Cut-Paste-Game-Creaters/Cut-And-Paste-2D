using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerInput
{
    public static bool isPausing = false;

    public static float GetAxisRaw(string dir)
    {
        if (isPausing) return 0.0f;
        if(dir == "Horizontal")
        {
            return Input.GetAxisRaw("Horizontal");
        }
        else if(dir == "Vertical")
        {
            return Input.GetAxisRaw("Vertical");
        }

        return 0.0f;
    }

    public static float GetDeltaTime()
    {
        if (isPausing) return 0.0f;
        return Time.deltaTime;
    }

    public static float GetFixedDeltaTime()
    {
        if (isPausing) return 0.0f;
        return Time.fixedDeltaTime;
    }
    public static bool GetKeyDown(KeyCode key)
    {
        if (isPausing) return false;
        return Input.GetKeyDown(key);
    }

    public static bool GetKeyDown1(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public static bool GetKey(KeyCode key)
    {
        if (isPausing) return false;
        return Input.GetKey(key);
    }

    public static bool GetKeyUp(KeyCode key)
    {
        if (isPausing) return false;
        return Input.GetKeyUp(key);
    }

    public static bool GetMouseButtonDown(int i)
    {
        if (isPausing) return false;
        return Input.GetMouseButtonDown(i);
    }

    public static bool GetMouseButtonDown1(int i)
    {
        return Input.GetMouseButtonDown(i);
    }

    public static bool GetMouseButton(int i)
    {
        if (isPausing) return false;
        return Input.GetMouseButton(i);
    }

    public static bool GetMouseButtonUp(int i)
    {
        if (isPausing) return false;
        return Input.GetMouseButtonUp(i);
    }
}



/*
 if(Input.GetKeyDown(KeyCode.A)){
    ...
}
 
 */