using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public bool isleft, isDie; // 플레이어 좌우반전 상태, 죽었는지 여부
    public int stairIndex; // 플레이어가 다음에 설 계단의 위치
    private Animator anim;

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        stairIndex = 0;
        isleft = true;
        isDie = false;
    }

    //이동 애니메이션 실행
    public void MoveAnimation()
    {
        if (!isleft)
            transform.rotation = Quaternion.Euler(0, -180, 0);
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);

        anim.SetBool("Move", true);
    }

    //이동 애니메이션 완료
    public void IdleAnimation()
    {
        anim.SetBool("Move", false);
    }

    public void Climb(bool isChange)
    {
        if (isChange) isleft = !isleft; //방향 바꾸기
        stairIndex++; // 이동할 계단의 위치
        stairIndex = stairIndex % 25; // 배열의 인덱스 넘어가지 않게
        MoveAnimation();
    }
}
