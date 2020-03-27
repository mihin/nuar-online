using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardsScriptableObject))]
public class CardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CardsScriptableObject cardData = (CardsScriptableObject)target;

        var property = serializedObject.FindProperty("Cards");
        serializedObject.Update();
        EditorGUILayout.PropertyField(property, true);
        serializedObject.ApplyModifiedProperties();

        #region Buttons

        if (GUILayout.Button("Загрузить из JSON"))
        {
            List<Card> cd = EditorGUIUtility.systemCopyBuffer.FromJson<Card>();
            if (cd != null) cardData.Cards = cd;
        }

        if (GUILayout.Button("Скопировать JSON"))
        {
            EditorGUIUtility.systemCopyBuffer = cardData.Cards.ToJson();
        }

        #endregion
    }
}
