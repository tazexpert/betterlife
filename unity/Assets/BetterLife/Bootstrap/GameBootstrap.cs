using System;
using System.Globalization;
using System.IO;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
using BetterLife.Core.Localization;
using BetterLife.Core.Navigation;
using BetterLife.Core.Notices;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using BetterLife.Infrastructure.Content;
using BetterLife.Presentation.Screens;
using BetterLife.Presentation.Shell;
using UnityEngine;

namespace BetterLife.Bootstrap
{
    /// <summary>
    /// Composition root: the only place that creates and wires game objects and services.
    /// It creates itself before the first scene loads, so nothing needs to be placed in a scene or set in the Inspector.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const string HostObjectName = "[BetterLife] GameBootstrap";
        private const string LogPrefix = "[BetterLife] ";
        private const string SaveFolder = "saves";
        // English until the Thai font and the language setting arrive (task 20).
        private const string DefaultLanguage = "en";

        private static GameBootstrap _instance;

        private IEventBus _eventBus;
        private IContentRepository _content;
        private LifeSession _life;
        private SaveService _saves;
        private UndoService _undo;
        private UiRoot _ui;
        private ShellPresenter _shell;
        private ScreenRouter _router;
        private NoticeFeed _notices;

        /// <summary>
        /// Clears static state when domain reload is disabled (Enter Play Mode Options).
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        /// <summary>
        /// Creates the persistent bootstrap host before any scene is loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateHost()
        {
            if (_instance != null)
            {
                return;
            }

            var host = new GameObject(HostObjectName);
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            Compose();
        }

