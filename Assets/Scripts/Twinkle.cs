using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twinkle : MonoBehaviour
{
    public GameObject twinkleGameObject;
    private float twinkleSpeed;
    private float timer;
    private bool twinkleActive;

    void Start()
    {
        timer = 0f;
        twinkleSpeed = 1f;
        twinkleActive = true;
    }

    void Update()
    {
        timer += Time.deltaTime * twinkleSpeed;
        if (timer >= 0.45f)
        {
            //껐다 켰다 반복
            if (twinkleActive == true)
            {
                twinkleActive = false;
                twinkleGameObject.SetActive(false);
            }
            else {
                twinkleActive = true;
                twinkleGameObject.SetActive(true);
            }

            timer = 0f;
        }

    }
}
