using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Work tab: school and job now, where to apply, and leaving school or a job.
    /// </summary>
    public sealed class WorkScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public WorkScreenPresenter(ScreenContext context)
            : base(context, "work.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            WorkState work = WorkViewModel.Build(session, Context.Localizer, Context.Money);
            bool running = !session.IsOver;

            AddSection("work.section.education", work.Education);
            if (work.CanDropOut && running)
            {
                Content.Add(UiRows.Action(Context.Localizer.Get("work.drop_out"), DropOut));
            }
            if (work.EducationOptions.Count > 0 && running)
            {
                Content.Add(UiRows.Section(Context.Localizer.Get("work.section.apply_school")));
                foreach (EducationOption option in work.EducationOptions)
                {
                    EducationOption chosen = option;
                    Content.Add(UiRows.Action(option.Title, () => ApplySchool(chosen), true, option.Detail));
                }
            }

            AddSection("work.section.job", work.Job);
            if (work.CanQuitJob && running)
            {
                Content.Add(UiRows.Action(Context.Localizer.Get("work.quit_job"), QuitJob));
            }
            if (work.JobOptions.Count > 0 && running)
            {
                Content.Add(UiRows.Section(Context.Localizer.Get("work.section.apply_job")));
                foreach (JobOption option in work.JobOptions)
                {
                    string trackId = option.TrackId;
                    Content.Add(UiRows.Action(option.Title, () => ApplyJob(trackId), true, option.Detail));
                }
            }
        }

        private void ApplySchool(EducationOption option)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            var result = Context.Decisions.ApplyEducation(session, option.ProgramId, option.InstitutionId, option.Funding);
            ShowResult(WorkViewModel.BuildAdmissionResult(session, result, Context.Localizer));
        }

        private void ApplyJob(string trackId)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            var result = Context.Decisions.ApplyJob(session, trackId);
            ShowResult(WorkViewModel.BuildHiringResult(session, result, Context.Localizer));
        }

        private void DropOut()
        {
            LifeSession session = Context.Session;
            if (session != null && Context.Decisions.DropOut(session))
            {
                ShowResult(new ActionResultState(Context.Localizer.Get(WorkViewModel.DroppedOutKey), true, new string[0]));
            }
        }

        private void QuitJob()
        {
            LifeSession session = Context.Session;
            if (session != null && Context.Decisions.QuitJob(session))
            {
                ShowResult(new ActionResultState(Context.Localizer.Get(WorkViewModel.QuitKey), true, new string[0]));
            }
        }
    }
}
