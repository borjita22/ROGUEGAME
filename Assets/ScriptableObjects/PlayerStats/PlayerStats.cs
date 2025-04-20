using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats", menuName = "Game/Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    //representa los valores de las stats del personaje
    //no solamente representa los valores de salud, daño, etc, sino que tambien lleva la contabilidad
    //del nivel actual de cada stat
    [Header("Stats base values")]
    [SerializeField] private int HealthLevel;
    [SerializeField] private int ArmorLevel;
    [SerializeField] private int DamageLevel;
    [SerializeField] private int StrengthLevel;
    [SerializeField] private int ManaLevel;
    [SerializeField] private int LuckLevel;
    [SerializeField] private int SpeedLevel;
    [SerializeField] private int WeaponLevel;

    public int Strength
    {
        get => StrengthLevel;

        set
        {
            StrengthLevel = value;
            OnStatChange?.Invoke("Strength", StrengthLevel);
        }
    }

    public int Speed
    {
        get => SpeedLevel;

        set
        {
            SpeedLevel = value;
            OnStatChange?.Invoke("Speed", StrengthLevel);
        }
    }

    public delegate void OnStatChangeHandler(string statName, int newValue);
    public event OnStatChangeHandler OnStatChange;

    public void ResetToDefault()
    {
        HealthLevel = 1;
        ArmorLevel = 1;
        DamageLevel = 1;
        StrengthLevel = 1;
        ManaLevel = 1;
        LuckLevel = 1;
        SpeedLevel = 1;
        WeaponLevel = 1;
    }

}

[CustomEditor(typeof(PlayerStats))]
public class PlayerStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Dibuja el inspector por defecto

        PlayerStats playerStats = (PlayerStats)target;

        // Añadir espacio vertical antes del botón
        EditorGUILayout.Space();

        // Dibujar un botón para resetear las estadísticas
        if (GUILayout.Button("Reset Stats to Default", GUILayout.Height(30)))
        {
            // Llamar al método de reseteo en el ScriptableObject
            playerStats.ResetToDefault();

            // Marcar el objeto como "dirty" para que Unity sepa que debe guardarlo
            EditorUtility.SetDirty(playerStats);

            // Notificar al editor que ha habido cambios
            serializedObject.Update();
        }
    }
}
