
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
	enum People
	{
		YOU,
		HIM,
		ALERT
	};

	class ChatMessage
	{
		public string Message;
		public People Owner;
		public int Resource;

		public ChatMessage ()
		{

		}

		public ChatMessage(string msg, People owner)
		{
			this.Message = msg;
			this.Owner = owner;
			SetResource();
		}

		private void SetResource ()
		{
			switch (Owner)
			{
			case People.YOU:
				Resource = YouBot.Resource.Layout.chat_item_you;
				break;
			case People.HIM:
				Resource = YouBot.Resource.Layout.chat_item_him;
				break;
			}
		}
	}
}

