/*
using UnityEngine;
using UnityEditor;

namespace Shooter.CustomEditors {
    [CustomEditor(typeof(Misc.Weapon))]
    public class EditorWeapon : Editor {
        private string[] tooltips = new string[8] { //This is an array of all of the tooptips used in the inspector
            "Sound that will be played when the weapon is fired.",
            "Effect that will show when the bullet hits a surface.",
            "Amount of damage that weapon outputs.",
            "Specifies the fire type of the weapon.",
            "Speed the weapon shoots.",
            "Amount that bullets will spread.",
            "Amount of bullets that are shot every time the player shoots.",
            "Distance that bullet travels. Default value is probably best for this."
        };

        public override void OnInspectorGUI() {
            Misc.Weapon weapon = (Misc.Weapon)target; //Gets the target, and casts it to a correct object type

            Do all of the GUI stuff
            weapon.shootSound = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Shoot Sound", tooltips[0]), weapon.shootSound, typeof(AudioClip), true);

            weapon.impactEffect = (Transform)EditorGUILayout.ObjectField(new GUIContent("Bullet Impact Effect", tooltips[1]), weapon.impactEffect, typeof(Transform), true);

            weapon.damage = EditorGUILayout.FloatField(new GUIContent("Damage", tooltips[2]), weapon.damage);
            
            weapon.fireType = (Misc.Weapon.FireType)EditorGUILayout.EnumPopup(new GUIContent("Fire Type", tooltips[3]), weapon.fireType);

            weapon.fireRate = EditorGUILayout.FloatField(new GUIContent("Fire Rate", tooltips[4]), weapon.fireRate);

            weapon.spread = EditorGUILayout.FloatField(new GUIContent("Spread", tooltips[5]), weapon.spread);

            weapon.bulletsPerShot = EditorGUILayout.IntField(new GUIContent("Bullets per shot", tooltips[6]), weapon.bulletsPerShot);

            weapon.shootDistance = EditorGUILayout.FloatField(new GUIContent("Bullet Distance", tooltips[7]), weapon.shootDistance);

            EditorUtility.SetDirty(target); //Makes sure that Editor saves everything
        }
    }
}
*/