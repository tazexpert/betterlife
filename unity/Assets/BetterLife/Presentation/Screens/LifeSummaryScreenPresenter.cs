using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using UnityEngine;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// End of a life: eulogy, key numbers, achievements of this life, and continuing as a child or starting anew.
    /// </summary>
    public sealed class LifeSummaryScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public LifeSummaryScreenPresenter(ScreenContext context)
            : base(context, null)
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            LifeSummaryState summary = LifeSummaryViewModel.Build(session, Context.Achievements, Context.Legacy, Context.Localizer, Context.Money);
            if (summary == null)
            {
                SetTitle(string.Empty);
                return;
            }
            SetTitle(summary.Title);
            Content.Add(UiRows.Text(summary.Subtitle, "list-screen__subtitle"));
            foreach (string line in summary.Eulogy)
            {
                Content.Add(UiRows.Text(line, "summary__line"));
            }
            AddSection("summary.section.facts", summary.Facts);
            if (summary.Achievements.Count > 0)
            {
                Content.Add(UiRows.Section(Context.Localizer.Get("summary.section.achievements")));
                foreach (string name in summary.Achievements)
                {
                    Content.Add(UiRows.Text(name, "summary__achievement"));
                }
            }

            Content.Add(UiRows.Section(Context.Localizer.Get("summary.section.next")));
            foreach (HeirOption heir in summary.Heirs)
            {
                int npcId = heir.NpcId;
                Content.Add(UiRows.Action(heir.Label, () => Context.Lives.ContinueAsHeir(npcId), true, heir.Detail));
            }
            if (summary.Heirs.Count == 0)
            {
                Content.Add(UiRows.Text(Context.Localizer.Get("summary.no_heir"), "list-screen__empty"));
            }
            Content.Add(UiRows.Action(Context.Localizer.Get("summary.new_life"), Context.Lives.StartNewLife));
            string share = summary.ShareText;
            Content.Add(UiRows.Action(Context.Localizer.Get("summary.copy"), () => Copy(share)));
        }

        private void Copy(string text)
        {
            // Copies to the system clipboard; sharing to other apps needs a native plugin (task 23).
            GUIUtility.systemCopyBuffer = text;
            ShowResult(new ActionResultState(Context.Localizer.Get("summary.copied"), true, new string[0]));
        }
    }
}
