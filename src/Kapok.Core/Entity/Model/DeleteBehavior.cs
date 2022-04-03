namespace Kapok.Entity.Model;

public enum DeleteBehavior
{
    NoAction = 0,
    SetNull,
    Restrict,
    Cascade
}