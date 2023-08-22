using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager _instance;
    //获得Text组件
    public Text shootNumText;
    public Text scoreText;
    public Text messageText;

    public int shootNum = 0;
    public int score = 0;

    //音乐开关的单选框和播放背景音乐的AudioSource
    public Toggle musicToggle;
    public AudioSource musicAudio;

    private void Awake()
    {
        _instance = this;

        if (PlayerPrefs.HasKey("MusicOn"))
        {
            if (PlayerPrefs.GetInt("MusicOn") == 0)
            {
                musicToggle.isOn = false;
                musicAudio.enabled = false;
            }
            else
            {
                musicToggle.isOn = true;
                musicAudio.enabled = true;
            }
        }
        else
        {
            musicToggle.isOn = true;
            musicAudio.enabled = true;
        }
    }

    private void Update()
    {
        //更新Text组件的显示内容
        shootNumText.text = shootNum.ToString();
        scoreText.text = score.ToString();
    }

    public void MusicSwitch()
    {
        //通过判断单选框是否被勾选上，从而来决定是否播放背景音乐
        if (musicToggle.isOn == false)
        {
            musicAudio.enabled = false;
            //保存音乐开关的状态，0代表关闭状态
            PlayerPrefs.SetInt("MusicOn", 0);
        }
        else
        {
            musicAudio.enabled = true;
            PlayerPrefs.SetInt("MusicOn", 1);
        }
        PlayerPrefs.Save();
    }

    //增加射击数（当开枪时）
    public void AddShootNum()
    {
        shootNum += 1;
    }
    //增加得分（当射中怪物时）
    public void AddScore()
    {
        score += 1;
    }

    public void ShowMessage(string str)
    {
        messageText.text = str;
    }
}
