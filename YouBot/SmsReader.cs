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
using Android.Database.Sqlite;
using Android.Database;
using Android.Net;
using Android.Util;

namespace YouBot
{
	class SmsReader : AsyncTask<string, int, List<string> >
	{
		private string TAG = "SmsReader";
		private Context context;
		private MarkovFactory markovFactory;
		public List<string> smsList;

		public SmsReader (Context context, MarkovFactory markovFactory)
		{
			this.context = context;
			this.markovFactory = markovFactory;
		}

		protected override void OnPreExecute ()
		{

		}

		protected override List<string> RunInBackground(params string[] @params)
		{
			smsList = new List<string>();
			Android.Net.Uri urisms = Android.Net.Uri.Parse("content://sms/sent");
			ICursor smsCursor = context.ContentResolver.Query(urisms, new string[]{ "body", "date" }, null, null, null);
			smsCursor.MoveToFirst();
			do
			{
				smsList.Add( smsCursor.GetString(0) );
				//Log.Debug( TAG, smsCursor.GetString (1) );
				smsCursor.MoveToNext();
			} while (!smsCursor.IsAfterLast);

			return smsList;
		}

		protected override void OnPostExecute (Java.Lang.Object result)
		{
			base.OnPostExecute (result);
			Log.Debug ( TAG, "OnPostExecute firing" );
			new Dialog(context).Show ();
			markovFactory.AddFromList( smsList );
			Global.UpdateTime(Global.ReaderTag.SMS);
		}
	}
}

