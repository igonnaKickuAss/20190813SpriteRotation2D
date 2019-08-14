using UnityEngine;
using System.Collections;

namespace OLiOYouxi
{
    public class AimAndShoot : MonoBehaviour
    {
        #region -- Private Data --
        [OLiOYouxiAttributes.BoxGroup("瞄准参数")] [SerializeField] private Transform Weapon;
        [OLiOYouxiAttributes.BoxGroup("瞄准参数")] [SerializeField] private int MaxAngle;
        [OLiOYouxiAttributes.BoxGroup("瞄准参数")] [SerializeField] private int MinAngle;

        private Animator _copAnimator;
        private Animator _weaponAnimator;
        private PixelRotation _weaponRotation;

        #endregion

        #region -- ShotC --
        public int Angle { get; private set; }

        #endregion

        #region -- MONO APIMethods --
        private void Awake()
        {
            _weaponAnimator = Weapon.GetComponent<Animator>();
            _copAnimator = GetComponent<Animator>();
            _weaponRotation = Weapon.GetComponent<PixelRotation>();

            Angle = 0;
        }

        private void Update()
        {
            RotateWeapon();
            Shoot();
        }

        #endregion

        #region -- Private APIMethods --
        Vector3 pos = Vector3.zero;
        Vector3 dir = Vector3.zero;
        private void RotateWeapon()
        {
            if (!_weaponAnimator.GetCurrentAnimatorStateInfo(0).IsTag("shooting"))
            {
                pos = Camera.main.WorldToScreenPoint(Weapon.position);
                dir = Input.mousePosition - pos;
                Angle = Mathf.RoundToInt(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

                if (Angle > MaxAngle)
                {
                    Angle = MaxAngle;
                }
                else if (Angle < MinAngle)
                {
                    Angle = MinAngle;
                }

                _weaponRotation.Angle = Angle;
            }
        }

        private void Shoot()
        {
            if (Input.GetMouseButtonUp(0) &&
                !_weaponAnimator.GetCurrentAnimatorStateInfo(0).IsTag("shooting") &&
                !_copAnimator.GetCurrentAnimatorStateInfo(0).IsTag("shooting"))
            {
                _copAnimator.SetTrigger("shoot");
                _weaponAnimator.SetTrigger("shoot");
            }
        }

        #endregion


    }
}

