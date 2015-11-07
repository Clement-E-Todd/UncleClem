using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Adventure : MonoBehaviour
{

	System.Random random = new System.Random();
	public string seedPassword = "";

	List<Culture> cultures = new List<Culture>();

	void Start() {
		if (seedPassword != "")
		{
			random = new System.Random(seedPassword.GetHashCode());
		}

		GenerateSetting();
	}

	void GenerateSetting()
	{
		/*
		 * SUMMARY
		 * -Time period (past, modern, futuristic)
		 * -Balance of technology and magic
		 * -Geography
		 * 		-Number of distinct regions
		 * 		-For each region:
		 * 			-Size of region (measured in grid spaces taken up on world map)
		 * -Cultures
		 * 		-Number of cultures
		 * 		-For each culture:
		 * 			-Size of culture
		 * 			-Balance between religion, science, art and economy
		 * 			-Government
		 * 				-Republic
		 * 				-Monarchy
		 * 				-Democracy
		 * 				-Extreme cultural governments (if previous balance leans heavily in one direction)
		 * 					-Theocracy (religion)
		 * 					-Technocrcy (science)
		 * 					-Geniocracy (art)
		 * 					-Bankocracy (economy)
		 * 			-Trust of other cultures
		 */

		char timePeriod = (char)random.Next(3);
		float techAndMagic = (float)random.NextDouble();
		Debug.Log("The story is set during a " +
		          ((timePeriod == 0) ? "past" : (timePeriod == 1 ? "modern" : "futuristic")) + " time period in a " +
		          ((techAndMagic < 0.2) ? "mechanically/technologically advanced" :
		          (techAndMagic < 0.4) ? "largely technological" :
		          (techAndMagic < 0.6) ? "mechanically and magically balanced" :
		          (techAndMagic < 0.8) ? "largely magical" :
		 		  "magical") + 
		          " world.");

		int peopleToAssign = 100;

		while (peopleToAssign > 0)
		{
			Culture culture = new Culture(peopleToAssign, random);
			cultures.Add(culture);
			peopleToAssign -= culture.population;
		}
	}
}
