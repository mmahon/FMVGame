using System;
using System.Collections.Generic;
using System.Linq;

namespace FMV.Data
{
    /// <summary>
    /// Decides which choices the player is shown for a scene.
    /// </summary>
    public static class FMVChoiceSelector
    {
        public const int MaxChoices = 3;

        /// <summary>
        /// Valid choices are ranked (choices with conditions first, then unconditional ones, each in
        /// file order), the top <paramref name="max"/> are kept, and those are shuffled for display.
        /// Pass a <paramref name="shuffleKey"/> to make the shuffle deterministic in tests.
        /// </summary>
        public static List<FMVChoice> SelectForDisplay(IEnumerable<FMVChoice> choices, int max = MaxChoices, Func<FMVChoice, double> shuffleKey = null)
        {
            if (shuffleKey == null)
            {
                var rng = new Random();
                shuffleKey = _ => rng.NextDouble();
            }

            // OrderBy is stable, so file order is preserved within each group.
            return choices
                .Where(c => c.IsAvailable())
                .OrderBy(c => c.Conditions != null && c.Conditions.Count > 0 ? 0 : 1)
                .Take(max)
                .OrderBy(shuffleKey)
                .ToList();
        }
    }
}
