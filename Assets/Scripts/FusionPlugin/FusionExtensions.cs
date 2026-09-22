namespace Fusion.Plugin
{
	public static class FusionExtensions
	{
		public static float GetRemoteAlpha(this NetworkRunner runner)
		{
			return runner.Simulation.RemoteAlpha;
		}

		public static Tick GetRemoteTick(this NetworkRunner runner)
		{
			return runner.Simulation.RemoteTick;
		}

		public static Tick GetRemoteTickPrevious(this NetworkRunner runner)
		{
			return runner.Simulation.RemoteTickPrevious;
		}
	}
}
