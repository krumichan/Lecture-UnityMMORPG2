using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
        switch (Dir)
        {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, -180);
                break;

            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;

            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }

        State = CreatureState.Moving;
        _speed = 15.0f;

        base.Init();
    }

    protected override void UpdateAnimation()
    {
        // Animation �ʿ� ����.
    }
}
