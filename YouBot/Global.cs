
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace YouBot
{

	public enum ReaderTag
	{
		SMS = 0,
		FACEBOOK = 1,
		GMAIL = 2,
		TWITTER = 3,
		TUMBLR = 4
	};

	static class Global
	{
		public static Context context;
		public static MarkovFactory markovFactory;
		public static string PREFS_NAME = "PREFERENCES";

		public static string[] ReaderTagText = { "SMS", "Facebook", "Gmail", "Twitter", "Tumblr" };

		// Twitter Shit
		public static string TwitterConsumerKey = "3dpdf19piemo6pwFlLFSuA";
		public static string TwitterConsumerSecret = "aE1bJ3FNkcEKSClq4PBQeMm7Q0m72UrWGNp8J9KhI";
		public static string TwitterAccessToken = "";
		public static string TwitterAccessTokenSecret = "";

		public static void UpdateContext(Context context)
		{
			Global.context = context;
			Global.markovFactory.context = context;
		}

		public static bool FirstRun ()
		{
			ISharedPreferences prefs = context.GetSharedPreferences (Global.PREFS_NAME, 0);
			bool isFirstRun = prefs.GetBoolean ("firstRun", true);

			if (isFirstRun)
			{
				ISharedPreferencesEditor editor = prefs.Edit();
				editor.PutBoolean("firstRun", false);
				editor.PutLong ("lastSMSUpdate", 0);
				editor.PutLong ("lastFacebookUpdate", 0);
				editor.PutLong ("lastGmailUpdate", 0);
				editor.PutLong ("lastTwitterUpdate", 0);
				editor.PutLong ("lastTumblrUpdate", 0);
				//editor.Commit();
			}
			return isFirstRun;
		}

		public static long CurrentTime ()
		{
			return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		}

		public static void UpdateTime (ReaderTag tag)
		{
			ISharedPreferencesEditor editor = context.GetSharedPreferences(Global.PREFS_NAME, 0).Edit();
			editor.PutLong ("last" + ReaderTagText[(int)tag] + "Update", Global.CurrentTime());
			editor.Commit();
		}

		public static long GetLastUpdateTime(ReaderTag tag)
		{
			ISharedPreferences prefs = context.GetSharedPreferences (Global.PREFS_NAME, 0);
			return prefs.GetLong("last" + ReaderTagText[(int)tag] + "Update", 0);
		}
	}
}