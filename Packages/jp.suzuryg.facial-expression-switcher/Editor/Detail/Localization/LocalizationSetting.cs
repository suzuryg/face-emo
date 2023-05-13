using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Localization
{
    public interface IReadOnlyLocalizationSetting
    {
        Locale Locale { get; }
        LocalizationTable Table { get; }
        IObservable<LocalizationTable> OnTableChanged { get; }

        LocalizationTable GetCurrentLocaleTable();
    }

    public interface ILocalizationSetting
    {
        Locale Locale { get; }
        LocalizationTable Table { get; }
        IObservable<LocalizationTable> OnTableChanged { get; }

        void SetLocale(Locale locale);
        LocalizationTable GetCurrentLocaleTable();
    }

    public class LocalizationSetting : IReadOnlyLocalizationSetting, ILocalizationSetting
    {
        private static readonly string LocalePrefKey = $"{DetailConstants.PackageName}.locale";

        public Locale Locale { get; private set; }
        public LocalizationTable Table { get; private set; }
        public IObservable<LocalizationTable> OnTableChanged => _onTableChanged.AsObservable();

        private Subject<LocalizationTable> _onTableChanged = new Subject<LocalizationTable>();

        public LocalizationSetting()
        {
            var localeString = EditorPrefs.GetString(LocalePrefKey);
            if (localeString is string && Enum.TryParse<Locale>(localeString, out var locale) && Enum.IsDefined(typeof(Locale), locale))
            {
                SetLocale(locale);
            }
            else
            {
                SetLocale(GetDefaultLocale());
            }
        }

        // TODO: Error handling
        public void SetLocale(Locale locale)
        {
            if (locale == Locale.ja_JP)
            {
                Table = AssetDatabase.LoadAssetAtPath<LocalizationTable>($"{DetailConstants.LocalizationDirectory}/ja_JP.asset");
            }
            else
            {
                Table = AssetDatabase.LoadAssetAtPath<LocalizationTable>($"{DetailConstants.LocalizationDirectory}/en_US.asset");
            }

            if (Table == null) { Table = ScriptableObject.CreateInstance<LocalizationTable>(); }

            Locale = locale;
            EditorPrefs.SetString(LocalePrefKey, Locale.ToString());
            _onTableChanged.OnNext(Table);
        }

        // TODO: Table property is unnecessary?
        public LocalizationTable GetCurrentLocaleTable()
        {
            var localeString = EditorPrefs.GetString(LocalePrefKey);
            if (localeString is string && Enum.TryParse<Locale>(localeString, out var locale) && Enum.IsDefined(typeof(Locale), locale))
            {
                if (locale == Locale.ja_JP)
                {
                    return AssetDatabase.LoadAssetAtPath<LocalizationTable>($"{DetailConstants.LocalizationDirectory}/ja_JP.asset");
                }
                else
                {
                    return AssetDatabase.LoadAssetAtPath<LocalizationTable>($"{DetailConstants.LocalizationDirectory}/en_US.asset");
                }
            }
            else
            {
                return new LocalizationTable();
            }
        }

        [MenuItem("Tools/Suzuryg/FacialExpressionSwitcher/Debug/ResetLocale")]
        public static void ResetLocale()
        {
            EditorPrefs.DeleteKey(LocalePrefKey);
        }

        private Locale GetDefaultLocale()
        {
            var currentCulture = CultureInfo.CurrentCulture;
            if (currentCulture.Name == "ja-JP")
            {
                return Locale.ja_JP;
            }
            else
            {
                return Locale.en_US;
            }
        }
    }
}
