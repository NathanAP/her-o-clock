using System.Collections;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Stages;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HerOClock.Tests.PlayMode
{
    /// <summary>
    /// The one test that needs the engine actually running.
    ///
    /// Everything about combat is checked in Edit Mode, where it is faster and does not need a
    /// scene. What cannot be checked there is the scene assembling itself: a missing asset
    /// reference, a renamed sorting layer, a component that throws on Start. Unity reports none
    /// of those as failures. The game simply sits there with a clean Console, which is the exact
    /// failure the bootstrap's own validation messages were written for.
    ///
    /// So this asserts the cheap, broad thing: the scene loads, the board and the characters get
    /// built, and the fight starts.
    ///
    /// Nothing here checks the Console explicitly, and that is on purpose. The Test Runner already
    /// fails a test on its own when an Error or an Exception is logged while it runs, which is
    /// exactly the net that is wanted. The stricter `LogAssert.NoUnexpectedReceived` is the wrong
    /// tool: it demands that **every** message, plain `Debug.Log` included, be declared in advance
    /// with `LogAssert.Expect`. The bootstrap reports what it built and which seed it drew, and
    /// those messages are the point of it, not noise to be silenced one by one.
    /// </summary>
    public class BootstrapSmokeTests
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";

        /// <summary>
        /// How long to keep looking for both sides on the board.
        ///
        /// It watches over a window instead of checking one instant, and that is not slack: the
        /// game plays itself, so what is on the board at any given second depends on how strong
        /// the party is. It used to look exactly three seconds in and passed only because the
        /// heroes were slow enough. Once a save existed, a party that came back from an absence a
        /// few levels up cleared the first wave before the test looked, and the scene assembling
        /// itself — the only thing this test is about — was reported as broken.
        /// </summary>
        private const float LookForSeconds = 15f;

        [UnityTest]
        public IEnumerator TheSceneBuildsItselfAndStartsFighting()
        {
            yield return LoadScene();

            bool anyHero = false;
            bool anyEnemy = false;

            for (float waited = 0f; waited < LookForSeconds && !(anyHero && anyEnemy); waited += Time.deltaTime)
            {
                Character[] characters = Object.FindObjectsByType<Character>();

                for (int i = 0; i < characters.Length; i++)
                {
                    if (characters[i].Team == Team.Heroes)
                    {
                        anyHero = true;
                    }
                    else
                    {
                        anyEnemy = true;
                    }
                }

                yield return null;
            }

            Assert.IsTrue(anyHero, "The heroes were never spawned.");
            Assert.IsTrue(anyEnemy, "No wave was ever spawned.");

            Assert.IsNotNull(Object.FindAnyObjectByType<StageRunner>(), "No StageRunner is running the stage.");
            Assert.IsNotNull(Object.FindAnyObjectByType<BattleDirector>(), "No BattleDirector was created.");
        }

        /// <summary>
        /// The board has to be drawn. A sorting layer renamed out from under the renderer would
        /// leave the cells built but invisible, with nothing in the Console.
        /// </summary>
        [UnityTest]
        public IEnumerator TheBoardIsDrawnOnTheExpectedSortingLayers()
        {
            yield return LoadScene();
            yield return null;

            SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>();
            Assert.Greater(renderers.Length, 0, "Nothing at all was drawn.");

            bool background = false;
            bool characters = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].sortingLayerName == "Background")
                {
                    background = true;
                }

                if (renderers[i].sortingLayerName == "Characters")
                {
                    characters = true;
                }
            }

            Assert.IsTrue(background, "No cell was drawn on the Background sorting layer.");
            Assert.IsTrue(characters, "No character was drawn on the Characters sorting layer.");
        }

        private static IEnumerator LoadScene()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
#endif
        }
    }
}
