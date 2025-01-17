﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

#if PAL3

namespace Pal3.GameSystems.MiniGames
{
    using System;
    using Command;
    using Command.SceCommands;

    public sealed class CaveExperienceMiniGame : IDisposable,
        ICommandExecutor<MiniGameStartCaveExperienceCommand>
    {
        public CaveExperienceMiniGame()
        {
            CommandExecutorRegistry<ICommand>.Instance.Register(this);
        }

        public void Dispose()
        {
            CommandExecutorRegistry<ICommand>.Instance.UnRegister(this);
        }

        public void Execute(MiniGameStartCaveExperienceCommand command)
        {
            CommandDispatcher<ICommand>.Instance.Dispatch(
                new UIDisplayNoteCommand("山洞初体验小游戏暂未实现，现已跳过"));
        }
    }
}

#endif