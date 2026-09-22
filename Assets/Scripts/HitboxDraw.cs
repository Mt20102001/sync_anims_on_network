using UnityEngine;
using Fusion;

namespace Animations
{
	using System.Collections.Generic;
	using Fusion.Plugin;

	public class HitboxDraw : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private bool _drawHitbox = true;
		[SerializeField]
		private Hitbox _hitbox;
		[SerializeField]
		private int _hitboxDrawIntervalTicks = 50;
		[SerializeField]
		private bool _subtickAccuracy;

		[Header("Colors")]
		[SerializeField]
		private Color _inputAuthorityColor = Color.white;
		[SerializeField]
		private Color _proxyColor = Color.gray;
		[SerializeField]
		private Color _stateAuthorityColor = Color.blue;

		// SimulationBehaviour INTERFACE

		public override void Spawned()
		{
			Runner.SetIsSimulated(Object, true);
		}

		public override void FixedUpdateNetwork()
		{
			DrawHitbox();
		}

		// PRIVATE METHODS

		private void DrawHitbox()
		{
            Debug.Log("Step 1");
            if (_drawHitbox == false)
				return;

            Debug.Log("Step 2");
            if (_hitbox == null)
				return;

            Debug.Log("Step 3");
            if (HasInputAuthority == true && HasStateAuthority == false)
				return; // Do not draw for input authority, no point

            Debug.Log("Step 4");
            if (Runner.IsForward == false)
				return;

            Debug.Log("Step 5");
            int tick = Runner.Tick;

			if (Object.HasStateAuthority == false)
			{
				float interpAlpha = Runner.GetRemoteAlpha();
				int   fromTick    = Runner.GetRemoteTickPrevious();
				int   toTick      = Runner.GetRemoteTick();

				if (_subtickAccuracy == true)
				{
                    Debug.Log("Step 6.1");
                    tick = Mathf.RoundToInt(Mathf.Lerp(fromTick, toTick, interpAlpha));
				}
				else
				{
                    Debug.Log("Step 6.2");
                    tick = interpAlpha < 0.5f ? fromTick : toTick;
				}
			}

			if (tick % _hitboxDrawIntervalTicks == 0)
			{
				Debug.Log("Step 7");
				float duration = _hitboxDrawIntervalTicks * Runner.DeltaTime;
				GameDraw.WireBox(_hitbox.Position, _hitbox.transform.rotation, _hitbox.BoxExtents * 2f, GetColor(), duration);
			}
		}

		private Color GetColor()
		{
			if (Object.IsProxy == true)
				return _proxyColor;

			return Object.HasStateAuthority == true ? _stateAuthorityColor : _inputAuthorityColor;
		}
	}
}
