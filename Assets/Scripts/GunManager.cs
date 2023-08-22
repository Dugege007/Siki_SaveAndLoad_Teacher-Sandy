using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    private AudioSource gunAudio;
    //最大和最小的X,Y轴的旋转角度
    private float maxYRotation = 120;
    private float minYRotation = 0;
    private float maxXRotation = 60;
    private float minXRotation = 0;
    //射击的间隔时长
    private float shootTime = 1;
    //射击间隔时间的计时器
    private float shootTimer = 0;
    //子弹的游戏物体，和子弹的生成位置
    public GameObject bulletGO;
    public Transform firePosition;

    private void Awake()
    {
        gunAudio = gameObject.GetComponent<AudioSource>();
    }

    private void Update()
    {
        //游戏是非暂停状态时才可以进行射击，并且枪随着鼠标旋转
        if (GameManager._instance.isPaused == false)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootTime)
            {
                //点击鼠标左键，进行射击
                if (Input.GetMouseButtonDown(0))
                {
                    //实例化子弹
                    GameObject bulletCurrent = GameObject.Instantiate(bulletGO, firePosition.position, Quaternion.identity);
                    //通过刚体组件给子弹添加一个正前方向上的力，以达到让子弹向前运动的效果
                    bulletCurrent.GetComponent<Rigidbody>().AddForce(transform.forward * 2200);
                    //播放手枪开火的动画
                    gameObject.GetComponent<Animation>().Play();
                    shootTimer = 0;
                    //播放手枪开火的音效
                    gunAudio.Play();

                    //增加射击数
                    UIManager._instance.AddShootNum();
                }
            }

            //根据鼠标在屏幕上的位置，去相对应的旋转手枪
            float xPosPrecent = Input.mousePosition.x / Screen.width;
            float yPosPrecent = Input.mousePosition.y / Screen.height;

            float xAngle = -Mathf.Clamp(yPosPrecent * maxXRotation, minXRotation, maxXRotation) + 15;
            float yAngle = Mathf.Clamp(xPosPrecent * maxYRotation, minYRotation, maxYRotation) - 60;

            transform.eulerAngles = new Vector3(xAngle, yAngle, 0);
        }
    }
}
