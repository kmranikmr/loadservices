using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.messages
{
    public abstract class MsgUserId
    {
        public readonly int userId;

        public MsgUserId(int userId = 0)
        {
            this.userId = userId;
        }
    }

}
