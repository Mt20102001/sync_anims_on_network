namespace Animations
{
    using UnityEngine;
    using Fusion.Plugin;
    using Fusion;

    public class HitboxDraw : NetworkBehaviour
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private bool _drawHitbox = true;
        [SerializeField]
        private Hitbox _hitbox;
        [SerializeField]
        private int _hitboxDrawIntervalTicks = 50;

        [Header("Colors")]
        [SerializeField]
        private Color _inputAuthorityColor = Color.white;
        [SerializeField]
        private Color _proxyColor = Color.gray;
        [SerializeField]
        private Color _stateAuthorityColor = Color.blue;

        // Tick vừa được vẽ ở phía authoritative, đang chờ visual "bắt kịp"
        private int _pendingTick = -1;
        // Tick cuối cùng đã vẽ visual, để không vẽ lặp lại nhiều lần/frame
        private int _lastVisualDrawnTick = -1;

        // SimulationBehaviour INTERFACE

        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);
        }

        public override void FixedUpdateNetwork()
        {
            DrawAuthoritativeHitbox();
        }

        public override void Render()
        {
            DrawVisualHitbox();
        }

        //private void LateUpdate()
        //{
        //    DrawVisualHitbox();
        //}

        // PRIVATE METHODS

        // Vẽ vị trí AUTHORITATIVE tại tick T — dùng cho cả host lẫn client,
        // luôn nhất quán vì lấy từ buffer lịch sử của HitboxManager, không phải Transform.
        private void DrawAuthoritativeHitbox()
        {
            if (_drawHitbox == false || _hitbox == null) return;
            if (HasInputAuthority == true && HasStateAuthority == false) return;
            if (Runner.IsForward == false) return;

            int tick = Runner.Tick;
            if (tick % _hitboxDrawIntervalTicks != 0) return;

            float duration = _hitboxDrawIntervalTicks * Runner.DeltaTime;

            Runner.LagCompensation.PositionRotation(_hitbox, tick, out Vector3 dataPos, out Quaternion dataRot);
            GameDraw.WireBox(dataPos + dataRot * _hitbox.Offset, dataRot, _hitbox.BoxExtents * 2f, _stateAuthorityColor, duration);
            //GameDraw.WireBox(_hitbox.Position, _hitbox.transform.rotation, _hitbox.BoxExtents * 2f, _stateAuthorityColor, duration);

            // Đăng ký tick này để Render() vẽ visual khớp đúng tick, không tự tính riêng
            _pendingTick = tick;
            Debug.Log($"Authoritative - Tick[{tick}]: {dataPos + dataRot * _hitbox.Offset} - {dataRot}");

        }

        // Vẽ vị trí VISUAL — chỉ vẽ khi Render() đã "bắt kịp" đúng cái tick
        // mà DrawAuthoritativeHitbox vừa vẽ, đảm bảo 2 box cùng 1 con số tick.
        private void DrawVisualHitbox()
        {
            if (_drawHitbox == false || _hitbox == null) return;
            if (HasInputAuthority == true && HasStateAuthority == false) return;
            if (_pendingTick < 0) return;

            // Tick hiện tại phía render: state authority dùng Runner.Tick (không có khái niệm remote),
            // proxy dùng Runner.GetRemoteTick() (tick xa nhất đã nhận được từ server).
            int renderTick = Object.HasStateAuthority ? Runner.Tick : Runner.GetRemoteTick();

            if (renderTick != _pendingTick) return;
            if (renderTick == _lastVisualDrawnTick) return; // tránh vẽ lặp nhiều lần cùng 1 tick

            _lastVisualDrawnTick = renderTick;

            float duration = _hitboxDrawIntervalTicks * Runner.DeltaTime;
            GameDraw.WireBox(_hitbox.Position, _hitbox.transform.rotation, _hitbox.BoxExtents * 2f, _proxyColor, duration);
            Debug.Log($"Visual - Tick[{renderTick}]: {_hitbox.Position} - {_hitbox.transform.rotation}");
        }

        private Color GetColor()
        {
            if (Object.IsProxy == true)
                return _proxyColor;

            return Object.HasStateAuthority == true ? _stateAuthorityColor : _inputAuthorityColor;
        }
    }
}