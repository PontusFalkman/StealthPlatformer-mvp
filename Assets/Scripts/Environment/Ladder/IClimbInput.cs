public interface IClimbInput
{
    float Vertical { get; }   // -1..1, uses Move.y
    bool ClimbHeld { get; }   // Up held -> true
}
