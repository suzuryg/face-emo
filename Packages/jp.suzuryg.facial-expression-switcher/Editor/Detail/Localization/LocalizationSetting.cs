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
            var locale = GetLocale();
            SetLocale(locale);
        }

        // TODO: Error handling
        public void SetLocale(Locale locale)
        {
            Table = GetTable(locale);
            if (Table == null) { Table = ScriptableObject.CreateInstance<LocalizationTable>(); }

            Locale = locale;
            EditorPrefs.SetString(LocalePrefKey, Locale.ToString());
            _onTableChanged.OnNext(Table);
        }

        // TODO: Table property is unnecessary?
        public LocalizationTable GetCurrentLocaleTable()
        {
            var locale = GetLocale();
            return GetTable(locale);
        }

        [MenuItem("Tools/suzuryg/FacialExpressionSwitcher/Debug/ResetLocale")]
        public static void ResetLocale()
        {
            EditorPrefs.DeleteKey(LocalePrefKey);
        }

        private Locale GetLocale()
        {
            var localeString = EditorPrefs.GetString(LocalePrefKey);
            if (localeString is string && Enum.TryParse<Locale>(localeString, out var locale) && Enum.IsDefined(typeof(Locale), locale))
            {
                return locale;
            }
            else
            {
                return GetDefaultLocale();
            }
        }

        private LocalizationTable GetTable(Locale locale)
        {
            if (locale == Locale.ja_JP)
            {
                return AssetDatabase.LoadAssetAtPath<LocalizationTable>($"{DetailConstants.LocalizationDirectory}/ja_JP.asset");
            }
            else
            {
                // en_US has the table in source code, not asset.
                return ScriptableObject.CreateInstance<LocalizationTable>();
            }
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

        public static string InsertLineBreak(string text)
        {
            return text.Replace("<br>", "\n");
        }
    }
}
