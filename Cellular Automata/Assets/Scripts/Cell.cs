using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField]private bool isAlive;
    public int X
    {
        get; set;
    }

    public int Y
    {
        get; set;
    }

    public bool IsAlive
    {
        get { return isAlive; } set { isAlive = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
