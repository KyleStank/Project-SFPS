namespace ProjectSFPS.Character
{
    public struct CharacterInput
    {
        public float Horizontal { get; set; }
        public float Vertical { get; set; }

        public CharacterInput(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }
    }
}
