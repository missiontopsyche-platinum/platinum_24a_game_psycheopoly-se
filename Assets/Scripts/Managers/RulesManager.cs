using UnityEngine;
using Assets.Scripts.Managers.Rules;

namespace Assets.Scripts.Managers
{
    /// <summary>
    /// keeps a ref to the active ruleset for now
    /// this is mostly still just a placeholder for a future task
    /// </summary>
    public class RulesManager : MonoBehaviour
    {
        [SerializeField] private StandardRuleSet standardRuleSet = new StandardRuleSet();

        // Currently always returns standard rules.
        // Later tasks may add alternate rule sets.
        public IRuleSet GetRuleSet()
        {
            return standardRuleSet;
        }
    }
}
