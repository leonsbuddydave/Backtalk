using System;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Util;
using System.Collections.Generic;
using System.Linq;

namespace YouBot
{
	[Activity (Label = "YouBot", MainLauncher = true)]
	public class Chat : Activity
	{
		private const string TAG = "CHAT";

		static int maxSentenceLength = 25;

		private ListView listView;

		private EditText InputBox;

		private List<ChatMessage> ChatMessages;

		private ChatAdapter listAdapter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			Global.markovFactory = new MarkovFactory (this);

			Global.UpdateContext (this);

			// TESTING: Load all Sms Messages
			//SmsReader smsReader = new SmsReader(this, markovFactory);
			//smsReader.Execute();
			if (Global.FirstRun ())
			{
				// Send to learning page if first run
				Intent i = new Intent(this, typeof(LearningPage));
				StartActivity(i);
			}
			else
			{
				Global.markovFactory.LoadChain ();
			}

			SetUpList();
		}

		private void SetUpList ()
		{
			listView = FindViewById<ListView>(Resource.Id.ChatList);

			InputBox = FindViewById<EditText>(Resource.Id.input);

			FindViewById<Button>(Resource.Id.send).Click += (sender, e) => SendMessage(sender, e);

			FindViewById<Button>(Resource.Id.learnButton).Click += (sender, e) => {
				Intent i = new Intent(this, typeof(LearningPage));
				StartActivity(i);
			};

			ChatMessages = new List<ChatMessage>();

			listAdapter = new ChatAdapter(this, ChatMessages.ToArray());

			listView.Adapter = listAdapter;

			listView.TranscriptMode = TranscriptMode.AlwaysScroll;
			listView.StackFromBottom = true;
		}

		private void SendMessage(object sender, EventArgs e)
		{
			// Do shit with the message
			if (InputBox.Text != "")
				PostMessage (InputBox.Text, People.YOU);
			GenerateResponse();
		}

		private void GenerateResponse()
		{
			Random rand = new Random ();
			StringBuilder sentence = new StringBuilder();

			//MarkovWord cur = markovFactory.chain.ElementAt (rand.Next (0, markovFactory.chain.Count)).Value;
			MarkovWord cur = Global.markovFactory.chain[MarkovFactory.SENTENCESTART];
			int words = 0;
			while (cur.word != "." && words < maxSentenceLength)
			{
				sentence.Append(cur.word).Append(" ");
				if (cur.afters.Count > 0)
					cur = cur.GetWord();
					//cur = Global.markovFactory.chain.ElementAt (rand.Next (0, Global.markovFactory.chain.Count)).Value;
				else
					break;

				words++;
			}

			PostMessage(sentence.ToString().Replace(MarkovFactory.SENTENCESTART, ""), People.HIM);
		}

		private void PostMessage (string Message, People person)
		{
			Log.Debug (TAG, Message);
			ChatMessages.Add ( new ChatMessage( Message, person) );

			listAdapter = new ChatAdapter(this, ChatMessages.ToArray());
			listView.Adapter = listAdapter;
		}
	}
}


