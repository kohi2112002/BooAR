using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchObj : MonoBehaviour {

    public GameObject[] go;
    int id = 0;
    private void Start()
    {
        
        Active();
    }

    void Update()
    {
        
        if ((Input.touchCount >0 && Input.GetTouch(0).phase == TouchPhase.Began)||(Input.GetMouseButtonDown(0)))
                Active();
            


        

        /*else if (Input.touchCount == 2)
        {


            if (Input.GetTouch(1).phase == TouchPhase.Began)
            {
                initPosDist = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                initPos = CamTrans.localPosition;
            }
            if ((Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) && (Input.GetTouch(1).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Stationary))
            {
                float PosDist = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                float posMult = Mathf.Abs(PosDist / initPosDist);
                CamTrans.localPosition = new Vector3(CamTrans.localPosition.x, CamTrans.localPosition.y, Mathf.Clamp((1 / posMult) * initPos.z, posMin, posMax));
            }
        }*/
    }

    void Active()
    {
        foreach (GameObject i in go) i.SetActive(false);
        go[id].SetActive(true);
        if (id >= go.Length - 1) id = 0; else id++;
    }
}
