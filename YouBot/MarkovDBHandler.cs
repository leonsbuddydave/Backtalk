using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Android.Database.Sqlite;
using Android.Database;
using Mono.Data;
using Mono.Data.Sqlite;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace YouBot
{
	class MarkovDBHandler
	{
		private const string TAG = "MarkovDBHandler";
		private const string DATABASE_NAME = "chain";
		private const int DATABASE_VERSION = 1;
		private SQLiteDatabase myDatabase;
		public Context context;

		public MarkovDBHandler(Context context)
		{
			this.context = context;
		}

		public void Save (MarkovFactory factory)
		{
			Log.Debug ( TAG, "Saving chain." );
			myDatabase = context.OpenOrCreateDatabase(DATABASE_NAME, FileCreationMode.Private, null);
			myDatabase.ExecSQL("DROP TABLE IF EXISTS 'words'");
			myDatabase.ExecSQL("CREATE TABLE IF NOT EXISTS 'words' (firstWord TEXT, secondWord TEXT, instanceCount INTEGER NOT NULL);");

			var chain = factory.chain;
			DatabaseUtils.InsertHelper ih = new DatabaseUtils.InsertHelper (myDatabase, "words");

			Stopwatch stopwatch = Stopwatch.StartNew();
			StringBuilder insertQuery = new StringBuilder();

			//insertQuery.Append("INSERT INTO 'words' ('firstWord', 'secondWord', 'instanceCount') VALUES (?, ?, ?)");
			myDatabase.BeginTransaction();
			string sql = "INSERT INTO 'words' ('firstWord', 'secondWord', 'instanceCount') VALUES (?, ?, ?)";
			SQLiteStatement insert = myDatabase.CompileStatement(sql);

			foreach (KeyValuePair<string, MarkovWord> word in chain)
			{
				foreach (KeyValuePair<MarkovWord, int> after in word.Value.afters)
				{
					insert.BindString(1, word.Value.word);
					insert.BindString(2, after.Key.word);
					insert.BindDouble(3, after.Value);
					insert.Execute();
				}
			}
			myDatabase.SetTransactionSuccessful();
			myDatabase.EndTransaction();
			stopwatch.Stop ();
			Log.Debug (TAG, "Database insert completed in " + stopwatch.Elapsed.TotalMilliseconds);
		}

		public void Load (MarkovFactory factory)
		{
			Log.Debug ( TAG, "Loading." );
			myDatabase = context.OpenOrCreateDatabase (DATABASE_NAME, FileCreationMode.WorldReadable, null);
			ICursor cursor = myDatabase.RawQuery ("SELECT * FROM 'words'", null);

			int firstWordIndex = cursor.GetColumnIndex("firstWord");
			int secondWordIndex = cursor.GetColumnIndex("secondWord");
			int instanceCountIndex = cursor.GetColumnIndex("instanceCount");

			string firstWord, secondWord;
			int instanceCount;

			cursor.MoveToFirst ();
			while (!cursor.IsAfterLast)
			{
				firstWord = cursor.GetString ( firstWordIndex );
				secondWord = cursor.GetString ( secondWordIndex );
				instanceCount = cursor.GetInt( instanceCountIndex );

				if (!factory.chain.ContainsKey( firstWord ) )
				{
					MarkovWord link = new MarkovWord(firstWord);
					factory.chain.Add( firstWord, link );
				}

				MarkovWord afterWord;

				if (!factory.chain.ContainsKey ( secondWord ) )
				{
					afterWord = new MarkovWord ( secondWord );
					factory.chain.Add( secondWord, afterWord );
				}

				afterWord = factory.chain[secondWord];

				factory.chain[ firstWord ].afters.Add( afterWord, instanceCount );

				cursor.MoveToNext();
			}
		}

		public static string CleanInput(string input)
		{
			input = input.Replace ("\r\n", "");
			input = input.Replace ("\n", "");
			input = input.Replace(". ", ".");
			input = input.Replace (".", " . ");
			input = input.Replace ("(", "");
			input = input.Replace (")", "");
			input = input.Replace (";", "");
			input = input.Replace (":", "");
			input = input.Replace ("]", "");
			input = input.Replace ("[", "");
			input = input.Replace (",", "");
			input = input.Replace ("\"", "");
			input = input.Replace ("-", "");
			return input;
		}
	}
}

