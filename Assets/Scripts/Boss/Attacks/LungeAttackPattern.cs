using UnityEngine;

namespace Core.Boss.Attacks
{
    /// <summary>
    /// 도약 돌진 공격 패턴 (Lunge).
    /// normalizedTime 기반으로 애니메이션 종료를 판단하여,
    /// 클립 길이가 변경되어도 자동으로 적응합니다.
    /// </summary>
    public class LungeAttackPattern : IBossAttackPattern
    {
        private readonly BossController.LungeAttackSettings _settings;

        /// <summary>도약 구간이 완료되었는지 추적하는 플래그</summary>
        private bool _rushComplete;

        public LungeAttackPattern(BossController.LungeAttackSettings settings)
        {
            _settings = settings;
        }

        public void Enter(BossController controller)
        {
            _rushComplete = false;
            controller.StopMoving();

            // 타겟 방향으로 즉시 회전
            if (controller.Target != null)
            {
                controller.RotateTowards(controller.Target.position);
            }

            // Lunge Attack 애니메이션 재생
            controller.Visual?.PlayLungeAttack();

            // DamageCaster 활성화 (기본 공격력 × damageMultiplier)
            int damage = (int)(controller.AttackDamage * _settings.damageMultiplier);
            controller.LungeDamageCaster?.EnableHitbox(damage);
        }

        /// <summary>
        /// 매 프레임 호출. normalizedTime으로 애니메이션 진행률을 추적하여
        /// 도약 구간과 종료 시점을 판단합니다.
        /// </summary>
        /// <returns>true: 공격 종료 → CombatState로 복귀</returns>
        public bool Update(BossController controller)
        {
            // Visual 또는 Animator가 없으면 즉시 종료 (안전 장치)
            if (controller.Visual?.Animator == null) return true;

            // 현재 Animator Layer 0의 상태 정보 조회
            AnimatorStateInfo stateInfo = controller.Visual.Animator.GetCurrentAnimatorStateInfo(0);

            // CrossFade 중이거나 타겟 상태 진입 전이면 대기
            // 레거시 Animator를 위해 "Claw Attack" 상태명도 허용한다.
            bool isLungeState = stateInfo.IsName("Lunge Attack") || stateInfo.IsName("Claw Attack");
            if (!isLungeState)
            {
                return false;
            }

            // normalizedTime: 0.0(시작) ~ 1.0(끝). 루프 클립은 1.0 초과 가능.
            float progress = stateInfo.normalizedTime;

            // [도약 구간] 애니메이션 초반(0 ~ rushPhaseRatio) 동안 전방 돌진
            if (!_rushComplete && progress < _settings.rushPhaseRatio)
            {
                controller.MoveRaw(controller.transform.forward, _settings.rushSpeed);
            }
            else if (!_rushComplete)
            {
                // 도약 구간 종료 (MoveRaw를 더 이상 호출하지 않으므로 자연히 정지)
                _rushComplete = true;
            }

            // 애니메이션 종료 판정 (exitPhaseRatio 시점에서 종료)
            return progress >= _settings.exitPhaseRatio;
        }

        public void Exit(BossController controller)
        {
            // 판정 종료 및 이동 정지 (사망 등 강제 전환 시에도 안전하게 정리)
            controller.LungeDamageCaster?.DisableHitbox();
            controller.StopMoving();
        }
    }
}
