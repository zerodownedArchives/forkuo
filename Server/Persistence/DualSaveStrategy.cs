/***************************************************************************
*                            DualSaveStrategy.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: DualSaveStrategy.cs 642 2010-12-20 11:31:46Z asayre $
*
***************************************************************************/

/***************************************************************************
*
*   This program is free software; you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation; either version 2 of the License, or
*   (at your option) any later version.
*
***************************************************************************/

using System;
using System.Threading;

namespace Server
{
    public sealed class DualSaveStrategy : StandardSaveStrategy
    {
        public override string Name
        {
            get
            {
                return "Dual";
            }
        }

        public DualSaveStrategy()
        {
        }

        public override void Save(SaveMetrics metrics, bool permitBackgroundWrite) 
        {
            this.PermitBackgroundWrite = permitBackgroundWrite;

            Thread saveThread = new Thread(delegate()
            {
                this.SaveItems(metrics);
            });

            saveThread.Name = "Item Save Subset";
            saveThread.Start();

            this.SaveMobiles(metrics);
            this.SaveGuilds(metrics);
            this.SaveCores(metrics);
            this.SaveModules(metrics);
            this.SaveServices(metrics);

            saveThread.Join();

            if (permitBackgroundWrite && this.UseSequentialWriters)	//If we're permitted to write in the background, but we don't anyways, then notify.
                World.NotifyDiskWriteComplete();
        }
    }
}