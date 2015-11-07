using UnityEngine;

public enum governmentType
{
	nill = -1,
	republic = 0,
	monarchy = 1,
	aristocracy = 2,
	theocracy = 3,
	technocracy = 4,
	geniocracy = 5,
	bankocracy = 6
}

public enum culturalSociability
{
	nill = -1,
	cautious = 0,
	neautral = 1,
	friendly = 2
}

public class Culture {
	public string name = "";
	public int population = 0;
	public governmentType government = governmentType.nill;
	public culturalSociability sociability = culturalSociability.nill;

	// Balance of cultural values (5 points are distributed between the following 4 values)
	public int religion = 0;
	public int science = 0;
	public int art = 0;
	public int economy = 0;

	public Culture(int peopleAvailable, System.Random randomSeed)
	{
		population = Mathf.Min(randomSeed.Next(10, 101), peopleAvailable);
		if (peopleAvailable - population < 10) population = peopleAvailable;
		
		for (int i = 0; i < 5; i++)
		{
			int balancePoint = randomSeed.Next(4);
			if (balancePoint == 0) religion++;
			else if (balancePoint == 1) science++;
			else if (balancePoint == 2) art++;
			else if (balancePoint == 3) economy++;
		}
		
		if (religion >= 3) government = governmentType.theocracy;
		else if (science >= 3) government = governmentType.technocracy;
		else if (art >= 3) government = governmentType.geniocracy;
		else if (economy >= 3) government = governmentType.bankocracy;
		else
		{
			int randGovernment = randomSeed.Next(3);
			if (randGovernment == 0) government = governmentType.monarchy;
			else if (randGovernment == 1) government = governmentType.republic;
			else if (randGovernment == 2) government = governmentType.aristocracy;
		}

		string[] governmentString =  new string[] {"republic", "monarchy", "aristocracy", "theocracy", "technocracy", "geniocracy", "bankocracy"};

		int randSociability = randomSeed.Next(3);
		if (randSociability == 0) sociability = culturalSociability.cautious;
		else if (randSociability == 1) sociability = culturalSociability.neautral;
		else if (randSociability == 2) sociability = culturalSociability.friendly;

		string[] socialString = new string[] {"cautious", "neautral", "friendly"};
		
		Debug.Log("CULTURE " +
		          "\npopulation: " + population +
		          "\nbalance of cultural aspects:" +
		          "\n   religion: " + religion*20 + "%" +
		          "\n   science: " + science*20 + "%" +
		          "\n   art: " + art*20 + "%" +
		          "\n   economy: " + economy*20 + "%" +
			      "\ngovernment: " + governmentString[(int)government] +
		          "\nsociability: " + socialString[(int)sociability]);
	}
}
