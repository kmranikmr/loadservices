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
