namespace KhaleefWorld
{
    public enum Subject { Maths, Language, Science, History, Logic }

    public enum Difficulty { Easy, Medium, Hard }

    public enum CosmeticSlot { Head, Top, Bottom, Shoes, Accessory, Back, Pet }

    public enum UnlockType { Free, Coins, Badge }

    public enum TravelMode { Plane, Train, Ferry, Car }

    public enum MiniGameTemplate { MathsClimb, MemoryMatch, WauKite }

    public enum MathMode { NextNumber, SkipCount, Addition, Subtraction, Multiplication, Money }

    public enum ItemShape
    {
        None,
        // Head
        Songkok, Tudung, Tengkolok, Cap, Helmet, ChefHat,
        // Accessory
        Glasses, Necklace, Wristband,
        // Back
        Backpack, Wings, Cape, Jetpack, WauKite,
        // Bottom
        Samping, LongSkirt,
        // Pets
        PetCat, PetTurtle, PetHornbill, PetTapir, PetOrangutan
    }

    public enum QuestType { PlayMiniGame, PlayMaths, PlayLanguage, PlayScience, PlayHistory, CollectCoins, EarnStars, VisitState, WearTraditional }
}
