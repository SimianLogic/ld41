using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SushiController : MonoBehaviour 
{
    public int cashMoney; //set in editor -- starting cash
    private bool checkingForSushi = false;

    public SushiPrefab sushiPrefab;
    public SuperMetaNode instructions;
    private SuperMetaNode metaNode;

    public List<Sushi> sushi;
    
    private List<SushiPrefab> orders;
    public SuperContainer queueContainer;

    //this is lame but it's quick!
    public Sushi rice;
    public Sushi uni;
    public Sushi maguro;
    public Sushi sake;
    public Sushi ika;
    public Sushi hamachi;
    public Sushi ebi;

    public Dictionary<Sushi, int> inventory;
    public Dictionary<int,Sushi> activeIngredients;

    public List<int> selected;

	// Use this for initialization
	void Start() 
    {
        orders = new List<SushiPrefab>();

		metaNode = GetComponent<SuperMetaNode>();
        queueContainer = metaNode.ContainerWithName("order_queue");

        inventory = new Dictionary<Sushi, int>();

        foreach(Sushi fish in sushi)
        {
            inventory[fish] = 0;
        }
        inventory[rice] = 0;

        AddSushiToQueue();
        AddSushiToQueue();
        AddSushiToQueue();
        AddSushiToQueue();

        foreach(SuperButtonBase button in metaNode.buttons.Values)
        {
            button.onClick += HandleButton;
        }

        Refresh();

        selected = new List<int>();
        activeIngredients = new Dictionary<int, Sushi>();
        for(var i = 1; i <= 9; i++)
        {
            activeIngredients[i] = null;
        }
        
        Fill();
	}

    public void HandleInstructions()
    {
        instructions.Get<SuperTab>("tab_instructions").Cycle();
        if(instructions.Get<SuperTab>("tab_instructions").currentState == "1")
        {
            instructions.gameObject.SetActive(false);
        }
    }

    void Refresh()
    {
        metaNode.LabelWithName("total_dollars").text = "$" + cashMoney;

        int total_inventory = 0;
        foreach(int fish_count in inventory.Values)
        {
            total_inventory += fish_count;
        }
        metaNode.LabelWithName("count_all").text = total_inventory + " PIECES";

        foreach(Sushi fish in sushi)
        {
            metaNode.LabelWithName("count_" + fish.english).text = inventory[fish] + " PIECES";
        }
        metaNode.LabelWithName("count_rice").text = inventory[rice] + " PIECES";
    }

    void RefreshIngredients()
    {
        List<int> keys = new List<int>(activeIngredients.Keys);
        foreach(int key in keys)
        {
            activeIngredients[key] = null;
        }
        Fill();
    }
    void Fill()
    {
        //build the available supply
        List<Sushi> supply_list = new List<Sushi>();
        foreach(Sushi fish in inventory.Keys)
        {
            Debug.Log(fish.name + ": " + inventory[fish]);
            int how_many = Mathf.Min(fish.maxActive, inventory[fish]);
            for(int i = 0; i < how_many; i++)
            {
                supply_list.Add(fish);
            }
        }

        //subtract any that have already been placed
        foreach(Sushi fish in activeIngredients.Values)
        {
            if(fish != null)
            {
                supply_list.Remove(fish);
            }            
        }

        //fill in randomly
        List<int> keys = new List<int>(activeIngredients.Keys);
        foreach(int key in keys)
        {
            //deactivate any that were selected
            metaNode.SpriteWithName("fill_" + key).gameObject.SetActive(false);

            if(activeIngredients[key] == null)
            {

                if(key == 5 && supply_list.IndexOf(rice) >= 0)
                {
                    int which = supply_list.IndexOf(rice);
                    Sushi fish = supply_list[which];
                    supply_list.RemoveAt(which);
                    activeIngredients[key] = fish;
                    metaNode.LabelWithName("label_" + key).text = fish.fish;
                    metaNode.Get<SuperTab>("tab_sushi_" + key).currentState = fish.name;
                } else if(supply_list.Count > 0){
                    int which = Random.Range(0, supply_list.Count);
                    Sushi fish = supply_list[which];
                    supply_list.RemoveAt(which);
                    activeIngredients[key] = fish;
                    metaNode.LabelWithName("label_" + key).text = fish.fish;
                    metaNode.Get<SuperTab>("tab_sushi_" + key).currentState = fish.name;
                }else{
                    metaNode.LabelWithName("label_" + key).text = "EMPTY";
                    metaNode.Get<SuperTab>("tab_sushi_" + key).currentState = "none";
                }
            }
        }
    }

    void Purchase(Sushi fish)
    {
        if(cashMoney > fish.fishPrice)
        {
            cashMoney -= fish.fishPrice;
            inventory[fish] += fish.quantity;
        }
        //RefreshIngredients();
        Fill();
        Refresh();
    }

    IEnumerator CheckForSushi()
    {
        if(checkingForSushi)
        {
            yield break;
        }

        if(selected.Count != 2)
        {
            yield break;
        }

        checkingForSushi = true;
        yield return new WaitForSeconds(0.5f);

        int first = selected[0];
        int second = selected[1];
        selected.Clear();

        Sushi fish = null;
        if(activeIngredients[first] == rice)
        {
            if(activeIngredients[second] == rice)
            {
                //DOUBLE RICE
                BadSushi();
            }else{
                Debug.Log("FISH 2nd");
                fish = activeIngredients[second];
            }
                
        }else if(activeIngredients[second] == rice){
            Debug.Log("FISH 1st");
            fish = activeIngredients[first];
        }else{
            Debug.Log("DOUBLE FISH");
            BadSushi();
        }

        inventory[activeIngredients[first]] -= 1;
        inventory[activeIngredients[second]] -= 1;

        activeIngredients[first] = null;
        activeIngredients[second] = null;

        if(fish != null)
        {
            foreach(SushiPrefab sp in orders)
            {
                if(sp.sushi == fish && sp.count < 2)
                {
                    sp.count += 1;
                    sp.Refresh();
                    break;
                }
            }
        }

        
        Refresh();
        Fill();
        checkingForSushi = false;
    }

    void BadSushi()
    {

    }

    void Toggle(int which)
    {
        Debug.Log("TOGGLE " + which + "  " + activeIngredients[which]);
        if(selected.IndexOf(which) >= 0)
        {
            Debug.Log("UNTOGGLE " + which);
            selected.Remove(which);
            metaNode.SpriteWithName("fill_" + which).gameObject.SetActive(false);
        }else{
            Debug.Log("TURN ON " + which);
            if(activeIngredients[which] != null)
            {
                Debug.Log("activeIngredients[which] = " + activeIngredients[which] + "   " + (activeIngredients[which] == null));
                selected.Add(which);
                metaNode.SpriteWithName("fill_" + which).gameObject.SetActive(true);
                StartCoroutine(CheckForSushi());
            } else
            {
                Debug.Log("null");
            }
            
        }
    }

    void HandleButton(SuperButtonBase button)
    {
        switch(button.name)
        {
            case "scalebtn_1":
                Toggle(1);
                break;
            case "scalebtn_2":
                Toggle(2);
                break;
            case "scalebtn_3":
                Toggle(3);
                break;
            case "scalebtn_4":
                Toggle(4);
                break;
            case "scalebtn_5":
                Toggle(5);
                break;
            case "scalebtn_6":
                Toggle(6);
                break;
            case "scalebtn_7":
                Toggle(7);
                break;
            case "scalebtn_8":
                Toggle(8);
                break;
            case "scalebtn_9":
                Toggle(9);
                break;

            case "scalebtn_urchin":
                Purchase(uni);
                break;
            case "scalebtn_tuna":
                Purchase(maguro);
                break;
            case "scalebtn_salmon":
                Purchase(sake);
                break;
            case "scalebtn_squid":
                Purchase(ika);
                break;
            case "scalebtn_yellowtail":
                Purchase(hamachi);
                break;
            case "scalebtn_shrimp":
                Purchase(ebi);
                break;
            case "scalebtn_rice":
                Purchase(rice);
                break;

            case "scalebtn_refresh":
                if(cashMoney >= 1)
                {
                    cashMoney -= 1;
                    //reroll
                    RefreshIngredients();
                    Refresh();
                }
                break;

            default:
                Debug.Log("TODO: " + button.name);
                break;
        }
    }
	
	void AddSushiToQueue()
    {
        int which = Random.Range(0, sushi.Count);
        
        SushiPrefab order = Instantiate(sushiPrefab, Vector3.zero, Quaternion.identity) as SushiPrefab;
        order.sushi = sushi[which];
        order.Refresh();

        order.transform.SetParent(queueContainer.transform);
        orders.Add(order);
    }


    private void Update()
    {
        if(instructions.gameObject.activeSelf)
        {
            return;
        }

        SushiPrefab donezo = null;
        SushiPrefab dead = null;
        foreach(SushiPrefab sp in orders)
        {
            sp.UpdateTimer();

            if(sp.count == 2)
            {
                if(sp.victoryTimer > 0)
                {
                    sp.victoryTimer -= Time.deltaTime;
                }else{
                    donezo = sp;
                }
                
            }else if(sp.serviceTimer < 0){
                dead = sp;
            }
        }

        if(donezo != null)
        {
            Debug.Log("DONEZO: " + donezo.sushi + "   " + donezo.sushi.nigiriPrice);
            cashMoney += donezo.sushi.nigiriPrice;
            orders.Remove(donezo);
            Destroy(donezo.gameObject);
            AddSushiToQueue();
            Refresh();
        }else if(dead != null){
            orders.Remove(dead);
            Destroy(dead.gameObject);
            AddSushiToQueue();
            Refresh();
        }
    }


}
