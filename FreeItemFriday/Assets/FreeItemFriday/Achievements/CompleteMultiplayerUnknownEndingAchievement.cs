using RoR2;
using RoR2.Achievements;

namespace FreeItemFriday.Achievements
{
	// Match achievement identifiers from FreeItemFriday
	[TryRegisterAchievement("CompleteMultiplayerUnknownEnding", "Items.FreeItemFriday.DeathEye", null, typeof(ServerAchievement))]
	public class CompleteMultiplayerUnknownEndingAchievement : BaseAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			SetServerTracked(true);
		}

		public override void OnUninstall()
		{
			base.OnUninstall();
		}

		public class ServerAchievement : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
				Run.onServerGameOver += OnServerGameOver;
			}

			public override void OnUninstall()
			{
				base.OnInstall();
				Run.onServerGameOver -= OnServerGameOver;
			}

			public void OnServerGameOver(Run run, GameEndingDef gameEndingDef)
			{
				if ((gameEndingDef == RoR2Content.GameEndings.ObliterationEnding || gameEndingDef == RoR2Content.GameEndings.LimboEnding) && RoR2Application.isInMultiPlayer)
				{
					Grant();
				}
			}
		}
	}
}