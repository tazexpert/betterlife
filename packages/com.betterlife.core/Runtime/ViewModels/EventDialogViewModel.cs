using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the event dialog (question and choices) and the result shown after answering.
    /// </summary>
    public static class EventDialogViewModel
    {
        /// <summary>Chance text key (placeholder <c>{value}</c>).</summary>
        public const string ChanceKey = "event.chance";
        /// <summary>Locked choice text.</summary>
        public const string LockedKey = "event.locked";
        /// <summary>Roll summary on success (placeholder <c>{value}</c> = chance).</summary>
        public const string RollSuccessKey = "event.roll.success";
        /// <summary>Roll summary on failure (placeholder <c>{value}</c> = chance).</summary>
        public const string RollFailureKey = "event.roll.failure";
        /// <summary>Format of chances.</summary>
        public const string ChanceFormat = "0";

        /// <summary>
        /// Builds the dialog of a pending event.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="instanceId">Pending event instance id.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state, or null when the event is no longer pending.</returns>
        public static EventDialogState Build(LifeSession session, int instanceId, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            PendingEvent pending = session.Events.State.FindPending(instanceId);
            if (pending == null || !session.Events.Events.TryGet(pending.EventId, out LifeEventDefinition definition))
            {
                return null;
            }
            GameDate now = session.TimeState.Clock.Now;
            RuleTextFormatter rules = Formatter(session, localizer, money);
            List<ChoiceState> choices = session.Events.GetChoices(session.Character, instanceId)
                .Where(c => c.IsVisible)
                .Select(c => new ChoiceState(
                    c.Choice.Id,
                    localizer.Get(c.Choice.TextKey),
                    c.IsAvailable,
                    c.Chance == null ? null : localizer.Format(ChanceKey, ("value", c.Chance.Final.ToString(ChanceFormat, CultureInfo.InvariantCulture))),
                    rules.FormatAll(c.SuccessPreview),
                    rules.FormatAll(c.FailurePreview),
                    c.IsAvailable ? null : localizer.Get(LockedKey)))
                .ToList();
            return new EventDialogState(instanceId, localizer.Get(definition.TitleKey), localizer.Get(definition.BodyKey),
                InboxViewModel.Deadline(pending, now, localizer), choices);
        }

        /// <summary>
        /// Builds the result of an answered event.
        /// </summary>
        /// <param name="session">Running life (for today's prices).</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static EventResultState BuildResult(LifeSession session, EventResolution resolution, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            string roll = null;
            if (resolution.Roll != null)
            {
                string chance = resolution.Roll.Breakdown.Final.ToString(ChanceFormat, CultureInfo.InvariantCulture);
                roll = localizer.Format(resolution.Succeeded ? RollSuccessKey : RollFailureKey, ("value", chance));
            }
            string result = resolution.Outcome == null ? string.Empty : localizer.Get(resolution.Outcome.ResultKey);
            IReadOnlyList<string> lines = Formatter(session, localizer, money).FormatAll(resolution.Result.Lines);
            return new EventResultState(localizer.Get(resolution.Definition.TitleKey), result, resolution.Succeeded, roll, lines);
        }

        private static RuleTextFormatter Formatter(LifeSession session, ILocalizer localizer, MoneyFormatter money)
        {
            return new RuleTextFormatter(localizer, money ?? new MoneyFormatter(localizer), session.Economy.State.Nominal);
        }
    }
}
