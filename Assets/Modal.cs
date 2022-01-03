using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Modal : MonoBehaviour
{
    public static Modal Instance;

    public Text GameName;
    public Text GamePlaytime;
    public Text GameGenres;

    public void Awake()
    {
        Instance = this;
    }
}
