using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SushiPrefab : MonoBehaviour 
{
    private SuperMetaNode metaNode;
    public Sushi sushi;

    public string frontFish = "none";
    public bool frontRice;
    public string backFish = "none";
    public bool backRice;

    public int count = 0;
    public float victoryTimer = 1.0f;

    public float MAX_SERVICE_TIMER = 60f;
    public float serviceTimer = 60f;

    public SuperLabel fishLabel
    {
        get
        {
            return metaNode.LabelWithName("sushi_label");
        }
    }
    public SuperLabel countLabel
    {
        get
        {
            return metaNode.LabelWithName("sushi_count");
        }
    }
    public SuperLabel priceLabel
    {
        get
        {
            return metaNode.LabelWithName("sushi_price");
        }
    }

	// Use this for initialization
	void Start()
    {
		metaNode = GetComponent<SuperMetaNode>();
	}

    public void UpdateTimer()
    {
        serviceTimer -= Time.deltaTime;
        float pct = Mathf.Max(0f, serviceTimer)/MAX_SERVICE_TIMER;
        metaNode.ContainerWithName("bar").GetComponent<RectTransform>().localScale = new Vector2(pct, 1f);
    }

    public void Refresh()
    {
        metaNode = GetComponent<SuperMetaNode>();

        metaNode.Get<SuperTab>("tab_front_sushi").currentState = sushi.name;
        metaNode.Get<SuperTab>("tab_back_sushi").currentState = sushi.name;
        

        fishLabel.text = sushi.fish + " NIGIRI";
        countLabel.text = count + "/2";
        priceLabel.text = "$" + sushi.nigiriPrice;

    }
}
