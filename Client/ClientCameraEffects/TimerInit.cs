using System.Collections.Generic;
using RAGETimer.Shared;
using static RAGE.Events;

class TimerManager : Script
{
	public TimerManager()
	{
		Timer.Init(RAGE.Chat.Output, GetTick);
		Tick += OnTick;
	}

	public static void OnTick(List<TickNametagData> _)
	{
		Timer.OnUpdateFunc();
	}

	private static ulong GetTick() => (ulong)RAGE.Game.Misc.GetGameTimer();
}
