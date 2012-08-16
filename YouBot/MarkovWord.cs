using System;
using System.Linq;
using System.Collections.Generic;

using Android.Util;

namespace YouBot
{
	public class MarkovWord
	{

		private const string TAG = "MarkovWord";

		public string word;

		public Dictionary<MarkovWord, int> afters;

		public MarkovWord (String newWord)
		{
			afters = new Dictionary<MarkovWord, int>();
			this.word = newWord;
		}

		public MarkovWord GetWord ()
		{
			double instanceTotal = 0;
			foreach (var v in afters.Values)
			{
				instanceTotal += v;
			}

			Random rand = new Random();

			double chance = rand.NextDouble ();
			double curLevel = 0.0;

			//Log.Debug(TAG, "Chance: " + chance);

			foreach (var k in afters.Keys)
			{
				curLevel += afters[k] / instanceTotal;
				//Log.Debug( TAG, k.word + " : " + (afters[k] / instanceTotal) );
				if (curLevel > chance)
				{
					return k;
				}
			}
			return null;
		}

		public MarkovWord GetMostCommon ()
		{
			MarkovWord mostCommon = null;
			foreach (var k in afters.Keys)
			{
				mostCommon = k;
				break;
			}
			foreach (var k in afters.Keys)
			{
				if ( afters[k] > afters[mostCommon] )
					mostCommon = k;
			}
			return mostCommon;
		}

		public MarkovWord GetRandomAfter ()
		{
			Random rand = new Random();
			return afters.ElementAt (rand.Next (0, afters.Count)).Key;
		}
	}
}

