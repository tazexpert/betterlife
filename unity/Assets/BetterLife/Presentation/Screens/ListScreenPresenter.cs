using System;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using BetterLife.Presentation.Shell;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Base of the list screens: a scrolling page with a title, an optional result card and content rebuilt on
    /// every refresh. Subclasses only fill the content.
    /// </summary>
    public abstract class ListScreenPresenter : IScreenPresenter
    {
        /// <summary>Close button key of result cards.</summary>
        public const string CloseKey = "action.close";

        private readonly Label _title;
        private readonly VisualElement _result;
        private ActionResultState _lastResult;

        /// <summary>
        /// Creates the page.
        /// </summary>
        /// <param name="context">Screen context.</param>
        /// <param name="titleKey">Localization key of the title (null for a title set by the subclass).</param>
        protected ListScreenPresenter(ScreenContext context, string titleKey)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Root = UiAssets.Layout(UiAssets.ListScreen).Instantiate();
            Root.style.flexGrow = 1;
            _title = Root.Q<Label>("title");
            _result = Root.Q<VisualElement>("result");
            Content = Root.Q<VisualElement>("content");
            _title.text = titleKey == null ? string.Empty : context.Localizer.Get(titleKey);
        }

        /// <inheritdoc />
        public VisualElement Root { get; }

        /// <summary>Screen context.</summary>
        protected ScreenContext Context { get; }

        /// <summary>Container the subclass fills.</summary>
        protected VisualElement Content { get; }

        /// <summary>Request that opened the screen.</summary>
        protected ScreenRequest Request { get; private set; }

        /// <inheritdoc />
        public void Show(ScreenRequest request)
        {
            if (Request != null && !Request.Equals(request))
            {
                _lastResult = null;
            }
            Request = request;
        }

        /// <inheritdoc />
        public void Refresh()
        {
            Content.Clear();
            _result.Clear();
            if (_lastResult != null)
            {
                _result.Add(UiRows.Result(_lastResult, Context.Localizer.Get(CloseKey), () =>
                {
                    _lastResult = null;
                    Refresh();
                }));
            }
            LifeSession session = Context.Session;
            if (session != null)
            {
                Fill(session);
            }
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Builds the content for the current state.
        /// </summary>
        /// <param name="session">Running life.</param>
        protected abstract void Fill(LifeSession session);

        /// <summary>
        /// Changes the title.
        /// </summary>
        /// <param name="text">Title text.</param>
        protected void SetTitle(string text) => _title.text = text ?? string.Empty;

        /// <summary>
        /// Shows the result of an action at the top and tells the shell that the state changed.
        /// </summary>
        /// <param name="result">Result.</param>
        protected void ShowResult(ActionResultState result)
        {
            _lastResult = result;
            Context.NotifyChanged();
            Refresh();
        }

        /// <summary>
        /// Adds a section title and its rows (or the empty text when there are none).
        /// </summary>
        /// <param name="titleKey">Section title key.</param>
        /// <param name="rows">Rows.</param>
        /// <param name="emptyKey">Key of the text shown when empty; null hides the empty section.</param>
        protected void AddSection(string titleKey, System.Collections.Generic.IReadOnlyList<ValueRow> rows, string emptyKey = null)
        {
            if (rows.Count == 0 && emptyKey == null)
            {
                return;
            }
            Content.Add(UiRows.Section(Context.Localizer.Get(titleKey)));
            if (rows.Count == 0)
            {
                Content.Add(UiRows.Text(Context.Localizer.Get(emptyKey), "list-screen__empty"));
                return;
            }
            foreach (ValueRow row in rows)
            {
                Content.Add(UiRows.Row(row));
            }
        }
    }
}
