using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	class GmailReader : AsyncTask<string, int, List<string> >
	{
		public delegate void Callback(ReaderTag tag);

		private string TAG = "GmailReader";
		private Context context;
		private MarkovFactory markovFactory;
		public List<string> emailList;
		private Callback callback;

		public GmailReader (Context context, MarkovFactory markovFactory, Callback callback)
		{
			this.context = context;
			this.markovFactory = markovFactory;
			this.callback = callback;
		}

		protected override void OnPreExecute ()
		{

		}

		protected override List<string> RunInBackground(params string[] @params)
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

			return emailList;
		}

		protected override void OnPostExecute (Java.Lang.Object result)
		{
			base.OnPostExecute (result);
			markovFactory.AddFromList( emailList );
			Global.UpdateTime(ReaderTag.GMAIL);
			callback(ReaderTag.GMAIL);
		}
	}
}

