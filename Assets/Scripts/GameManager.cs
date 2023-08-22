using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LitJson;
using System.Xml;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    //是否是暂停状态
    public bool isPaused = true;
    public GameObject menuGO;

    public GameObject[] targetGOs;

    private void Awake()
    {
        _instance = this;
        //游戏开始时是暂停的状态
        Pause();
    }

    private void Update()
    {
        //判断是否按下ESC键，按下的话，调出Menu菜单，并将游戏状态更改为暂停状态
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    //暂停状态
    private void Pause()
    {
        isPaused = true;
        menuGO.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
    }

    //非暂停状态
    private void UnPause()
    {
        isPaused = false;
        menuGO.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
    }

    //创建Save对象并存储当前游戏状态信息
    private Save CreateSaveGO()
    {
        //新建一个Save对象
        Save save = new Save();
        //遍历所有的target
        //如果其中有处于激活状态的怪物，就把该target的位置信息和激活状态的怪物的类型添加到List中
        foreach (var target in targetGOs)
        {
            TargetManager tm = target.GetComponent<TargetManager>();
            if (tm.activeMonster != null)
            {
                save.livingTargetPosition.Add(tm.targetPosition);
                int type = tm.activeMonster.GetComponent<MonsterManager>().monsterType;
                save.livingMonsterTypes.Add(type);

            }
        }

        //把射击数和分数保存在save对象中
        save.shootNum = UIManager._instance.shootNum;
        save.score = UIManager._instance.score;

        //返回该save对象
        return save;
    }

    /// <summary>
    /// 通过独当信息重置我们的游戏状态（分数、激活怪物的状态）
    /// </summary>
    /// <param name="save"></param>
    private void SetGame(Save save)
    {
        //现将所有的target里面的怪物清空，并重置所有的计时
        foreach (var target in targetGOs)
        {
            target.GetComponent<TargetManager>().UpdateMonsters();
        }
        //通过反序列化的到的对象都存储到信息，激活指定的怪物
        for (int i = 0; i < save.livingTargetPosition.Count; i++)
        {
            int position = save.livingTargetPosition[i];
            int type = save.livingMonsterTypes[i];

            targetGOs[position].GetComponent<TargetManager>().ActivateMonsterByType(type);
        }

        //更新UI显示
        UIManager._instance.shootNum = save.shootNum;
        UIManager._instance.score = save.score;

        //调整为未暂停状态
        UnPause();
    }

    //保存为二进制
    private void SaveByBin()
    {
        //序列化过程，将save对象转换为字节流
        //创建Save对象并保存当前游戏状态
        Save save = CreateSaveGO();

        //需要using System.IO
        //创建一个二进制格式化程序
        BinaryFormatter bf = new BinaryFormatter();
        //创建一个文件流
        FileStream fileStream = File.Create(Application.dataPath + "/StreamingFile" + "/ByBin.txt");
        //用二进制格式化程序的序列化方法来序列化Save对象
        //参数：创建的文件流和需要序列化的对象
        bf.Serialize(fileStream, save);
        //关闭流
        fileStream.Close();

        //如果文件存在，则显示保存成功
        if (File.Exists(Application.dataPath + "/StreamingFile" + "/ByBin.txt"))
            UIManager._instance.ShowMessage("保存成功");
        else
            UIManager._instance.ShowMessage("保存失败");
    }

    private void LoadByBin()
    {
        //如果文件存在，则显示保存成功
        if (File.Exists(Application.dataPath + "/StreamingFile" + "/ByBin.txt"))
        {

            //反序列化过程
            //创建一个二进制格式化程序
            BinaryFormatter bf = new BinaryFormatter();
            //打开一个文件夹，要加一个模式参数
            FileStream fileStream = File.Open(Application.dataPath + "/StreamingFile" + "/ByBin.txt", FileMode.Open);
            //调用格式化程序的反序列化，将文件流转换为一个Save对象
            Save save = (Save)bf.Deserialize(fileStream);
            //关闭文件流
            fileStream.Close();
            //UI提示加载成功
            UIManager._instance.ShowMessage("加载成功");

            SetGame(save);
        }
        else
        {
            //当前没有存档
            UIManager._instance.ShowMessage("没有存档数据");
        }
    }

    //保存为XML
    private void SaveByXML()
    {
        Save save = CreateSaveGO();
        //创建xml文件的存储路径
        string filePath = Application.dataPath + "/StreamingFile" + "/ByXml.txt";
        //创建XML文档
        XmlDocument xmlDoc = new XmlDocument();
        //创建根节点，即最上层节点
        XmlElement root = xmlDoc.CreateElement("save");
        //设置根节点中的值
        root.SetAttribute("name", "saveFile1");

        //创建XmlElement
        XmlElement target;
        XmlElement targetPosition;
        XmlElement monsterType;

        //遍历save中存储的数据，将数据转换成XML格式
        for (int i = 0; i < save.livingTargetPosition.Count; i++)
        {
            target = xmlDoc.CreateElement("target");
            targetPosition = xmlDoc.CreateElement("targetPosition");
            //设置InnerText值
            monsterType = xmlDoc.CreateElement("monsterType");
            monsterType.InnerText = save.livingTargetPosition[i].ToString();

            //设置节点建的层级关系：root - target - (targetPosition, monsterType)
            target.AppendChild(targetPosition);
            target.AppendChild(monsterType);
            root.AppendChild(target);
        }

        //设置射击和分数节点并设置层级关系，xnlDoc - root - (target - (targetPosition, monsterType), shootNum, score)
        XmlElement shootNum = xmlDoc.CreateElement("shootNum");
        shootNum.InnerText = save.shootNum.ToString();
        root.AppendChild(shootNum);

        XmlElement score = xmlDoc.CreateElement("score");
        score.InnerText = save.score.ToString();
        root.AppendChild(score);

        xmlDoc.AppendChild(root);
        xmlDoc.Save(filePath);

        //如果文件存在，则显示保存成功
        if (File.Exists(Application.dataPath + "/StreamingFile" + "/ByXml.txt"))
            UIManager._instance.ShowMessage("保存成功");
        else
            UIManager._instance.ShowMessage("保存失败");
    }

    private void LoadByXML()
    {
        string filePath = Application.dataPath + "/StreamingFile" + "/ByXml.txt";
        if (File.Exists(filePath))
        {
            Save save = new Save();
            //加载XML文档
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            //通过节点名称来获取元素，结果为XmlNodeList类型
            XmlNodeList targets = xmlDoc.GetElementsByTagName("target");
            //遍历所有的target几点，并去的子节点和子节点的InnerText
            if (targets.Count != 0)
            {
                foreach (XmlNode target in targets)
                {
                    XmlNode targetPosition = target.ChildNodes[0];
                    int targetPositionIndex = int.Parse(targetPosition.InnerText);
                    //把得到的值存储到save中
                    save.livingTargetPosition.Add(targetPositionIndex);

                    XmlNode monsterType = target.ChildNodes[1];
                    int monsterTypeIndex=int.Parse(monsterType.InnerText);
                    save.livingMonsterTypes.Add(monsterTypeIndex);
                }
            }

            XmlNodeList shootNum = xmlDoc.GetElementsByTagName("shootNum");
            int shootNumCount = int.Parse(shootNum[0].InnerText);
            save.shootNum = shootNumCount;

            XmlNodeList score = xmlDoc.GetElementsByTagName("score");
            int scoreCount = int.Parse(score[0].InnerText);
            save.score = shootNumCount;

            SetGame(save);
        }
        else
        {
            UIManager._instance.ShowMessage("没有存档数据");
        }
    }

    //保存为Json
    private void SaveByJson()
    {
        Save save = CreateSaveGO();
        string filePath = Application.dataPath + "/StreamingFile" + "/ByJson.json";

        //利用JsonMapper将save对象转换为Json格式的字符串
        string saveJsonStr = JsonMapper.ToJson(save);
        //创建一个StreamWriter，将这个字符串写入到文件中
        StreamWriter sw = new StreamWriter(filePath);
        sw.Write(saveJsonStr);
        //关闭StreamWriter
        sw.Close();

        //如果文件存在，则显示保存成功
        if (File.Exists(Application.dataPath + "/StreamingFile" + "/ByJson.json"))
            UIManager._instance.ShowMessage("保存成功");
        else
            UIManager._instance.ShowMessage("保存失败");
    }

    private void LoadByJson()
    {
        string filePath = Application.dataPath + "/StreamingFile" + "/ByJson.json";
        if (File.Exists(filePath))
        {
            //创建一个StreamReader，用来读取流
            StreamReader sr = new StreamReader(filePath);
            //讲读取到的流赋值给jsonStr
            string jsonStr = sr.ReadToEnd();
            //关闭
            sr.Close();

            //将字符串jsonStr转换为Save对象
            Save save = JsonMapper.ToObject<Save>(jsonStr);
            SetGame(save);
        }
        else
        {
            //当前没有存档
            UIManager._instance.ShowMessage("没有存档数据");
        }
    }

    //从暂停状态恢复到非暂停状态
    public void ContinueGame()
    {
        UnPause();
    }

    //重新开始游戏
    public void NewGame()
    {
        foreach (GameObject targetGO in targetGOs)
        {
            targetGO.GetComponent<TargetManager>().UpdateMonsters();
        }
        UIManager._instance.shootNum = 0;
        UIManager._instance.score = 0;
        UIManager._instance.ShowMessage("");
        UnPause();
    }

    //退出游戏
    public void QuitGame()
    {
        Application.Quit();
    }

    //保存游戏
    public void SaveGame()
    {
        //SaveByBin();
        //SaveByJson();
        SaveByXML();
    }

    //加载游戏
    public void LoadGame()
    {
        //LoadByBin();
        //LoadByJson();
        LoadByXML();
    }
}
