using Nuthouse.Services;
using UnityEngine;

namespace Nuthouse.Core
{
    public sealed class GameBootstrap : PersistentSingleton<GameBootstrap>
    {
        protected override void OnAwake()
        {
            ServiceLocator.Register<ISceneService>(new SceneService());
            ServiceLocator.Register<IProgressService>(new ProgressService());
        }
    }
}
