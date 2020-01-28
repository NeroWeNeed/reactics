namespace Reactics.Battle
{
    public interface IStats : Reactics.Util.IMutableExchangeable<IStats>
    {
        int MaxHealthPoints { get; }
        int MaxMagicPoints { get; }
        int Strength { get; }

        int Magic { get; }
        int Defense { get; }

        int Resistance { get; }

        int Speed { get; }

        int Movement { get; }


    }

    public struct ImmutableStats : IStats
    {

        private readonly int maxHealthPoints;
        public int MaxHealthPoints => maxHealthPoints;

        private readonly int maxMagicPoints;
        public int MaxMagicPoints => maxMagicPoints;

        private readonly int strength;
        public int Strength => strength;

        private readonly int magic;
        public int Magic => magic;

        private readonly int defense;

        public int Defense => defense;

        private readonly int resistance;
        public int Resistance => resistance;



        private readonly int speed;
        public int Speed => speed;

        private readonly int movement;

        public int Movement => movement;

        public ImmutableStats(int maxHealthPoints = 0, int maxMagicPoints = 0, int strength = 0, int magic = 0, int defense = 0, int resistance = 0, int speed = 0, int movement = 0)
        {
            this.maxHealthPoints = maxHealthPoints;
            this.maxMagicPoints = maxMagicPoints;
            this.strength = strength;
            this.magic = magic;
            this.defense = defense;
            this.resistance = resistance;
            this.speed = speed;
            this.movement = movement;
        }
        public ImmutableStats(IStats stats) : this(stats.MaxHealthPoints, stats.MaxMagicPoints, stats.Strength, stats.Magic, stats.Defense, stats.Resistance, stats.Speed, stats.Movement) { }

        public IStats AsImmutable() => this;

        public IStats AsMutable()
        {
            throw new System.NotImplementedException();
        }
    }

    public class MutableStats : IStats
    {


        public int MaxHealthPoints { get; set; }

        public int MaxMagicPoints { get; set; }

        public int Strength { get; set; }

        public int Magic { get; set; }

        public int Defense { get; set; }

        public int Resistance { get; set; }

        public int Speed { get; set; }

        public int Movement { get; set; }

        public MutableStats(int maxHealthPoints, int maxMagicPoints, int strength, int magic, int defense, int resistance, int speed, int movement)
        {
            MaxHealthPoints = maxHealthPoints;
            MaxMagicPoints = maxMagicPoints;
            Strength = strength;
            Magic = magic;
            Defense = defense;
            Resistance = resistance;
            Speed = speed;
            Movement = movement;
        }

        public MutableStats(IStats stats) : this(stats.MaxHealthPoints, stats.MaxMagicPoints, stats.Strength, stats.Magic, stats.Defense, stats.Resistance, stats.Speed, stats.Movement) { }


        public IStats AsImmutable() => new ImmutableStats(this);

        public IStats AsMutable() => new MutableStats(this);
    }

}