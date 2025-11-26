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
        [Header("Active Ruleset")]
        [SerializeField] private StandardRuleSet standardRuleSet = new StandardRuleSet();

        private IRuleSet active;

        private void Awake()
        {
            //create new StandardRuleSet if inspector doesn't already have on serialzeied
            active ??= standardRuleSet ?? new StandardRuleSet();
        }

        public IRuleSet GetRuleSet()
        {
            if (active == null)
                active = standardRuleSet ?? new StandardRuleSet();

            return active;
        }
    }
}
