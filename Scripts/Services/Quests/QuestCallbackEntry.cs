using System;
using Server.ContextMenus;

namespace Server.Engines.Quests
{
    public class QuestCallbackEntry : ContextMenuEntry
    {
        private readonly QuestCallback m_Callback;

        public QuestCallbackEntry(int number, QuestCallback callback) : this(number, -1, callback)
        {
        }

        public QuestCallbackEntry(int number, int range, QuestCallback callback) : base(number, range)
        {
            this.m_Callback = callback;
        }

        public override void OnClick()
        {
            if (this.m_Callback != null)
                this.m_Callback();
        }
    }
}