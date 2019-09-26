using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MLAgents
{
    /// <summary>
    /// The Heuristic Brain type allows you to hand code an Agent's decision making process.
    /// A Heuristic Brain requires an implementation of the Decision interface to which it
    /// delegates the decision making process.
    /// When yusing a Heuristic Brain, you must give it a Monoscript of a Decision implementation.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHeuristicBrain", menuName = "ML-Agents/Heuristic Brain")]
    public class HeuristicBrain : Brain
    {
        [SerializeField]
        [HideInInspector]
        public Decision decision;
#if UNITY_EDITOR
        [HideInInspector]
        public MonoScript decisionScript;
#endif
        [FormerlySerializedAs("c_decision")]
        [SerializeField]
        [HideInInspector]
        public string cDecision;

        public void OnValidate()
        {
#if UNITY_EDITOR
            if (decisionScript != null)
            {
                cDecision = decisionScript.GetClass().Name;
            }
            else
            {
                cDecision = "";
            }
#endif
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            if ((cDecision != null) && decision == null)
            {
                decision = CreateInstance(cDecision) as Decision;
                decision.brainParameters = brainParameters;
            }
        }

        ///Uses the Decision Component to decide that action to take
        protected override void DecideAction()
        {
            if (decision == null)
            {
                throw new UnityAgentsException(
                    "The Brain is set to Heuristic, but no decision script attached to it");
            }
            foreach (var agent in m_Agents)
            {
                var info = agent.Info;
                agent.UpdateVectorAction(decision.Decide(
                    info.stackedVectorObservation,
                    info.visualObservations,
                    info.reward,
                    info.done,
                    info.memories));
            }
            foreach (var agent in m_Agents)
            {
                var info = agent.Info;
                agent.UpdateMemoriesAction(decision.MakeMemory(
                    info.stackedVectorObservation,
                    info.visualObservations,
                    info.reward,
                    info.done,
                    info.memories));
            }
            m_Agents.Clear();
        }
    }
}
