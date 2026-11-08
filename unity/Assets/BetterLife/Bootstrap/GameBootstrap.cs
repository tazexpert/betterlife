using BetterLife.Core.Content;
using BetterLife.Core.Events;
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
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Builds the object graph. Systems, infrastructure services and presenters are registered here
        /// in later milestones.
        /// </summary>
        private void Compose()
        {
            _eventBus = new EventBus();
            _content = LoadContent();
            if (_content == null)
            {
                enabled = false;
            }
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