        /// <summary>
        /// Autosaves when the app goes to the background (mobile may kill it without calling quit).
        /// </summary>
        /// <param name="paused">True when pausing.</param>
        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                Autosave();
            }
        }

        private void OnApplicationQuit()
        {
            Autosave();
        }

        private void OnDestroy()
        {
            if (_ui != null)
            {
                _ui.BackPressed -= OnBackPressed;
            }
            _shell?.Dispose();
            _shell = null;
            _life?.Dispose();
            _life = null;
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Builds the object graph: event bus, validated content, saving/undo, the current life and the UI shell.
        /// </summary>
        private void Compose()
        {
            _eventBus = new EventBus();
            _content = LoadContent();
            if (_content == null)
            {
                enabled = false;
                return;
            }

            var storage = new FileSaveStorage(Path.Combine(Application.persistentDataPath, SaveFolder));
            var serializer = new SaveSerializer();
            _saves = new SaveService(storage, serializer, _content);
            _undo = new UndoService(serializer, storage, new SystemRealTimeClock());

            // Until the slot and creation screens exist (UI tasks), continue the autosave or start a new random life.
            _life = TryLoadAutosave() ?? StartNewLife(new CreationRequest(NewSeed(), CreationMode.Random));
            ComposeUi();
        }

        /// <summary>
        /// Creates the UI: localizer, router, screen registry, the UI Toolkit panel and the shell presenter.
        /// Screens register here as they are built; an unregistered screen shows a placeholder.
        /// </summary>
        private void ComposeUi()
        {
            var localizer = new TableLocalizer(_content.Get<StringTables>(), DefaultLanguage, DefaultLanguage);
            _router = new ScreenRouter(ShellTabs.All);
            _notices = new NoticeFeed();
            var context = new ScreenContext(() => _life, localizer, new MoneyFormatter(localizer), _router, _notices,
                new DecisionService(_undo, _content.ContentVersion));

            var screens = new ScreenRegistry(localizer);
            screens.Register(ScreenIds.Home, () => new DashboardScreenPresenter(context));
            screens.Register(ScreenIds.Inbox, () => new InboxScreenPresenter(context));
            screens.Register(ScreenIds.EventDialog, () => new EventDialogPresenter(context));
            screens.Register(ScreenIds.Breakdown, () => new BreakdownPresenter(context));
            screens.Register(ScreenIds.Money, () => new FinanceScreenPresenter(context));
            screens.Register(ScreenIds.People, () => new PeopleScreenPresenter(context));
            screens.Register(ScreenIds.Person, () => new PersonScreenPresenter(context));
            screens.Register(ScreenIds.More, () => new MoreScreenPresenter(context));
            screens.Register(ScreenIds.Astrology, () => new AstrologyScreenPresenter(context));
            screens.Register(ScreenIds.Timeline, () => new TimelineScreenPresenter(context));
            screens.Register(ScreenIds.Life, () => new LifePlanScreenPresenter(context));
            screens.Register(ScreenIds.Work, () => new WorkScreenPresenter(context));
            screens.Register(ScreenIds.Invest, () => new InvestScreenPresenter(context));

            _ui = UiRoot.Create(transform);
            _ui.BackPressed += OnBackPressed;
            _shell = new ShellPresenter(_ui.Root, screens, context);
            _shell.TimeAdvanced += OnTimeAdvanced;
            _shell.Bind(_life);
        }

        private void OnBackPressed()
        {
            if (_shell != null && _shell.Back() == BackResult.Exit)
            {
                Autosave();
                Application.Quit();
            }
        }

        private void OnTimeAdvanced(AdvanceResult result)
        {
            _notices.Add(result, _life.Events.Inbox);
            int? decision = NoticeFeed.StoppingDecision(result, _life.Events.Inbox);
            if (decision.HasValue)
            {
                _router.OpenModal(new ScreenRequest(ScreenIds.EventDialog, decision.Value.ToString(CultureInfo.InvariantCulture)));
            }
            if (result.WasInterrupted)
            {
                Debug.Log(LogPrefix + "Time stopped at " + result.To + ": " + result.StoppedBy.ReasonKey);
            }
        }

        /// <summary>
        /// Creates a life and composes all of its systems.
        /// </summary>
        /// <param name="request">Creation request.</param>
        /// <returns>The running life.</returns>
        private LifeSession StartNewLife(CreationRequest request)
        {
            _life?.Dispose();
            _undo?.Clear();
            _notices?.Clear();
            CreationResult creation = new CharacterCreator(_content).Create(request);
            LifeSession life = LifeSessionFactory.CreateNew(_content, creation, _eventBus);
            Debug.Log(LogPrefix + "New life " + creation.ChallengeCode + ": " + life.Character.Name.GivenName + " " + life.Character.Name.FamilyName
                + ", born " + life.Character.Birth.Date + " in " + life.Character.Residence.CityId + ".");
            return life;
        }

        /// <summary>
        /// Loads the autosave if there is a readable one.
        /// </summary>
        /// <returns>The life, or null.</returns>
        private LifeSession TryLoadAutosave()
        {
            try
            {
                LifeSessionLoadResult result = _saves.Load(SlotNames.Autosave, _eventBus);
                foreach (string warning in result.Warnings)
                {
                    Debug.LogWarning(LogPrefix + "Save: " + warning);
                }
                Debug.Log(LogPrefix + "Continued " + result.Session.Character.Name.GivenName + " from the autosave.");
                return result.Session;
            }
            catch (SaveLoadException ex)
            {
                if (ex.Kind != SaveErrorKind.Missing)
                {
                    Debug.LogWarning(LogPrefix + "Autosave not loaded (" + ex.Kind + "): " + ex.Message);
                }
                return null;
            }
        }

        /// <summary>
        /// Saves the current life to the autosave slot; failures are logged, never thrown.
        /// </summary>
        private void Autosave()
        {
            if (_life == null || _saves == null)
            {
                return;
            }
            try
            {
                _saves.Autosave(_life);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError(LogPrefix + "Autosave failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns a fresh life seed. The clock is only the entropy source for the seed; all game randomness then
        /// comes from the seeded PCG32 streams.
        /// </summary>
        /// <returns>A seed.</returns>
        private static ulong NewSeed()
        {
            return unchecked((ulong)DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Loads and validates all content. On failure every error is logged and null is returned.
        /// </summary>
        /// <returns>The repository, or null.</returns>
        private static IContentRepository LoadContent()
        {
            ContentLoadResult result = DefaultContent.CreatePipeline(ContentSourceFactory.CreateForCurrentPlatform()).Load();
            if (result.Succeeded)
            {
                Debug.Log(LogPrefix + "Content v" + result.Repository.ContentVersion + " loaded.");
                return result.Repository;
            }

            foreach (ContentError error in result.Errors)
            {
                Debug.LogError(LogPrefix + "Content error: " + error);
            }
            return null;
        }
    }
}
