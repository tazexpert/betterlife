using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Settings: language, vice content, undo left today, resetting achievements (with a second press to confirm).
    /// </summary>
    public sealed class SettingsScreenPresenter : ListScreenPresenter
    {
        private readonly ISettingsController _settings;
        private bool _confirmReset;

        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        /// <param name="settings">Settings controller.</param>
        public SettingsScreenPresenter(ScreenContext context, ISettingsController settings)
            : base(context, "settings.title")
        {
            _settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            AchievementsViewModel.Build(Context.Achievements, Context.Localizer, out string progress);
            SettingsState state = SettingsViewModel.Build(_settings.Settings, _settings.Language, _settings.Tables, _settings.UndoLeftToday, progress,
                _settings.ContentVersion, Context.Localizer);

            Content.Add(UiRows.Section(Context.Localizer.Get("settings.section.language")));
            foreach (AmountChoice language in state.Languages)
            {
                string code = language.Id;
                Content.Add(UiRows.Action(language.Label, () => _settings.SetLanguage(code), language.IsAvailable, language.Detail));
            }

            Content.Add(UiRows.Section(Context.Localizer.Get("settings.section.content")));
            Content.Add(UiRows.Text(Context.Localizer.Get(state.ViceContent ? "settings.vice.on" : "settings.vice.off"), "list-screen__subtitle"));
            bool turnOn = !state.ViceContent;
            Content.Add(UiRows.Action(state.ViceToggleText, () =>
            {
                _settings.SetViceContent(turnOn);
                Refresh();
            }));

            AddSection("settings.section.info", state.Info);
            Content.Add(UiRows.Action(Context.Localizer.Get(_confirmReset ? "settings.reset_achievements.confirm" : "settings.reset_achievements"), ResetAchievements));
        }

        private void ResetAchievements()
        {
            if (!_confirmReset)
            {
                _confirmReset = true;
                Refresh();
                return;
            }
            _confirmReset = false;
            _settings.ResetAchievements();
            ShowResult(new ActionResultState(Context.Localizer.Get("settings.reset_achievements.done"), true, new string[0]));
        }
    }
}
