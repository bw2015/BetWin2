using BW.Common.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Games
{
    public class GameFactory
    {
        public static IGame CreateGame(GameType type, string setting)
        {
            IGame game = null;
            switch (type)
            {
                case GameType.PT:
                    game = new PT(setting);
                    break;
                case GameType.AG:
                    game = new AG(setting);
                    break;
                case GameType.BBIN:
                    game = new BBIN(setting);
                    break;
                case GameType.Sing3:
                    game = new SB(setting);
                    break;
                case GameType.Casino:
                    game = new Casino(setting);
                    break;
                case GameType.SunBet:
                    game = new SunBet(setting);
                    break;
                case GameType.MW:
                    game = new MW(setting);
                    break;
                case GameType.MG:
                    game = new MG(setting);
                    break;
                case GameType.BWGaming:
                    game = new BWGaming(setting);
                    break;
                case GameType.OG:
                    game = new OG(setting);
                    break;
            }
            return game;
        }
    }
}
