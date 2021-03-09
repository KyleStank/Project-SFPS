namespace ProjectSFPS.Character
{
    public interface ICharacterControl
    {
        CharacterInput CharacterInput { get; }

        CharacterInput ReadInput();
    }
}
