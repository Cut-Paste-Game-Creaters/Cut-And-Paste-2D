using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    GameObject throwObject;     //‘å–C‚©‚ç‘Å‚Âƒ‚ƒm

    public GameObject canon_body;   //‘å–C‚ÌŠp“x‚ğ•Ï‚¦‚é‚½‚ß
    private bool isRigid = false;   //gameObject‚Érigidoby2D‚ª‚ ‚é‚©‚Ç‚¤‚©
    [SerializeField] float angle = 0;            //-90 ` 0 `@90
    [SerializeField] float firePower = 1.0f;     //‘Å‚¿o‚·—Í
    [SerializeField] float fireTime = 3.0f;

    private float wholeTime = 0.0f;
    private Rigidbody2D rb;

    void Start()
    {
        Vector3 rot = canon_body.transform.localEulerAngles;
        rot.z = angle;
        canon_body.transform.localEulerAngles = rot;

        if(throwObject.GetComponent<Rigidbody2D>() != null)
        {
            isRigid = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rot = canon_body.transform.localEulerAngles;
        rot.z = angle;
        canon_body.transform.localEulerAngles = rot;

        wholeTime += Time.deltaTime;

        if(wholeTime > fireTime)
        {
            wholeTime = 0;
            if (isRigid)
            {
                //Šp“x’²®
                float _angle = angle + 90;
                //Œü‚¢‚½•ûŒü‚É¶¬‚·‚é
                Vector2 moveDir = new Vector2(
                    Mathf.Cos(_angle * Mathf.Deg2Rad) * firePower, Mathf.Sin(_angle * Mathf.Deg2Rad) * firePower
                    );
                //Œü‚¢‚½•ûŒü‚ÉˆÊ’u‚ğ‘«‚·
                Vector3 pos = this.transform.position;
                pos.x += moveDir.normalized.x;
                pos.y += moveDir.normalized.y;

                //’e‚ğ¶¬‚µ‚ÄŒü‚«‚ğ“`‚¦‚é
                GameObject g = Instantiate(throwObject, pos,
                    Quaternion.identity);
                ThrowObjectController bc = g.GetComponent<ThrowObjectController>();
                bc.SetDir(moveDir.normalized);

                //Šp“x‚ğ®‚¦‚é
                Vector3 fire_rot = g.transform.localEulerAngles;
                fire_rot.z = angle;
                g.transform.localEulerAngles = fire_rot;
            }
            else
            {

            }
        }
    }
}
