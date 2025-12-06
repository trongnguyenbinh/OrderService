namespace Domain.Contractors;

public interface IEntity<T>
{
    T Id { get; set; }
}