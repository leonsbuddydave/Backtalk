using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using Mono.Data.Sqlite;
using Android.Database.Sqlite;
using Android.Database;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace YouBot
{
	class MarkovFactory
	{
		private const string TAG = "MarkovFactory";

		public const string SENTENCESTART = "ុ";

		public const string SENTENCEEND = ".";

		public Dictionary<string, MarkovWord> chain;

		public Context context;

		private string DBPath;

		public MarkovFactory(Context context)
		{
			this.context = context;
			chain = new Dictionary<string, MarkovWord>();
			DBPath = Path.Combine( System.Environment.GetFolderPath( System.Environment.SpecialFolder.Personal ), "chain.db3" );
			Log.Debug ( TAG, System.Environment.GetFolderPath( System.Environment.SpecialFolder.Personal ) );
		}

		public void AddFromList (List<string> input)
		{
			Log.Debug (TAG, "Adding from list of words.");
			int wordCount = 0;
			for (int j = 0; j < input.Count; j++) {
				string[] words = ( SENTENCESTART + " " + CleanInput (input[j]) + SENTENCEEND  ).Split (' ');

				for (int i = 0; i < words.Length - 1; i++) {
					string curWord = words [i].ToLower ();
					string nextWord = words [i + 1].ToLower ();

					// Debug, tracking how many words
					wordCount++;

					//Log.Debug (TAG, curWord);

					if (!chain.ContainsKey (curWord)) {
						MarkovWord link = new MarkovWord (curWord);
						chain.Add (curWord, link);
					}

					if (!chain.ContainsKey (nextWord)) {
						MarkovWord link = new MarkovWord (nextWord);
						chain.Add (nextWord, link);
					}

					MarkovWord after = chain [nextWord];
					if (!chain [curWord].afters.ContainsKey (chain [nextWord])) {
						chain [curWord].afters.Add (after, 1);
					} else {
						chain [curWord].afters [after]++;
					}
				}
			}
			Log.Debug(TAG, "Done importing list - " + wordCount + " words processed.");
		}

		public string CleanInput(string input)
		{
			input = input.Replace ("\r\n", "");
			input = input.Replace ("\n", "");
			input = input.Replace(". ", ".");
			input = input.Replace (".", " . ");
			return input;
		}

		public void SaveChain ()
		{
			Log.Debug (TAG, "Optimized save.");
			MarkovDBHandler dbh = new MarkovDBHandler(context);
			dbh.Save (this);
		}

		public void LoadChain ()
		{
			Log.Debug (TAG, "Load chain...");
			MarkovDBHandler dbh = new MarkovDBHandler(context);
			dbh.Load (this);
		}

		public bool SaveChainSlow ()
		{
			Log.Debug(TAG, "Saving to database.");
			bool exists = File.Exists (DBPath);

			if (!exists)
				SqliteConnection.CreateFile (DBPath);

			var connection = new SqliteConnection ("Data Source=" + DBPath);
			connection.Open ();

			if (!exists) {
				var commands = new[]{
					"CREATE TABLE [Words] (first NTEXT, second NTEXT, count INTEGER NOT NULL) "
				};

				foreach (var command in commands) {
					using (var c = connection.CreateCommand()) {
						c.CommandText = command;
						c.ExecuteNonQuery ();
					}
				}
			}

			Stopwatch stopwatch = Stopwatch.StartNew();

			using (var c = connection.CreateCommand())
			{
				c.CommandText = "INSERT INTO [Words] (first, second, count) VALUES(?, ?, ?)";
				IDbDataParameter first = c.CreateParameter ();
				IDbDataParameter second = c.CreateParameter ();
				IDbDataParameter count = c.CreateParameter ();
				c.Parameters.Add (first);
				c.Parameters.Add (second);
				c.Parameters.Add (count);
				foreach (KeyValuePair<string, MarkovWord> word in chain)
				{
					Log.Debug ( TAG, "Saving " + word.Value.word);
					foreach (KeyValuePair<MarkovWord, int> after in word.Value.afters)
					{
						Log.Debug ( TAG, "Saving " + after.Key.word );
						first.Value = word.Value.word;
						second.Value = after.Key.word;
						count.Value = after.Value;
						c.ExecuteNonQuery ();
					}
				}
			}
			stopwatch.Stop ();
			Log.Debug (TAG, "Database insert completed in " + stopwatch.Elapsed.TotalMilliseconds);
			//Log.Debug ( TAG, File.Exists (DBPath).ToString() );
			return true;
		}
	}
}