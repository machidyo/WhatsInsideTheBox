using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum State
    {
        Standing,
        Walking,
        Falling,
    }

    public State CurrentState;
    public bool IsGrounded => IsGroundedInternal();

    public float WalkingTime { get; private set; } = 0.0f;

    private Rigidbody rigid;
    private CancellationTokenSource walking;
    private CancellationTokenSource falling;
    
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        CurrentState = State.Standing;
    }

    async void Update()
    {
        switch (CurrentState)
        {
            case State.Standing:
                CurrentState = State.Walking;
                await MoveForward();
                CurrentState = IsGrounded ? State.Standing : State.Falling;
                break;
            case State.Walking:
                if (!IsGrounded)
                {
                    CurrentState = State.Falling;
                    walking.Cancel();
                    Respawn().Forget();
                }
                else
                {
                    WalkingTime += Time.deltaTime;
                }
                break;
            case State.Falling:
                if (IsGrounded)
                {
                    CurrentState = State.Standing;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async UniTask MoveForward()
    {
        walking = new CancellationTokenSource();
        var speedPerSec = 0.1f;
        var loop = 1 / speedPerSec;
        for (var i = 0; i < loop && !walking.IsCancellationRequested; i++)
        {
            transform.DOLocalMove(transform.position + transform.forward * speedPerSec, speedPerSec).SetEase(Ease.Linear);
            await UniTask.Delay(TimeSpan.FromSeconds(speedPerSec));
        }
    }

    private bool IsGroundedInternal()
    {
        var scaleAdjust = transform.localScale.y;
        var length = 0.5f * scaleAdjust;
        var ray = new Ray(transform.position - transform.forward * 0.5f, Vector3.down);
        Debug.DrawRay(ray.origin, Vector3.down * length, Color.red, 3);
        return Physics.Raycast(ray, length);
    }

    private async UniTask Respawn()
    {
        falling = new CancellationTokenSource();
        while (!falling.IsCancellationRequested)
        {
            if (transform.position.y < -5)
            {
                rigid.velocity = Vector3.zero;
                rigid.useGravity = false;
                falling.Cancel();
                await UniTask.DelayFrame(1);
            }
            else
            {
                await UniTask.Delay(100);
            }
        }
        
        transform.SetPositionAndRotation(new Vector3(0, 0, -3), Quaternion.identity);
        rigid.useGravity = true;
        falling.Dispose();
    }
    
    /// <summary>
    /// for debug
    /// 今回に関して言えば、動きの質感的に MoveForward のほうがいい気がした。念のため取っておく。
    /// </summary>
    private async UniTask MoveByPath()
    {
        var poss = new[]
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 0),
        };
        transform
            .DOPath(poss, 15, PathType.Linear)
            .SetLookAt(0.1f, Vector3.forward);
        await UniTask.Delay(TimeSpan.FromSeconds(16));
    }
}