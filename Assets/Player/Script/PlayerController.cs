using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region [Parameter]
    private Rigidbody2D rb;
    public float moveSpeed;

    private Animator animator;

    [Header("连招设置")]
    private float totalTimer = -1;
    private float recoveryTime = 0;
    public List<ComboClipConfig> skillAction;
    public List<ComboClipConfig> skillActionInSky;
    public List<ComboClipConfig> specialAction;
    public List<ComboClipConfig> specialActionInSky;
    public List<ComboClipConfig> startCombo;
    public List<ComboClipConfig> startComboInSky;
    private ComboClipConfig currentComboConfig;
    private bool isComboStart = false;
    private bool isUpPressed = false;
    private bool isDownPressed = false;
    private bool isShiftPressed = false;

    [Header("地面检测")]
    private bool isInSky;
    public Vector2 posOffset;
    public float checkRadius;
    public LayerMask checkLayer;

    [Header("缓冲区")]
    public List<KeyCode> keyBuffer;
    private float keyBufferRefreshTime = 0.15f;
    private float keyBufferRefreshTimer = 0.15f;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (keyBufferRefreshTimer > 0)
        {
            keyBufferRefreshTimer -= Time.fixedDeltaTime;
        }
        else if (keyBuffer.Count != 0)
        {
            keyBufferRefreshTimer = keyBufferRefreshTime;
            keyBuffer.Clear();
        }
    }

    private void GroundCheck()
    {
        isInSky = !Physics2D.OverlapCircle((Vector2)gameObject.transform.position + posOffset, checkRadius, checkLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)gameObject.transform.position + posOffset, checkRadius);
    }

    private void Update()
    {
        GroundCheck();
        animator.SetBool("isInSky", isInSky);
        float moveX = Input.GetAxisRaw("Horizontal");

        if (totalTimer > 0)
        {
            totalTimer -= Time.deltaTime;
        }
        else
        {
            Move(moveX);
            isComboStart = false;
            currentComboConfig = null;
        }

        SetKeyStatus();

        InputKey();

        if (totalTimer <= recoveryTime)
        {
            rb.gravityScale = 6;
            Flip(moveX);
            JudgeAction();
        }
    }

    private void Move(float moveX)
    {   
        if (moveX != 0)
        {
            animator.Play("Move");
            rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
        }
        else
        {
            animator.Play("Idle");
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void Flip(float moveX)
    {
        if (moveX != 0)
        {
            gameObject.transform.localScale = new(moveX > 0 ? 1 : -1, 1, 1);
        }
    }

    private void SetKeyStatus()
    {
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            isUpPressed = true;
        }
        else if (Input.GetAxisRaw("Vertical") < 0)
        {
            isDownPressed = true;
        }
        else
        {
            isUpPressed = false;
            isDownPressed = false;
        }
        isShiftPressed = Input.GetKey(KeyCode.LeftShift);
    }

    private bool IsSkillKey(ComboClipConfig.AvailableKey key)
    {
        if (key == ComboClipConfig.AvailableKey.A || key == ComboClipConfig.AvailableKey.S || key == ComboClipConfig.AvailableKey.D || key == ComboClipConfig.AvailableKey.C)
            return true;
        return false;
    }

    private void InputKey()
    {
        foreach (ComboClipConfig.AvailableKey key in Enum.GetValues(typeof(ComboClipConfig.AvailableKey)))
        {
            if (Input.GetKeyDown((KeyCode)key))
            {
                keyBufferRefreshTimer = keyBufferRefreshTime;
                if (isUpPressed)
                    keyBuffer.Add(KeyCode.UpArrow);
                if (isDownPressed)
                    keyBuffer.Add(KeyCode.DownArrow);
                if (isShiftPressed && IsSkillKey(key))
                    keyBuffer.Add(KeyCode.LeftShift);
                keyBuffer.Add((KeyCode)key);
            }
        }
    }

    private bool KeyEqualAKey(List<KeyCode> keys, List<ComboClipConfig.AvailableKey> availableKeys)
    {
        bool result = true;
        if (keys.Count == availableKeys.Count)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                result = result && keys[i] == (KeyCode)availableKeys[i];
            }
        }
        else
            result = false;
        return result;
    }

    private void JudgeAction()
    {
        if (keyBuffer.Count == 0)
        {
            return;
        }

        if (isInSky == false)
        {
            if (FindAction(specialAction))
            {
                DoAction();
                return;
            }
        }
        else
        {
            if (FindAction(specialActionInSky))
            {
                DoAction();
                return;
            }
        }
            
        if(isComboStart == false)
        {
            ComboStart();
        }
        else
        {
            ComboContinue();
        }
    }

    private bool FindAction(List<ComboClipConfig> action)
    {
        for (int i = 0; i < action.Count; i++)
        {
            if (action[i].key.Count <= keyBuffer.Count)
            {
                if (KeyEqualAKey(keyBuffer.GetRange(keyBuffer.Count - action[i].key.Count, action[i].key.Count), action[i].key))
                {
                    currentComboConfig = action[i];
                    //isComboStart = true;
                    return true;
                }
            }
        }
        return false;
    }

    private void ComboStart()
    {
        if (isInSky == false)
        {
            if (FindAction(skillAction))
            {
                isComboStart = true;
                DoAction();
            }
            else if (FindAction(startCombo))
            {
                isComboStart = true;
                DoAction();
            }
            else
            {
                Debug.Log("未找到按键对应操作");
            }
        }
        else
        {
            if (FindAction(skillActionInSky))
            {
                isComboStart = true;
                DoAction();
            }
            else if (FindAction(startComboInSky))
            {
                isComboStart = true;
                DoAction();
            }
            else
            {
                Debug.Log("未找到按键对应操作");
            }
        }
    }

    private void ComboContinue()
    {
        if (FindAction(currentComboConfig.derivedActions))
        {
            DoAction();
        }
        else
        {
            if (currentComboConfig.isSkill == true)
            {
                ComboStart();
                return;
            }
            if (isInSky == false)
            {
                if(FindAction(skillAction))
                    DoAction();
                else
                    Debug.Log("未找到按键对应操作");
            }
            else
            {
                if (FindAction(skillActionInSky))
                    DoAction();
                else
                    Debug.Log("未找到按键对应操作");
            }
        }
    }

    private void DoAction()
    {
        recoveryTime = currentComboConfig.recoveryTime;
        totalTimer = currentComboConfig.requiredTime + recoveryTime;
        if (currentComboConfig.isControlMove == true)
        {
            rb.velocity = Vector2.zero;
        }
        if (currentComboConfig.isControlGravity == true)
        {
            rb.gravityScale = 0;
        }
        animator.Play(currentComboConfig.comboClip);
    }

    #region [AnimationEvent]

    public void ActionMoveX(float x)
    {
        rb.velocity = new Vector2(gameObject.transform.localScale.x * x, rb.velocity.y);
    }
    public void ActionMoveY(float y)
    {
        rb.velocity = new Vector2(rb.velocity.x, y);
    }
    public void SetTimer(float t)
    {
        totalTimer = t;
    }
    #endregion
}
