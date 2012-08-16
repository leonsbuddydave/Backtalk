
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
using Android.Util;
using Android.Accounts;

namespace YouBot
{
	[Activity (Label = "LearningPage")]			
	public class LearningPage : Activity
	{

		private const string TAG = "LearningPage";

		private CheckBox smsBox, facebookBox, gmailBox, twitterBox, tumblrBox;

		public bool[] ready = { true, true, true, true, true };

		private ProgressDialog dialog;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView(Resource.Layout.LearningPage);

			Global.UpdateContext(this);

			smsBox = FindViewById<CheckBox>(Resource.Id.smsCheckBox);
			facebookBox = FindViewById<CheckBox>(Resource.Id.facebookCheckBox);
			gmailBox = FindViewById<CheckBox>(Resource.Id.gmailCheckBox);
			twitterBox = FindViewById<CheckBox>(Resource.Id.twitterCheckBox);
			tumblrBox = FindViewById<CheckBox>(Resource.Id.tumblrCheckBox);

			FindViewById<Button>(Resource.Id.learnButton).Click += (sender, e) => Learn ();

			gmailBox.Click += (sender, e) => GmailPermissions();

			twitterBox.Click += (sender, e) => TwitterPermissions();
		}

		public void TwitterPermissions ()
		{
			if (!twitterBox.Checked)
				return;

			ISharedPreferences prefs = GetSharedPreferences(Global.PREFS_NAME, 0);
			string AccountName = prefs.GetString ("TwitterName", "");

			Dialog dialog = new Dialog(this);
			dialog.SetContentView(Resource.Layout.TwitterPopup);
			EditText twitterNameBox = (EditText)dialog.FindViewById(Resource.Id.twitterNameBox);
			twitterNameBox.Text = AccountName;
			dialog.SetTitle("Twitter");
			dialog.SetCancelable(false);

			dialog.FindViewById(Resource.Id.twitterCancel).Click += delegate {
				twitterBox.Checked = false;
				dialog.Dismiss();
			};
			dialog.FindViewById(Resource.Id.twitterConfirm).Click += delegate {
				AccountName = twitterNameBox.Text;
				ISharedPreferencesEditor editor = prefs.Edit ();
				editor.PutString("TwitterName", AccountName);
				editor.Commit();
				dialog.Dismiss();
				Log.Debug ( TAG, AccountName );
			};
			dialog.Show();
		}

		public void GmailPermissions ()
		{
			if (!gmailBox.Checked)
				return;

			ISharedPreferences prefs = GetSharedPreferences (Global.PREFS_NAME, 0);
			string AccountName = prefs.GetString ("GmailName", "");

			if (AccountName == "")
			{
				// Pulls all on-phone accounts that resemble emails
				// and makes an array of the names
				Java.Util.Regex.Pattern emailPattern = Patterns.EmailAddress;
				Account[] accounts = AccountManager.Get (this).GetAccounts ();
				List<string> accountNames = new List<string> ();
				foreach (Account account in accounts)
				{
					if (emailPattern.Matcher (account.Name).Matches ())
						accountNames.Add (account.Name);
				}

				// If we have any account names
				if (accountNames.Count > 0)
				{
					if (accountNames.Count == 1)
					{
						// If we only have one name available, just use it
						AccountName = accountNames [0];
						return;
					}

					// Otherwise, ask the user which account they would like to use
					AlertDialog.Builder ad = new AlertDialog.Builder (this);

					ad.SetTitle ("Choose an account to use with FlyoverChic.");

					ad.SetCancelable (false);

					int choice = 0;

					ad.SetSingleChoiceItems (accountNames.ToArray (), 0, delegate(object sender, DialogClickEventArgs e) {
						choice = e.Which;
					})
					.SetPositiveButton ("Got it!", delegate(object sender, DialogClickEventArgs e) {
						AccountName = accountNames [choice];
						Log.Debug (TAG, AccountName);
					})
					.SetNegativeButton ("Never mind.", delegate(object sender, DialogClickEventArgs e) {
						AccountName = "";
						gmailBox.Checked = false;
					});
					ad.Show ();
				}
				else
				{
					gmailBox.Checked = false;
				}
				// If we get here, it means there were no available accounts on the phone.
				// Turn to alternatives - like having the user register one or some shit
			}
		}

		public void Learn ()
		{
			if (smsBox.Checked)
			{
				new SmsReader (Global.context, Global.markovFactory, AllReady).Execute ();
				ready [0] = false;
			}

			if (facebookBox.Checked) {

				//ready [1] = false;
			}
			if (gmailBox.Checked) {
				// Not yet - we need to fight Google for this one
				//new GmailReader(Global.context, Global.markovFactory, AllReady).Execute();
				//ready [2] = false;
			}
			if (twitterBox.Checked) {
				new TwitterReader(Global.context, Global.markovFactory, AllReady).Execute();
				ready [3] = false;
			}
			if (tumblrBox.Checked) {

				//ready [4] = false;
			}

			dialog = new ProgressDialog (this);
			dialog.SetMessage ("Learning...");
			dialog.SetCancelable(false);
			dialog.Show ();
		}

		public void AllReady (ReaderTag tag)
		{
			Log.Debug (TAG, "All ready.");

			ready[(int)tag] = true;

			foreach (bool flag in ready)
			{
				if (!flag)
					return;
			}

			Global.markovFactory.SaveChain();

			dialog.Dismiss();
		}
	}
}

