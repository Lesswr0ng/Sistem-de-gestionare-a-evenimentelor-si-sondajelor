namespace EventsAndPolls.Domain.Interfaces;

public interface IPrototype<T>
{
     T Clone();
     T DeepClone();
}
