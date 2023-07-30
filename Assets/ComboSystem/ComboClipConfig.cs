using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewComboClipConfig", menuName = "ComboSystem/ComboClipConfig", order = 2)]
public class ComboClipConfig : ScriptableObject
{
    public enum AvailableKey
    {
        Up = KeyCode.UpArrow,
        Down = KeyCode.DownArrow,
        Shift = KeyCode.LeftShift,
        Z = KeyCode.Z,
        X = KeyCode.X,
        A = KeyCode.A,
        S = KeyCode.S,
        D = KeyCode.D,
        C = KeyCode.C
    }
    public List<AvailableKey> key;
    public string comboClip;
    public float requiredTime;
    public float recoveryTime;
    public List<ComboClipConfig> derivedActions;
    public bool isControlMove;
    public bool isControlGravity;
    public bool isSkill;
}