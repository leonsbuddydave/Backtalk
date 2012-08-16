using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Json;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Database.Sqlite;
using Android.Net;
using Android.Util;

using Mono.Data;

namespace YouBot
{
	class TwitterReader : AsyncTask<string, int, List<string> >
	{

		public delegate void Callback(ReaderTag tag);

		private string TAG = "TwitterReader";
		private Context context;
		private MarkovFactory markovFactory;
		public List<string> tweetList;
		private Callback callback;

		private string TwitterStatusURL = "http://api.twitter.com/1/statuses/user_timeline.json?";
		private long maxID = 0;

		public TwitterReader (Context context, MarkovFactory markovFactory, Callback callback)
		{
			this.context = context;
			this.markovFactory = markovFactory;
			this.callback = callback;
		}

		public string CompileURL ()
		{
			ISharedPreferences prefs = context.GetSharedPreferences (Global.PREFS_NAME, 0);
			string ScreenName = prefs.GetString ("TwitterName", "");

			if (ScreenName == "") {
				// THERE GON BE PROBLEMS
				return null;
			} else {
				string url = TwitterStatusURL;
				url += "screen_name=";
				url += ScreenName;
				url += "&count=200";

				if (maxID != 0) {
					url += "&max_id=";
					url += maxID;
				}

				return url;
			}
		}

		protected override void OnPreExecute ()
		{

		}

		protected override List<string> RunInBackground (params string[] @params)
		{
			/*
			smsList = new List<string>();
			Android.Net.Uri urisms = Android.Net.Uri.Parse("content://sms/sent");
			ICursor smsCursor = context.ContentResolver.Query(urisms, new string[]{ "body", "date" }, null, null, null);
			smsCursor.MoveToFirst();
			do
			{
				smsList.Add( smsCursor.GetString(0) );
				smsCursor.MoveToNext();
			} while (!smsCursor.IsAfterLast);

			return smsList;
			*/
			tweetList = new List<string> ();
			bool done = false; // used to determine when we've collected all the tweets we can

			for (int i = 0; i < 17; i++)
			{
				var request = HttpWebRequest.Create (CompileURL ());
				request.ContentType = "application/json";
				request.Method = "GET";

				using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
				{
					if (response.StatusCode != HttpStatusCode.OK)
						Log.Debug (TAG, "Error fetching data. Status Code: " + response.StatusCode);

					using (StreamReader reader = new StreamReader(response.GetResponseStream()))
					{
						var content = reader.ReadToEnd ();
						if (string.IsNullOrWhiteSpace (content))
							Log.Debug (TAG, "Response was empty.");
						else
							ParseTweets (content);
					}
				}
			}
			return tweetList;
		}

		private void ParseTweets (string jsonString)
		{
			Log.Debug ( TAG, "Parsing tweet batch." );
			var obj = JsonObject.Parse (jsonString);

			int l = ((JsonArray)obj).Count ();
			for (int i = 0; i < l; i++)
			{
				tweetList.Add( obj[i]["text"] );
				long id = obj[i]["id"];
				if (maxID == 0 || id < maxID)
					maxID = id;
			}
		}

		private string CleanTweet (string tweet)
		{
			// TODO: Strip mentions, images and links.
			return "";
		}

		protected override void OnPostExecute (Java.Lang.Object result)
		{
			base.OnPostExecute (result);
			Log.Debug ( TAG, "Tweets Processed: " + tweetList.Count() );
			markovFactory.AddFromList( tweetList );
			Global.UpdateTime(ReaderTag.TWITTER);
			callback(ReaderTag.TWITTER);
		}
	}
}

