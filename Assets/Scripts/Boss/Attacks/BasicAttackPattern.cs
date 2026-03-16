using UnityEngine;

namespace Core.Boss.Attacks
{
    /// <summary>
    /// 기본 근접 공격 패턴.
    /// 기존 BossAttackState의 로직을 이관한 것.
    /// </summary>
    public class BasicAttackPattern : IBossAttackPattern
    {
        private const float FixedExitNormalizedTime = 1.0f;
        private const float MinAnimatorPlaybackSpeed = 0.01f;
        private const float MaxAnimatorPlaybackSpeed = 20f;
        private const float MinReadySliceLength = 0.0001f;
        private const float MinFallbackTotalDuration = 0.05f;

        private bool _damageWindowOpen;
        private bool _basicAttackStateObserved;
        private float _fallbackElapsedTime;
        private float _fallbackDamageOpenTime;
        private float _fallbackExitTime;

        public void Enter(BossController controller)
        {
            controller.StopMoving();

            // 타겟 방향으로 회전
            if (controller.Target != null)
            {
                controller.RotateTowards(controller.Target.position);
            }

            // 공격 애니메이션 재생
            controller.Visual?.ResetAnimatorPlaybackSpeed();
            controller.Visual?.PlayAttack();
            controller.HeadDamageCaster?.DisableHitbox();

            _damageWindowOpen = false;
            _basicAttackStateObserved = false;
            _fallbackElapsedTime = 0f;

            BossController.BasicAttackSettings settings = controller.BasicAttackConfig;
            _fallbackDamageOpenTime = settings != null ? Mathf.Max(0f, settings.readyDuration) : 0f;
            _fallbackExitTime = Mathf.Max(
                _fallbackDamageOpenTime,
                Mathf.Max(MinFallbackTotalDuration, controller.AttackDuration + _fallbackDamageOpenTime));
        }

        public bool Update(BossController controller)
        {
            if (controller.Visual?.Animator == null)
            {
                return UpdateFallback(controller);
            }

            AnimatorStateInfo stateInfo = controller.Visual.Animator.GetCurrentAnimatorStateInfo(0);
            bool isBasicAttackState = stateInfo.IsName("Basic Attack");
            if (!isBasicAttackState)
            {
                if (_basicAttackStateObserved)
                {
                    RestorePlaybackSpeed(controller);
                    CloseDamageWindow(controller);
                    return true;
                }

                return false;
            }

            _basicAttackStateObserved = true;

            BossController.BasicAttackSettings settings = controller.BasicAttackConfig;
            float progress = stateInfo.normalizedTime;
            float readyEnd = settings != null ? settings.readyNormalizedWindow.y : 0f;

            ApplyReadyPlaybackSpeed(controller, settings, progress);

            if (!_damageWindowOpen && progress >= readyEnd)
            {
                OpenDamageWindow(controller);
            }

            if (progress >= FixedExitNormalizedTime)
            {
                RestorePlaybackSpeed(controller);
                CloseDamageWindow(controller);
                return true;
            }

            return false;
        }

        public void Exit(BossController controller)
        {
            RestorePlaybackSpeed(controller);
            // 히트박스 안전 정리
            controller.HeadDamageCaster?.DisableHitbox();
        }

        private bool UpdateFallback(BossController controller)
        {
            _fallbackElapsedTime += Time.deltaTime;

            if (!_damageWindowOpen && _fallbackElapsedTime >= _fallbackDamageOpenTime)
            {
                OpenDamageWindow(controller);
            }

            if (_fallbackElapsedTime >= _fallbackExitTime)
            {
                CloseDamageWindow(controller);
                return true;
            }

            return false;
        }

        private void ApplyReadyPlaybackSpeed(
            BossController controller,
            BossController.BasicAttackSettings settings,
            float progress)
        {
            if (controller.Visual == null) return;
            if (settings == null)
            {
                RestorePlaybackSpeed(controller);
                return;
            }

            float readyStart = settings.readyNormalizedWindow.x;
            float readyEnd = settings.readyNormalizedWindow.y;
            float readyDuration = settings.readyDuration;
            float readySliceLength = readyEnd - readyStart;

            if (readyDuration <= 0f || readySliceLength <= MinReadySliceLength)
            {
                RestorePlaybackSpeed(controller);
                return;
            }

            if (progress < readyStart || progress >= readyEnd)
            {
                RestorePlaybackSpeed(controller);
                return;
            }

            float clipLength = controller.Visual.GetBasicAttackClipLengthOrDefault(controller.AttackDuration);
            float baseReadyDuration = readySliceLength * Mathf.Max(MinFallbackTotalDuration, clipLength);
            float targetPlaybackSpeed = Mathf.Clamp(
                baseReadyDuration / readyDuration,
                MinAnimatorPlaybackSpeed,
                MaxAnimatorPlaybackSpeed);

            controller.Visual.SetAnimatorPlaybackSpeed(targetPlaybackSpeed);
        }

        private void OpenDamageWindow(BossController controller)
        {
            if (_damageWindowOpen) return;

            controller.HeadDamageCaster?.EnableHitbox(controller.AttackDamage);
            _damageWindowOpen = true;
        }

        private void CloseDamageWindow(BossController controller)
        {
            if (!_damageWindowOpen) return;

            controller.HeadDamageCaster?.DisableHitbox();
            _damageWindowOpen = false;
        }

        private static void RestorePlaybackSpeed(BossController controller)
        {
            controller.Visual?.ResetAnimatorPlaybackSpeed();
        }
    }
}
