using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {

    private void Start()
    {
        StartCoroutine("DestroySelf");
    }

    IEnumerator DestroySelf()
    {
        //等待2秒之后销毁自身
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }

}
