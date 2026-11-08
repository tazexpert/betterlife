using System;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
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

        private static GameBootstrap _instance;

        private IEventBus _eventBus;
        private IContentRepository _content;
        private LifeSession _life;

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
        /// Builds the object graph: event bus, validated content and the current life.
        /// Infrastructure services and presenters are added in later tasks.
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

            // Until the creation screen exists (UI tasks), every launch starts a new random life.
            _life = StartNewLife(new CreationRequest(NewSeed(), CreationMode.Random));
        }

        /// <summary>
        /// Creates a life and composes all of its systems.
        /// </summary>
        /// <param name="request">Creation request.</param>
        /// <returns>The running life.</returns>
        private LifeSession StartNewLife(CreationRequest request)
        {
            _life?.Dispose();
            CreationResult creation = new CharacterCreator(_content).Create(request);
            LifeSession life = LifeSessionFactory.CreateNew(_content, creation, _eventBus);
            Debug.Log(LogPrefix + "New life " + creation.ChallengeCode + ": " + life.Character.Name.GivenName + " " + life.Character.Name.FamilyName
                + ", born " + life.Character.Birth.Date + " in " + life.Character.Residence.CityId + ".");
            return life;
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
