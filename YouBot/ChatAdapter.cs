
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
	class ChatAdapter : BaseAdapter
	{
		Context context;
		ChatMessage[] ListData;

		public ChatAdapter (Context context, ChatMessage[] ListData) : base()
		{
			this.context = context;
			this.ListData = ListData;
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return null;
		}

		public override long GetItemId (int position)
		{
			return 0;
		}

		public override int Count {
			get {
				return ListData.Count();
			}
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			ChatMessage temp = ListData[position];

			LayoutInflater inflater = ((Activity)context).LayoutInflater;
			View row = inflater.Inflate(temp.Resource, parent, false);

			row.FindViewById<TextView>(Resource.Id.message_content).Text = temp.Message;

			return row;
		}
	}
}

