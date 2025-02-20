using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEnabler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EnableEnemy(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        DisableEnemy(other.gameObject);
    }

    public static void DisableEnemy(GameObject Enemy)
    {
        if (Enemy.tag == "Enemy")
        {
            if (Enemy.GetComponent<t800_soul>() != null)
            {
                Enemy.GetComponent<t800_soul>().OutOfRange();
                Enemy.GetComponent<t800_soul>().enabled = false;
            }
            else
            {
                Enemy.GetComponent<t200_soul>().OutOfRange();
                Enemy.GetComponent<t200_soul>().enabled = false;
            }

            Enemy.GetComponent<Animator>().enabled = false;
        }
    }

    public static void EnableEnemy(GameObject Enemy)
    {
        if (Enemy.tag == "Enemy")
        {
            if (Enemy.GetComponent<t800_soul>() != null)
            {
                Enemy.GetComponent<t800_soul>().enabled = true;
                Enemy.GetComponent<t800_soul>().InOfRange();
            }
            else
            {
                Enemy.GetComponent<t200_soul>().enabled = true;
                Enemy.GetComponent<t200_soul>().InOfRange();
            }

            Enemy.GetComponent<Animator>().enabled = true;
        }
    }
}
