using System;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Careers;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Relationships;
using BetterLife.Core.Saving;
using BetterLife.Core.Scams;
using BetterLife.Core.Vices;

namespace BetterLife.Core.Sessions
{
    /// <summary>
    /// Applies the player's decisions. Every decision first takes an undo checkpoint, so the latest one can be taken back.
    /// </summary>
    public sealed class DecisionService
    {
        private readonly UndoService _undo;
        private readonly int _contentVersion;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="undo">Undo snapshots; null disables checkpoints.</param>
        /// <param name="contentVersion">Version of the content in use.</param>
        public DecisionService(UndoService undo, int contentVersion)
        {
            _undo = undo;
            _contentVersion = contentVersion;
        }

        /// <summary>
        /// Answers a pending event.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="instanceId">Pending event instance id.</param>
        /// <param name="choiceId">Chosen option (must be available).</param>
        /// <returns>The resolution.</returns>
        public EventResolution Answer(LifeSession session, int instanceId, string choiceId)
        {
            Checkpoint(session);
            return session.Events.Resolve(session.Character, instanceId, choiceId);
        }

        /// <summary>
        /// Does something with a person (chat, gift, propose...).
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="npcId">NPC id.</param>
        /// <param name="interactionId">Interaction id.</param>
        /// <returns>The result (not performed when the quote says it is unavailable).</returns>
        public InteractionResult Interact(LifeSession session, int npcId, string interactionId)
        {
            Checkpoint(session);
            return session.Relationships.Interact(npcId, interactionId);
        }

        /// <summary>
        /// Reports a scam to the police and the bank.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="incidentId">Scam incident id.</param>
        /// <returns>The result.</returns>
        public ScamReportResult ReportScam(LifeSession session, int incidentId)
        {
            Checkpoint(session);
            return session.Scams.Report(incidentId);
        }

        /// <summary>
        /// Changes the lifestyle tier (adults only).
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="tierId">Lifestyle tier id.</param>
        public void ChooseLifestyle(LifeSession session, string tierId)
        {
            Checkpoint(session);
            session.Finance.SetLifestyle(tierId);
        }

        /// <summary>
        /// Changes the weekly hours of an activity. Not a checkpointed decision: the player can simply change it back.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="activityId">Activity id (not the default activity).</param>
        /// <param name="hours">New hours.</param>
        /// <returns>True when applied; false when it breaks the activity's maximum or the week.</returns>
        public bool SetHours(LifeSession session, string activityId, int hours)
        {
            RequireRunning(session);
            return session.TimeState.Budget.Set(activityId, hours);
        }

        /// <summary>
        /// Starts quitting a habit with a method.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="viceId">Vice id.</param>
        /// <param name="methodId">Quit method id.</param>
        /// <returns>The quote (unavailable reasons when it did not start).</returns>
        public QuitQuote StartQuit(LifeSession session, string viceId, string methodId)
        {
            Checkpoint(session);
            return session.Vices.StartQuit(viceId, methodId);
        }

        /// <summary>
        /// Applies to a school or university.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="programId">Program id.</param>
        /// <param name="institutionId">Institution id.</param>
        /// <param name="funding">Who pays the tuition.</param>
        /// <returns>The result.</returns>
        public AdmissionResult ApplyEducation(LifeSession session, string programId, string institutionId, TuitionFunding funding)
        {
            Checkpoint(session);
            return session.Education.Apply(programId, institutionId, funding);
        }

        /// <summary>
        /// Leaves school.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <returns>True when the character was enrolled.</returns>
        public bool DropOut(LifeSession session)
        {
            Checkpoint(session);
            return session.Education.DropOut();
        }

        /// <summary>
        /// Applies for a job.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="trackId">Career track id.</param>
        /// <returns>The result.</returns>
        public HiringResult ApplyJob(LifeSession session, string trackId)
        {
            Checkpoint(session);
            return session.Career.Apply(trackId);
        }

        /// <summary>
        /// Quits the current job.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <returns>True when the character had a job.</returns>
        public bool QuitJob(LifeSession session)
        {
            Checkpoint(session);
            return session.Career.Quit();
        }

        /// <summary>
        /// Buys an asset with cash.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="assetId">Asset id.</param>
        /// <param name="amount">Real amount.</param>
        /// <returns>The new holding.</returns>
        public Holding Buy(LifeSession session, string assetId, decimal amount)
        {
            Checkpoint(session);
            return session.Finance.Buy(assetId, amount);
        }

        /// <summary>
        /// Sells part of a holding.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="holdingId">Holding id.</param>
        /// <param name="share">Share to sell (0..1].</param>
        /// <returns>Cash received after fees.</returns>
        public decimal Sell(LifeSession session, int holdingId, decimal share)
        {
            Checkpoint(session);
            return session.Finance.Sell(holdingId, share);
        }

        /// <summary>
        /// Takes a loan.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="productId">Debt product id.</param>
        /// <param name="amount">Real amount.</param>
        /// <returns>The debt.</returns>
        public Debt TakeLoan(LifeSession session, string productId, decimal amount)
        {
            Checkpoint(session);
            return session.Finance.TakeLoan(productId, amount);
        }

        /// <summary>
        /// Opens a credit line (credit card).
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="productId">Revolving debt product id.</param>
        /// <returns>The credit line.</returns>
        public Debt OpenCreditLine(LifeSession session, string productId)
        {
            Checkpoint(session);
            return session.Finance.OpenCreditLine(productId);
        }

        /// <summary>
        /// Pays back part of a debt.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="debtId">Debt id.</param>
        /// <param name="amount">Real amount.</param>
        /// <returns>Amount actually paid.</returns>
        public decimal Repay(LifeSession session, int debtId, decimal amount)
        {
            Checkpoint(session);
            return session.Finance.Repay(debtId, amount);
        }

        private void Checkpoint(LifeSession session)
        {
            RequireRunning(session);
            _undo?.Checkpoint(session, _contentVersion);
        }

        private static void RequireRunning(LifeSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (session.IsOver)
            {
                throw new InvalidOperationException("The life has ended.");
            }
        }
    }
}
