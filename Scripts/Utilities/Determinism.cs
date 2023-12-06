using UnityEngine;

namespace Utilities
{
    public static class Determinism
    {
        private static Random.State _nonDeterministicState;

        public static Random.State GetSeed(int seed)
        {
            _nonDeterministicState = Random.state;
            Random.InitState(seed);
            var resultSeed = Random.state;
            Random.state = _nonDeterministicState;
            return resultSeed;
        }
        
        public static void Begin(Random.State state)
        {
            _nonDeterministicState = Random.state;
            Random.state = state;
        }

        public static void End(out Random.State state)
        {
            state = Random.state;
            Random.state = _nonDeterministicState;
        }
    }
}