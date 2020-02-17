namespace Reactics.Battle
{
    public struct Proficiency
    {

        public string identifier;
        public ProficiencyLevel level;
        public bool IsProgressable() {
            return (int) level > 0;
        }
    }
    public enum ProficiencyLevel
    {
        FIXED = -1, F = 0, D = 1, C = 2, B = 3, A = 4, S = 5
    }
}