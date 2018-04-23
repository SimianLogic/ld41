using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Sushi/Make Sushi")]
public class Sushi : ScriptableObject 
{
    //name is lowercase
    public bool canHasNigiri;
    public int nigiriPrice;

    public string fish;//fish is UPPERCASE
    public string english;
    public int fishPrice;
    public int quantity = 10;  //how much do you get for buying it?
    public int maxActive; //3 for most things, 5 for rice
}
