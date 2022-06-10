using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    const int stairsMax = 25;

    //배경 화면들
    public GameObject[] backgroundImage;
    public GameObject backgroundImageBackup;

    //캐릭터들
    public Character player;
    public Character enemy;

    //음악
    public AudioClip audio;
    private AudioSource backGroundMusic;

    //계단, 코인
    public GameObject stairPrefab;
    private GameObject[] stairs;
    public GameObject coinPrefab;

    //UI에 사용할 텍스트들
    public Text[] coinText;
    public Text scoreText;
    public Text endScoreText;

    //점수
    private int score;

    //계단과 코인 상태
    private bool[] IsChangeDir; // 해당 계단이 방향이 전환되어야 하는지 확인
    private bool[] IsCoin; // 해당 계단이 방향이 전환되어야 하는지 확인

    //기타 설정들
    private float timer;
    private float enemySpeed;
    private bool enemyStart;

    //게임의 플레이 상태
    enum GameState { start, startComplete, play, end, endComplete }
    private GameState gameSate = GameState.start;

    //계단의 생성 상태
    enum State { start, leftDir, rightDir } // 계단 생성 위치
    private State StairsState = State.start;

    private Vector3 beforePos, // 이전 계단의 위치
    startPos = new Vector3(540f, 768f, 0),
    leftPos = new Vector3(-171f, 96f, 0), // 왼쪽 방향 계단 생성 위치
    rightPos = new Vector3(171f, 96f, 0), // 오른쪽 방향 계단 생성 위치
    leftDir = new Vector3(171f, -96f, 0), // 플레이어가 왼쪽 방향 이동했을때 계단 이동
    rightDir = new Vector3(-171f, -96f, 0); // 플레이어가 오른쪽 방향 이동했을때 계단 이동

    //초기화
    void Start()
    {
        //음악 초기화
        backGroundMusic = gameObject.AddComponent<AudioSource>();
        backGroundMusic.loop = true;
        backGroundMusic.clip = audio;
        backGroundMusic.Play();

        //설정 초기화
        enemySpeed = 1f;
        stairs = new GameObject[stairsMax];
        for (int i = 0; i < stairsMax; i++) {
            stairs[i] = Instantiate(stairPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

    //게임 진행
    void Update() {

        //상태에 따라 진행
        switch (gameSate)
        {
            case GameState.start:
                {
                    GameObject.Find("Canvas").transform.Find("GameStart").gameObject.SetActive(true);
                    GameObject.Find("Canvas").transform.Find("GamePlaying").gameObject.SetActive(false);
                    GameObject.Find("Canvas").transform.Find("GameEnd").gameObject.SetActive(false);

                    for (int i = 0; i < coinText.Length; i++)
                    {
                        coinText[i].text = "Coin : " + PlayerPrefs.GetInt("Coin").ToString();
                    }

                    StairsInit(); // 계단 점수 등 초기화
                    gameSate = GameState.startComplete;
                }
                break;
            case GameState.play:
                {
                    GameObject.Find("Canvas").transform.Find("GameStart").gameObject.SetActive(false);
                    GameObject.Find("Canvas").transform.Find("GamePlaying").gameObject.SetActive(true);
                    GameObject.Find("Canvas").transform.Find("GameEnd").gameObject.SetActive(false);

                    //3칸 이상 이동하면 적 이동 시작
                    if (score > 3)
                    {
                        timer += Time.deltaTime * enemySpeed;
                        if (timer >= 1f)
                        {
                            EnemyMove();
                            timer = 0.0f;
                            enemySpeed += 0.1f;
                            //적 최대 이동속도
                            if (enemySpeed > 5f)
                            {
                                enemySpeed = 5f;
                            }
                        }
                    }

                    for (int i = 0; i < coinText.Length; i++)
                    {
                        coinText[i].text = "Coin : " + PlayerPrefs.GetInt("Coin").ToString();
                    }
                }
                break;
            case GameState.end:
                {
                    GameObject.Find("Canvas").transform.Find("GameStart").gameObject.SetActive(false);
                    GameObject.Find("Canvas").transform.Find("GamePlaying").gameObject.SetActive(false);
                    GameObject.Find("Canvas").transform.Find("GameEnd").gameObject.SetActive(true);

                    gameSate = GameState.endComplete;
                }
                break;
        }
    }

    //계단 초기화
    void StairsInit()
    {
        //배경 초기화
        for (int i = 0; i < backgroundImage.Length; i++)
        {
            backgroundImage[i].transform.position = new Vector3(540f, 2048f + (i * 4096f), 0);
            if (i == 0) {
                backgroundImage[i].GetComponent<SpriteRenderer>().sprite = backgroundImageBackup.GetComponent<SpriteRenderer>().sprite;
            }
        }

        //캐릭터 초기화
        player.isleft = true;
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        player.isDie = false;
        player.stairIndex = 0;

        //적 초기화
        enemy.isleft = true;
        enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
        enemy.stairIndex = 0;
        enemyStart = false;

        //계단 초기화
        StairsState = State.start;
        IsChangeDir = new bool[stairsMax];
        IsCoin = new bool[stairsMax];

        enemySpeed = 1f; // 적 이동속도 초기화
        timer = 0.0f; // 시간 초기화

        // 0점으로 초기화
        score = 0;
        scoreText.text = score.ToString();

        player.transform.position = new Vector3(startPos.x, startPos.y + 190f, startPos.z); // 플레이어 초기 위치. 플레이어는 이동 안함
        enemy.transform.position = new Vector3(startPos.x - 3000f , startPos.y + 190f, startPos.z); // 적 초기 위치 // 멀리 숨겨두기

        //계단 생성
        for (int i = 0; i < stairsMax; i++)
        {
            switch (StairsState)
            {
                case State.start: // 첫번째 계단
                    stairs[i].transform.position = startPos + leftPos;
                    StairsState = State.leftDir; // 다음 계단은 왼쪽에 생김
                    break;
                case State.leftDir: // 왼쪽 방향으로 계단 생성
                    stairs[i].transform.position = beforePos + leftPos;
                    break;
                case State.rightDir: // 오른쪽 방향으로 계단 생성
                    stairs[i].transform.position = beforePos + rightPos;
                    break;
            }
            beforePos = stairs[i].transform.position; // 지금까지 생성된 마지막 계단의 위치를 beforePos에 저장

            if (i != 0)
            {
                // 30프로의 확률로 계단 방향 바꿔서 생성
                if (Random.Range(1, 9) < 3) 
                {
                    if (StairsState == State.leftDir) StairsState = State.rightDir;
                    else if (StairsState == State.rightDir) StairsState = State.leftDir;
                    IsChangeDir[(i + 1) % stairsMax] = true;

                    //첫 계단은 true가 되면 안됨
                    if (((i + 1) % stairsMax) == 0) {
                        IsChangeDir[(i + 1) % stairsMax] = false;
                    }
                }             
            }

            // 30프로의 확률로 코인 생성
            if (Random.Range(1, 9) <= 3)
            { 
                IsCoin[(i + 1) % stairsMax] = true;
                stairs[(i + 1) % stairsMax].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                IsCoin[(i + 1) % stairsMax] = false;
                stairs[(i + 1) % stairsMax].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    //발판 재사용
    void SpawnStair(int num)
    {
        IsChangeDir[num + 1 == stairsMax ? 0 : num + 1] = false; // false로 초기화
        beforePos = stairs[num == 0 ? (stairsMax - 1) : num - 1].transform.position; // 제일 위에 있는 계단 좌표
        switch (StairsState)
        {
            case State.leftDir:
                stairs[num].transform.position = beforePos + leftPos;
                break;
            case State.rightDir:
                stairs[num].transform.position = beforePos + rightPos;
                break;
        }

        // 30프로의 확률로 계단 방향 바꿔서 생성
        if (Random.Range(1, 9) < 3) 
        {
            if (StairsState == State.leftDir) StairsState = State.rightDir; // 다음 계단의 state가 바뀜
            else if (StairsState == State.rightDir) StairsState = State.leftDir;
            IsChangeDir[num + 1 == stairsMax ? 0 : num + 1] = true; // 지금 생성되는 계단은 isChangeDir가 true로 됨
        }

        // 30프로의 확률로 코인 생성
        if (Random.Range(1, 9) <= 3)
        {
            IsCoin[num + 1 == stairsMax ? 0 : num + 1] = true;
            stairs[num + 1 == stairsMax ? 0 : num + 1].transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            IsCoin[num + 1 == stairsMax ? 0 : num + 1] = false;
            stairs[num + 1 == stairsMax ? 0 : num + 1].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    //계단 이동
    public void StairMove(int stairIndex, bool isChange, bool isleft)
    {
        for (int i = 0; i < stairsMax; i++)
        {
            if (isleft) stairs[i].transform.position += leftDir; // 모든 계단을 leftDir로 이동
            else stairs[i].transform.position += rightDir; // 모든 계단을 rightDir로 이동
        }

        //계단 좌표 확인
        for (int i = 0; i < stairsMax; i++)
        {
            if (stairs[i].transform.position.y < (-96 * 3)) SpawnStair(i); // 화면 아래로 벗어난 계단을 재활용
        }

        //방향 전환 잘못했으면 게임오버
        if (IsChangeDir[stairIndex] != isChange)
        {
            GameOver();
            return;
        }

        //점수 증가
        score++;
        scoreText.text = score.ToString();

        //코인 없애기, 코인 증가
        if (IsCoin[stairIndex] == true) {
            PlayerPrefs.SetInt("Coin", PlayerPrefs.GetInt("Coin") + 1);
            stairs[stairIndex].transform.GetChild(0).gameObject.SetActive(false);
            IsCoin[stairIndex] = false;
        }

        //배경 이미지 이동
        for (int i = 0; i < backgroundImage.Length; i++)
        {
            //배경 재활용 
            if (backgroundImage[i].transform.position.y < -(2048f + 300f))
            {
                backgroundImage[i].transform.position = new Vector3(backgroundImage[i + 1 % backgroundImage.Length].transform.position.x, backgroundImage[i + 1 % backgroundImage.Length].transform.position.y + 4096f, backgroundImage[i + 1 % backgroundImage.Length].transform.position.z);
                backgroundImage[i].GetComponent<SpriteRenderer>().sprite = backgroundImage[i + 1 % backgroundImage.Length].GetComponent<SpriteRenderer>().sprite;
            }

            backgroundImage[i].transform.position = new Vector3(backgroundImage[i].transform.position.x, backgroundImage[i].transform.position.y - 100f, backgroundImage[i].transform.position.z);     
        }
    }

    //게임 시작 버튼
    public void BtnStart(GameObject btn)
    {
        gameSate = GameState.play;
    }

    //게임 다시 시작 re play
    public void BtnReplay(GameObject btn)
    {        
        StairsInit(); // 계단 점수 등 초기화
        gameSate = GameState.play;
    }

    //게임 다시 시작 re play
    public void BtnMain(GameObject btn)
    {        
        gameSate = GameState.start;
    }

    //앞으로 이동
    public void BtnDownMove(GameObject btn)
    {
        StairMove(player.stairIndex, false, player.isleft);
        player.Climb(false); // 방향 바꾸지 않고 이동
        EnymyCheck();
    }

    //방향 전환
    public void BtnDownTurn(GameObject btn)
    {
        StairMove(player.stairIndex, true, !player.isleft);
        player.Climb(true); // 방향 바꾸면서 이동
        EnymyCheck();
    }

    //적 한칸 쫒아오기
    private void EnemyClimb()
    {
        EnemyMove();
    }

    //게임오버
    void GameOver()
    {
        gameSate = GameState.end;

        endScoreText.text = score.ToString();

        player.isDie = true;
    }

    void EnymyCheck()
    {
        // 3칸 넘지 않았으면 활성화 되지 않음
        if (score <= 3) {
            return;
        }

        //플레이어 위에는 존재할수 없음
        if (enemy.transform.position.y > player.transform.position.y)
        {
            //가장 아래 있는 블럭으로 이동
            int minstairIndex = enemy.stairIndex;
            for (int i = 0; i < stairsMax; i++) {
                if (stairs[i].transform.position.y < stairs[minstairIndex].transform.position.y) {
                    minstairIndex = i;
                }
            }
            enemy.stairIndex = minstairIndex;
        }

        float xPos = stairs[enemy.stairIndex].transform.position.x;
        float yPos = stairs[enemy.stairIndex].transform.position.y;
        float zPos = stairs[enemy.stairIndex].transform.position.z;
        enemy.transform.position = new Vector3(xPos, yPos + 190f, zPos);
    }

    //적 한칸 이동
    void EnemyMove()
    {
        float ComparePositionX = stairs[enemy.stairIndex].transform.position.x;

        enemy.stairIndex++;
        enemy.stairIndex = enemy.stairIndex % 25; // 배열의 인덱스 넘어가지 않게
        enemy.MoveAnimation();
        EnymyCheck();

        ////적 좌우 반전 하기
        if (ComparePositionX < stairs[enemy.stairIndex].transform.position.x)
        {
            enemy.isleft = false;
        }
        else
        {
            enemy.isleft = true;

        }
        ////적 좌우 반전 하기
        //if (enemyStart == true && IsChangeDir[enemy.stairIndex == 0 ? (stairsMax - 1) : enemy.stairIndex])
        //{
        //    enemy.isleft = !enemy.isleft; //방향 바꾸기
        //}

        if (enemy.isleft)
        {
            enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            enemy.transform.rotation = Quaternion.Euler(0, -180, 0);
        }

        //적과 플레이어과 겹치면 에러
        if (score > 3)
        {
            //따라잡히면 게임오버
            if (player.stairIndex == enemy.stairIndex + 1)
            {
                GameOver();
            }
        }

        enemyStart = true; //가장 처음 적의 이동은 좌우 반전 오류가 있을수 있어서 
    }


}
