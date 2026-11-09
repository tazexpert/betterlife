using System;
using System.IO;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Infrastructure.Content;
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

        private static GameBootstrap _instance;

        private IEventBus _eventBus;
        private IContentRepository _content;
        private LifeSession _life;
        private SaveService _saves;
        private UndoService _undo;

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
            _life?.Dispose();
            _life = null;
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Builds the object graph: event bus, validated content, saving/undo and the current life.
        /// Presenters are added in later tasks.
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
